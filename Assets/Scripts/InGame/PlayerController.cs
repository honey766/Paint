using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public struct PlayerMoveData
{
    public Vector2Int moveDir; // 움직인 방향 ((0, 1), (0, -1), (1, 0), (-1, 0))
    public TileType prevMyColor; // 현재 타일에 도착하기 전 플레이어의 색
    public TileType prevTileColor; // 플레이어가 색칠하기 전 타일색

    public PlayerMoveData(Vector2Int moveDir, TileType prevMyColor, TileType prevTileColor)
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
    public Color white;
    public Color black;

    public TextMeshProUGUI MoveCountText;
    public ParticleSystem particle;
    public TileType myColor { get; private set; }
    private int curI, curJ; // 현재 플레이어가 위치한 좌표
    private int destI, destJ; // 최종 이동 장소로서 예약된 좌표 (플레이어 이동 중에는 현재좌표 != 도착좌표)
    private Queue<Vector2Int> moveToQueue = new Queue<Vector2Int>(); // 한 번에 여러 칸 이동하기 위해 다음에 이동할 타일을 나열한 큐
    private bool isMoving; // 현재 이동 중인지
    private int moveCount;
    private LinkedList<PlayerMoveData> moveDataListToRedo = new LinkedList<PlayerMoveData>(); // 되돌리기를 위해 그동안의 이동 데이터를 기록한 양방향 리스트
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

        // if (Input.GetKey(KeyCode.Space))
        //     StartCoroutine(Redo());
    }
#endif

    public void InitPlayer(BoardSO boardSO)
    {
        ChangeColor(TileType.None);
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
        if (!Board.Instance.board.ContainsKey(new Vector2Int(i, j))) return false; // 타일이 없으면 이동x

        // // 플레이어와 같은 색의 타일은 이동 불가능
        // TileColor playerColor = myColor;
        // TileColor changeColor;

        // 같은 행, 열 중 어디인지에 따라 분기
        if (destI == i) // j 방향(가로 이동)
        {
            int step = (j > destJ) ? 1 : -1;
            for (int col = destJ + step; col != j + step; col += step)
            {
                if (!Board.Instance.board.ContainsKey(new Vector2Int(i, col)))
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

                if (!Board.Instance.board.ContainsKey(new Vector2Int(row, j)))
                    return false;
            }
        }

        return true;
    }

    private IEnumerator Move()
    {
        isMoving = true;

        while (moveToQueue.Count > 0 && GameManager.Instance.isGaming)
        {
            IncreaseMoveCount();
            float moveTime = moveTimePerTile * Mathf.Lerp(1, 0.8f, Mathf.InverseLerp(1, 10, moveToQueue.Count));
            Vector2Int nextPos = moveToQueue.Dequeue(); // 큐에서 하나 꺼내기
            PlayerMoveAnimation(nextPos, moveTime);
            Board.Instance.board[nextPos].OnPlayerEnter(this, moveTime);
            yield return new WaitForSeconds(moveTime / 2f);

            // 타일 색칠 or 플레이어 색 변화
            curI = nextPos.x; curJ = nextPos.y;
            if (Board.Instance.IsGameClear())
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

    public void ChangeColor(TileType changeColor)
    {
        myColor = changeColor;
        var main = particle.main;

        ColorPaletteSO colorPalette;
        if (GameManager.Instance.startGameDirectlyAtInGameScene)
            colorPalette = Board.Instance.colorPaletteSO;
        else
            colorPalette = PersistentDataManager.Instance.colorPaletteSO;

        switch (myColor)
        {
            case TileType.None:
                spriter.color = white;
                main.startColor = white;
                break;
            case TileType.Color1:
                spriter.color = colorPalette.color1;
                main.startColor = colorPalette.color1;
                break;
            case TileType.Color2:
                spriter.color = colorPalette.color2;
                main.startColor = colorPalette.color2;
                break;
            case TileType.Black:
                spriter.color = black;
                main.startColor = black;
                break;
            default:
                Logger.LogError($"[PlayerController] 처리되지 않은 TileType으로 플레이어 색상을 변경할 수 없습니다: {changeColor}. " +
                           $"ChangeColor() 메소드의 switch문에 해당 색상에 대한 case를 추가해주세요.");
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

    // private IEnumerator Redo()
    // {
    //     if (Time.time < lastRedoTime + moveTimePerTile || moveCount <= 0 || !GameManager.Instance.isGaming)
    //         yield break;    
    //     lastRedoTime = Time.time;

    //     DecreaseMoveCount();
    //     PlayerMoveData moveData = moveDataListToRedo.Last.Value;
    //     moveDataListToRedo.RemoveLast();
    //     destI -= moveData.moveDir.x;
    //     destJ -= moveData.moveDir.y;
    //     PlayerMoveAnimation(new Vector2Int(destI, destJ), moveTimePerTile);
    //     yield return new WaitForSeconds(moveTimePerTile / 2f);

    //     if (myColor != moveData.prevMyColor)
    //         ChangeColor(moveData.prevMyColor);
    //     Board.Instance.ColorTile(curI, curJ, moveData.prevTileColor, false);
    //     curI = destI; curJ = destJ;

    //     yield return new WaitForSeconds(moveTimePerTile / 2f);
    //     player.DOScale(1, moveTimePerTile / 1.2f);
    // }
}