using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Random = UnityEngine.Random;

public class Board : SingletonBehaviour<Board>
{
    public Transform tileParent, blockParent;

    public Dictionary<Vector2Int, TileData> board; // 보드 상태
    public Dictionary<Vector2Int, BlockData> blocks; // 블록 위치와 상태
    public Dictionary<Vector2Int, TileType> target; // 목표 보드
    public HashSet<TileData> sprays; // 현재 보드에 있는 스프레이 타일
    public Dictionary<Vector2Int, TileType> boardTypeForRedo;
    private int n, m; // 세로, 가로 크기
    private bool existsEraser;

    [Header("색칠 색")]
    public Color white;
    public Color black;
    public Color none;
    [SerializeField] private PurpleBorderDrawer purpleBorder;
    [SerializeField] private TileOutlineDrawer tileOutline;

    private BoardSO boardSO;
    [SerializeField] private TileFactoryConfigSO tileFactoryConfig;

    [Header("InGame씬에서 바로 실행하기 (GameManager에서 설정)")]
    public ColorPaletteSO colorPaletteSO;

    protected override void Init()
    {
        m_IsDestroyOnLoad = true;
        base.Init();
        TileFactory.Initialize(tileFactoryConfig);
    }

    public void InitBoard(BoardSO boardSO)
    {
        this.boardSO = boardSO;
        InitProperties();
        InitBoard();
        BlockMoveController.Instance.InitBoard();
    }

    private void InitProperties()
    {
        if (!GameManager.Instance.startGameDirectlyAtInGameScene)
        {
            this.colorPaletteSO = PersistentDataManager.Instance.colorPaletteSO;
        }
        if (board != null)
        {
            foreach (var entry in board.Values)
            {
                Destroy(entry.gameObject);
            }
        }
        if (blocks != null)
        {
            foreach (var entry in blocks.Values)
                if (entry.Type != TileType.Player)
                    Destroy(entry.gameObject);
        }

        this.n = boardSO.n;
        this.m = boardSO.m;
        board = new Dictionary<Vector2Int, TileData>();
        blocks = new Dictionary<Vector2Int, BlockData>();
        target = new Dictionary<Vector2Int, TileType>();
        sprays = new HashSet<TileData>();
        boardTypeForRedo = new Dictionary<Vector2Int, TileType>();
    }

    private void InitBoard()
    {
        existsEraser = false;
        foreach (var entry in boardSO.boardTileList)
        {
            if (!entry.type.IsBlock())
            {
                board[entry.pos] = TileFactory.CreateTile<TileData>(entry);
                boardTypeForRedo[entry.pos] = entry.type;
                if (entry.type == TileType.WhitePaint)
                    existsEraser = true;
                if (entry.type == TileType.Spray || entry.type == TileType.DirectedSpray)
                    sprays.Add(board[entry.pos]);
            }
        }
        foreach (var entry in boardSO.boardTileList)
        {
            if (entry.type.IsBlock())
                blocks[entry.pos] = TileFactory.CreateTile<BlockData>(entry);
        }
        blocks[boardSO.startPos] = PlayerController.Instance;
        foreach (var entry in boardSO.targetTileList)
        {
            target[entry.pos] = entry.type;
        }

        tileOutline.InitOutlineAndShadow(n, m, board.Keys);
        purpleBorder.InitBorder(n, m, target, 1f);
        purpleBorder.InitBorder(n, m, target, 0.7f);
    }

    public void InitBoardWhenRestart(BoardSO boardSO)
    {
        StopSpraying();
        
        // Tile
        foreach (var entry in boardSO.boardTileList)
            if (entry.type.IsNormalTile() && board[entry.pos] is NormalTile normalTile)
                normalTile.SetTileColor(entry.type, 0);

        // Block
        foreach (BlockData blockData in blocks.Values)
            if (blockData.Type != TileType.Player)
                Destroy(blockData.gameObject);
        blocks.Clear();
        foreach (var entry in boardSO.boardTileList)
            if (entry.type.IsBlock())
                blocks[entry.pos] = TileFactory.CreateTile<BlockData>(entry);
    }

    /// <summary>
    /// (i, j) 타일의 실제 좌표를 반환
    /// </summary>
    public Vector2 GetTilePos(int i, int j)
    {
        return new Vector2(i, j) - new Vector2((n - 1) / 2f, (m - 1) / 2f);
    }

    public bool CheckGameClear()
    {
        bool isGameClear = IsGameClear();
        if (isGameClear) GameManager.Instance.GameClear();
        return isGameClear;
    }

    // board의 target위치와 target이 같은지 검사
    private bool IsGameClear()
    {
        int matchingTileWithTargetCount = 0;
        foreach (var entry in board)
        {
            if (entry.Value.Type == TileType.Color12)
            {
                bool isOkay = false;
                if (target.TryGetValue(entry.Key, out TileType targetTileType))
                    if (targetTileType == TileType.Color12)
                        isOkay = true;

                if (isOkay)
                {
                    matchingTileWithTargetCount++;
                }
                else // 엄한 데에다 보라색을 칠함
                {
                    if (!existsEraser) GameManager.Instance.Color12Warning(true);
                    return false;
                }
            }
        }
        GameManager.Instance.Color12Warning(false);
        return matchingTileWithTargetCount == target.Count;
    }

    public Color GetColorByType(TileType type)
    {
        switch (type)
        {
            case TileType.None:
                return none;
            case TileType.White:
                return white;
            case TileType.Color1:
            case TileType.Color1Paint:
                return colorPaletteSO.color1;
            case TileType.Color2:
            case TileType.Color2Paint:
                return colorPaletteSO.color2;
            case TileType.Color12:
                return colorPaletteSO.color12;
            case TileType.Black:
                return black;
            default:
                Logger.LogWarning($"No color defined for tile type: {type}");
                return white;
        }
    }

    public void StopSpraying()
    {
        foreach (var spray in sprays)
            if (spray is SprayTile sprayT)
                sprayT.StopSpraying();
    }
    public void RedoBoard(Dictionary<Vector2Int, TileType> normalBoard)
    {
        foreach (var entry in normalBoard)
        {
            if (board.TryGetValue(entry.Key, out TileData tileData))
            {
                if (tileData.Type == entry.Value) // 실제 타일과 boardTypeForRedo가 동기화되지 않을 수 있으므로 값만 할당해주고 애니메이션은 X
                    boardTypeForRedo[entry.Key] = entry.Value;
                else if (tileData is NormalTile normalTile)
                    normalTile.SetTileColor(entry.Value, 0);
            }
        }
    }
    public void RedoPlayer(Vector2Int destPos, Vector2Int moveDirection)
    {
        blocks[destPos - moveDirection] = PlayerController.Instance;
        if (blocks.ContainsKey(destPos))
            blocks.Remove(destPos);
    }
    public void RedoBlocks(Dictionary<Vector2Int, BlockMoveData> blockMoveData)
    {
        foreach (var entry in blockMoveData)
        {
            if (blocks.TryGetValue(entry.Value.destPos, out BlockData blockData))
            {
                blockData.ChangeColor(entry.Value.prevColor);
                blockData.MoveAnimation(entry.Key, true);
                blocks.Remove(entry.Value.destPos);
                blocks[entry.Key] = blockData;
            }
        }
    }
}