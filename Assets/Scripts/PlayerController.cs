using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public struct PlayerMoveData
{
    public Vector2Int moveDir; // 움직인 방향 ((0, 1), (0, -1), (1, 0), (-1, 0))
    public TileColor prevMyColor; // 현재 타일에 도착하기 전 플레이어의 색
    public TileColor prevTileColor; // 플레이어가 색칠하기 전 타일색

    public PlayerMoveData(Vector2Int moveDir, TileColor prevMyColor, TileColor prevTileColor)
    {
        this.moveDir = moveDir;
        this.prevMyColor = prevMyColor;
        this.prevTileColor = prevTileColor;
    }
}

public class PlayerController : SingletonBehaviour<PlayerController>
{
    [Tooltip("타일 한 칸을 건너는 데 걸리는 시간")]
    public float moveTimePerTile;
    [Header("플레이어의 색")]
    public Color gray;
    public Color red, blue;

    public TextMeshProUGUI MoveCountText;
    public ParticleSystem particle;
    private TileColor myColor;
    private int curI, curJ; // 현재 플레이어가 위치한 좌표
    private int destI, destJ; // 최종 이동 장소로서 예약된 좌표 (플레이어 이동 중에는 현재좌표 != 도착좌표)
    private Queue<Vector2Int> moveToQueue = new Queue<Vector2Int>(); // 한 번에 여러 칸 이동하기 위한 큐
    private bool isMoving; // 현재 이동 중인지
    private int moveCount;
    private LinkedList<PlayerMoveData> moveDataListToRedo = new LinkedList<PlayerMoveData>(); // 되돌리기를 위해 그 동안의 이동 데이터를 기록한 큐
    private float lastRedoTime = 0f;

    private float keyboardNextFireTime = 0f;

    // 캐싱
    private SpriteRenderer spriter;
    private Transform player;

    private void Awake()
    {
        m_IsDestroyOnLoad = true;
        Init();
        spriter = transform.GetChild(0).GetComponent<SpriteRenderer>();
        player = transform.GetChild(0).transform;
    }

#if UNITY_STANDALONE   // Windows, macOS, Linux
    private void Update()
    {
        // 화살표 방향 입력 (좌우/상하 합쳐서 Vector2)
        Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if ((dir.x == 0 && dir.y != 0) || (dir.x != 0 && dir.y == 0)) // 방향키 눌림
        {
            if (Time.time >= keyboardNextFireTime)
            {
                TryMoveTo(destI + (int)dir.x, destJ + (int)dir.y);
                keyboardNextFireTime = Time.time + moveTimePerTile;
            }
        }
        else
        {
            keyboardNextFireTime = 0f; // 키 뗐을 때 초기화
        }

        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(Redo());
    }
#endif

    public void InitPlayer(BoardSO boardSO)
    {
        ChangeColor(TileColor.None);
        curI = destI = boardSO.startPos.x;
        curJ = destJ = boardSO.startPos.y;
        transform.position = Board.Instance.GetTilePos(curI, curJ);
        SetMoveCount(0);
    }

    // (i, j) 좌표로 이동 시도
    public void TryMoveTo(int i, int j)
    {
        if (!CanMoveTo(i, j) || !GameManager.Instance.isGaming) return;

        // 같은 행, 열 중 어디인지에 따라 분기
        if (destI == i) // j 방향(가로 이동)
        {
            int step = (j > destJ) ? 1 : -1;
            for (int col = destJ + step; col != j + step; col += step)
                moveToQueue.Enqueue(new Vector2Int(destI, col));
        }
        else if (destJ == j) // i 방향(세로 이동)
        {
            int step = (i > destI) ? 1 : -1;
            for (int row = destI + step; row != i + step; row += step)
                moveToQueue.Enqueue(new Vector2Int(row, destJ));
        }

        destI = i;
        destJ = j;

        // 이동 중이 아니라면 이동 시작
        if (!isMoving) StartCoroutine(Move());
    }

    // 플레이어가 (destI, destJ) => (i, j)로 갈 수 있는지 검사
    private bool CanMoveTo(int i, int j)
    {
        if (!Board.Instance.IsInBounds(i, j)) return false; // 범위 바깥
        if (destI != i && destJ != j) return false; // 대각선 방향은 이동X
        if (!Board.Instance.tileSet.Contains(new Vector2Int(i, j))) return false; // 타일이 없으면 이동x

        // // 플레이어와 같은 색의 타일은 이동 불가능
        // TileColor playerColor = myColor;
        // TileColor changeColor;

        // 같은 행, 열 중 어디인지에 따라 분기
        if (destI == i) // j 방향(가로 이동)
        {
            int step = (j > destJ) ? 1 : -1;
            for (int col = destJ + step; col != j + step; col += step)
            {
                // changeColor = Board.Instance.IsChangeTile(i, col);
                // if (changeColor != TileColor.None) // 색 바꾸는 타일
                //     playerColor = changeColor;
                // else if (playerColor != TileColor.None && Board.Instance.HasColor(i, col, playerColor))
                //     return false;

                if (!Board.Instance.tileSet.Contains(new Vector2Int(i, col)))
                    return false;
            }
        }
        else if (destJ == j) // i 방향(세로 이동)
        {
            int step = (i > destI) ? 1 : -1;
            for (int row = destI + step; row != i + step; row += step)
            {
                // changeColor = Board.Instance.IsChangeTile(row, j);
                // if (changeColor != TileColor.None) // 색 바꾸는 타일
                //     playerColor = changeColor;
                // else if (playerColor != TileColor.None && Board.Instance.HasColor(row, j, playerColor))
                //     return false;

                if (!Board.Instance.tileSet.Contains(new Vector2Int(row, j)))
                    return false;
            }
        }

        return true;
    }

    private IEnumerator Move()
    {
        isMoving = true;
        TileColor changeColor;

        while (moveToQueue.Count > 0 && GameManager.Instance.isGaming)
        {
            IncreaseMoveCount();
            float moveTime = moveTimePerTile * Mathf.Lerp(1, 0.8f, Mathf.InverseLerp(1, 10, moveToQueue.Count));
            Vector2Int nextPos = moveToQueue.Dequeue(); // 큐에서 하나 꺼내기
            PlayerMoveAnimation(nextPos, moveTime);
            yield return new WaitForSeconds(moveTime / 2f);

            // 타일 색칠 or 플레이어 색 변화
            moveDataListToRedo.AddLast(new PlayerMoveData(nextPos - new Vector2Int(curI, curJ), 
                                                          myColor, Board.Instance.board[nextPos.x, nextPos.y]));
            if (moveDataListToRedo.Count >= 1000)
                moveDataListToRedo.RemoveFirst();

            curI = nextPos.x; curJ = nextPos.y;
            changeColor = Board.Instance.IsChangeTile(nextPos.x, nextPos.y);
            if (changeColor != TileColor.None) // 색 바꾸는 타일
                ChangeColor(changeColor);
            else if (myColor != TileColor.None) // 타일 색칠
                Board.Instance.ColorTile(nextPos.x, nextPos.y, myColor);
            if (Board.Instance.IsClear())
                GameManager.Instance.GameClear();
            
            yield return new WaitForSeconds(moveTime / 2f);
        }

        player.DOScale(1, moveTimePerTile / 1.2f);
        isMoving = false;
    }

    private void PlayerMoveAnimation(Vector2Int nextPos, float moveTime)
    {
        Vector2 nextRealPos = Board.Instance.GetTilePos(nextPos.x, nextPos.y);
        transform.DOMove(nextRealPos, moveTime).SetEase(Ease.Linear);        
        if (curI == nextPos.x)
        {// 세로 이동 
            player.DOScale(new Vector2(0.85f, 1.15f), moveTimePerTile / 1.2f);
            particle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else
        {
            player.DOScale(new Vector2(1.15f, 0.85f), moveTimePerTile / 1.2f);
            particle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }
        particle.Emit(4);
    }

    private void ChangeColor(TileColor changeColor)
    {
        myColor = changeColor;
        var main = particle.main;
        switch (changeColor)
        {
            case TileColor.None:
                spriter.color = gray;
                main.startColor = gray;
                break;
            case TileColor.Color1:
                spriter.color = Board.Instance.colorPallete.color1;
                main.startColor = Board.Instance.colorPallete.color1;
                break;
            case TileColor.Color2:
                spriter.color = Board.Instance.colorPallete.color2;
                main.startColor = Board.Instance.colorPallete.color2;
                break;
        }
    }

    private void SetMoveCount(int n)
    {
        moveCount = n;
        MoveCountText.text = moveCount.ToString();
    }

    private void IncreaseMoveCount()
    {
        moveCount++;
        MoveCountText.text = moveCount.ToString();
    }

    private void DecreaseMoveCount()
    {
        moveCount--;
        MoveCountText.text = moveCount.ToString();
    }

    private IEnumerator Redo()
    {
        if (Time.time < lastRedoTime + moveTimePerTile || moveCount <= 0)
            yield break;    
        lastRedoTime = Time.time;

        DecreaseMoveCount();
        PlayerMoveData moveData = moveDataListToRedo.Last.Value;
        moveDataListToRedo.RemoveLast();
        destI -= moveData.moveDir.x;
        destJ -= moveData.moveDir.y;
        PlayerMoveAnimation(new Vector2Int(destI, destJ), moveTimePerTile);
        yield return new WaitForSeconds(moveTimePerTile / 2f);

        if (myColor != moveData.prevMyColor)
            ChangeColor(moveData.prevMyColor);
        Board.Instance.ColorTile(curI, curJ, moveData.prevTileColor, false);
        curI = destI; curJ = destJ;

        yield return new WaitForSeconds(moveTimePerTile / 2f);
        player.DOScale(1, moveTimePerTile / 1.2f);
    }
}