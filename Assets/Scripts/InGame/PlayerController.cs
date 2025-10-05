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

public class PlayerController : BlockData
{
    public override TileType Type { get; protected set; } = TileType.Player;
    public override bool HasMutableColor { get; protected set; } = true;
    public override bool HasColor { get; protected set; } = true;
    public override TileType Color { get; protected set; } = TileType.White;

    [Header("플레이어의 색")]
    public Color white;
    public Color black, none;

    public ParticleSystem particle;
    public Vector2Int movingDirection { get; private set; }
    public Vector2Int destPos; // 최종 이동 장소로서 예약된 좌표 (플레이어 이동 중에는 현재좌표 != 도착좌표)
    private Queue<Vector2Int> moveToQueue = new Queue<Vector2Int>(); // 한 번에 여러 칸 이동하기 위해 다음에 이동할 타일을 나열한 큐
    private bool isMoving; // 현재 이동 중인지
    public int moveCount { get; private set; }
    private LinkedList<PlayerMoveData> moveDataListToRedo = new LinkedList<PlayerMoveData>(); // 되돌리기를 위해 그동안의 이동 데이터를 기록한 양방향 리스트
    private float lastRedoTime = 0f;

    private float keyboardNextFireTime = 0f;

    // 캐싱
    private SpriteRenderer spriter;
    private Transform player;
    private Coroutine inputMoveCoroutine;
    private WaitForSeconds halfMoveWaitForSeconds;

    protected static PlayerController m_Instance;
    public static PlayerController Instance { get { return m_Instance; } }

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    protected virtual void Init()
    {
        if (m_Instance == null)
            m_Instance = this;
        else
            Destroy(gameObject);

        spriter = transform.GetChild(0).GetComponent<SpriteRenderer>();
        player = transform.GetChild(0).transform;
        halfMoveWaitForSeconds = new WaitForSeconds(moveTime / 2f);
    }

    //삭제 시 실행되는 함수
    protected virtual void OnDestroy()
    {
        Dispose();
    }

    //삭제 시 추가로 처리해 주어야할 작업을 여기서 처리
    protected virtual void Dispose()
    {
        m_Instance = null;
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
                TryMoveTo(destPos.x + (int)dir.x, destPos.y + (int)dir.y);
                keyboardNextFireTime = Time.time + moveTime * 2;
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
    public void MoveOnce(Vector2Int dir)
    {
        TryMoveTo(destPos.x + dir.x, destPos.y + dir.y);
    }

    public void InitPlayer(BoardSO boardSO)
    {
        ApplyColorChange(boardSO.startPlayerColor);
        curPos.x = destPos.x = boardSO.startPos.x;
        curPos.y = destPos.y = boardSO.startPos.y;
        transform.position = Board.Instance.GetTilePos(curPos.x, curPos.y);
        SetMoveCount(0);
    }

    // (i, j) 좌표로 이동 시도
    public void TryMoveTo(int i, int j)
    {
        if (!CanMoveTo(i, j) || !GameManager.Instance.isGaming || slidingDirection != Vector2Int.zero)
            return;

        // 같은 행, 열 중 어디인지에 따라 분기
        if (destPos.x == i) // j 방향(가로 이동)
        {
            int step = (j > destPos.y) ? 1 : -1;
            for (int col = destPos.y + step; col != j + step; col += step)
                moveToQueue.Enqueue(new Vector2Int(destPos.x, col));
        }
        else if (destPos.y == j) // i 방향(세로 이동)
        {
            int step = (i > destPos.x) ? 1 : -1;
            for (int row = destPos.x + step; row != i + step; row += step)
                moveToQueue.Enqueue(new Vector2Int(row, destPos.y));
        }

        destPos.x = i;
        destPos.y = j;

        // 이동 중이 아니라면 이동 시작
        if (!isMoving) inputMoveCoroutine = StartCoroutine(Move());
    }

    public void ClearMoveQueue()
    {
        moveToQueue.Clear();
        if (inputMoveCoroutine != null)
            StopCoroutine(inputMoveCoroutine);
        inputMoveCoroutine = null;
    }

    // 플레이어가 (destPos.x, destPos.y) => (i, j)로 갈 수 있는지 검사
    private bool CanMoveTo(int i, int j)
    {
        if (destPos.x != i && destPos.y != j) return false; // 대각선 방향은 이동X
        if (!Board.Instance.board.ContainsKey(new Vector2Int(i, j))) return false; // 타일이 없으면 이동x

        // // 플레이어와 같은 색의 타일은 이동 불가능
        // TileColor playerColor = myColor;
        // TileColor changeColor;

        // 같은 행, 열 중 어디인지에 따라 분기
        if (destPos.x == i) // j 방향(가로 이동)
        {
            int step = (j > destPos.y) ? 1 : -1;
            for (int col = destPos.y + step; col != j + step; col += step)
            {
                if (!Board.Instance.board.ContainsKey(new Vector2Int(i, col)))
                    return false;
            }
        }
        else if (destPos.y == j) // i 방향(세로 이동)
        {
            int step = (i > destPos.x) ? 1 : -1;
            for (int row = destPos.x + step; row != i + step; row += step)
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
            Vector2Int nextPos = moveToQueue.Dequeue(); // 큐에서 하나 꺼내기
            movingDirection = nextPos - curPos;
            if (!BlockMoveController.Instance.CanMove(curPos, movingDirection))
            {
                destPos = curPos;
                break;
            }

            IncreaseMoveCount();
            curPos = nextPos;
            BlockMoveController.Instance.MoveBlocks(this, nextPos - movingDirection, movingDirection);
            PlayerMoveAnimation(nextPos, movingDirection, moveTime);
            Board.Instance.board[nextPos].OnBlockEnter(this, nextPos, movingDirection, Color, moveTime);
            yield return halfMoveWaitForSeconds;

            Board.Instance.CheckGameClear();
            yield return halfMoveWaitForSeconds;
        }
        if (slidingDirection == Vector2Int.zero)
        {
            player.DOScale(1, moveTime / 1.2f);
            isMoving = false;
        }
    }

    protected override IEnumerator StartMoveCoroutine(Vector2Int curPos, Vector2Int slidingDirection)
    {
        while (BlockMoveController.Instance.CanMove(curPos, slidingDirection))
        {
            BlockMoveController.Instance.MoveBlocks(this, curPos, slidingDirection);
            curPos += slidingDirection;
            destPos = this.curPos = curPos;
            PlayerMoveAnimation(curPos, slidingDirection, moveTime);
            Board.Instance.board[curPos].OnBlockEnter(this, curPos, movingDirection, Color, moveTime);
            yield return halfMoveWaitForSeconds;

            if (Board.Instance.CheckGameClear())
                break;

            yield return halfMoveWaitForSeconds;
        }
        this.slidingDirection = Vector2Int.zero;
        player.DOScale(1, moveTime / 1.2f);
        isMoving = false;
    }

    private void PlayerMoveAnimation(Vector2Int nextPos, Vector2Int direction, float moveTime)
    {
        Vector2 nextRealPos = Board.Instance.GetTilePos(nextPos.x, nextPos.y);
        transform.DOMove(nextRealPos, moveTime).SetEase(Ease.Linear);
        PlayerScaleAnimation(direction, moveTime);
        if (curPos.x == nextPos.x) // 세로 이동
            particle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        else
            particle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        particle.Emit(4);
    }

    private void PlayerScaleAnimation(Vector2Int moveDirection, float moveTime)
    {
        if (moveDirection == Vector2Int.zero) player.DOScale(1, moveTime / 1.2f); // 이동 중지
        else if (moveDirection.x == 0) player.DOScale(new Vector2(0.85f, 1.15f), moveTime / 1.2f); // 세로 이동
        else player.DOScale(new Vector2(1.15f, 0.85f), moveTime / 1.2f); // 가로 이동
    }

    protected override void ApplyColorChange(TileType changeColor)
    {
        Color = changeColor;
        var main = particle.main;

        ColorPaletteSO colorPalette;
        if (GameManager.Instance.startGameDirectlyAtInGameScene)
            colorPalette = Board.Instance.colorPaletteSO;
        else
            colorPalette = PersistentDataManager.Instance.colorPaletteSO;

        switch (Color)
        {
            case TileType.None:
                spriter.color = none;
                main.startColor = none;
                break;
            case TileType.White:
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
        GameManager.Instance.UpdateMoveCount(moveCount);
    }

    private void IncreaseMoveCount()
    {
        moveCount++;
        GameManager.Instance.UpdateMoveCount(moveCount);
    }

    private void DecreaseMoveCount()
    {
        moveCount--;
        GameManager.Instance.UpdateMoveCount(moveCount);
    }

    // private IEnumerator Redo()
    // {
    //     if (Time.time < lastRedoTime + moveTimePerTile || moveCount <= 0 || !GameManager.Instance.isGaming)
    //         yield break;    
    //     lastRedoTime = Time.time;

    //     DecreaseMoveCount();
    //     PlayerMoveData moveData = moveDataListToRedo.Last.Value;
    //     moveDataListToRedo.RemoveLast();
    //     destPos.x -= moveData.moveDir.x;
    //     destPos.y -= moveData.moveDir.y;
    //     PlayerMoveAnimation(new Vector2Int(destPos.x, destPos.y), moveTimePerTile);
    //     yield return new WaitForSeconds(moveTimePerTile / 2f);

    //     if (myColor != moveData.prevMyColor)
    //         ChangeColor(moveData.prevMyColor);
    //     Board.Instance.ColorTile(curI, curJ, moveData.prevTileColor, false);
    //     curI = destPos.x; curJ = destPos.y;

    //     yield return new WaitForSeconds(moveTimePerTile / 2f);
    //     player.DOScale(1, moveTimePerTile / 1.2f);
    // }
}