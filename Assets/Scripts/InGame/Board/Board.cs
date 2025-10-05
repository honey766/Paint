using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : SingletonBehaviour<Board>
{
    public Transform tileParent, blockParent;

    public Dictionary<Vector2Int, TileData> board; // 보드 상태
    public Dictionary<Vector2Int, BlockData> blocks; // 블록 위치와 상태
    public Dictionary<Vector2Int, TileType> target; // 목표 보드
    private int n, m; // 세로, 가로 크기
    private bool existsEraser;

    [Header("색칠 색")]
    public Color white;
    public Color black;
    public GridBorderDrawer color1Border, color2Border, color12Border, blackBorder;

    private BoardSO boardSO;
    [SerializeField]
    private TileFactoryConfigSO tileFactoryConfig;

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
        this.n = boardSO.n;
        this.m = boardSO.m;
        board = new Dictionary<Vector2Int, TileData>();
        blocks = new Dictionary<Vector2Int, BlockData>();
        target = new Dictionary<Vector2Int, TileType>();
    }

    private void InitBoard()
    {
        existsEraser = false;
        foreach (var entry in boardSO.boardTileList)
        {
            if (!entry.type.IsBlock())
            {
                board[entry.pos] = TileFactory.CreateTile<TileData>(entry);
                if (entry.type == TileType.WhitePaint)
                    existsEraser = true;
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

        // color1Border.InitBorder(colorPaletteSO.color1, TileType.Color1, n, m, target);
        // color2Border.InitBorder(colorPaletteSO.color2, TileType.Color2, n, m, target);
        color12Border.InitBorder(colorPaletteSO.color12, TileType.Color12, n, m, target);
        // blackBorder.InitBorder(black, TileType.Black, n, m, target);
    }

    public void InitBoardWhenRestart(BoardSO boardSO)
    {
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
                    if (!existsEraser) GameManager.Instance.Color12Warning();
                    return false;
                }
            }
        }
        return matchingTileWithTargetCount == target.Count;
    }

    // public bool IsInBounds(int i, int j)
    // {
    //     return 0 <= i && i < n && 0 <= j && j < m;
    // }

    public Color GetColorByType(TileType type)
    {
        switch (type)
        {
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
            // case TileType.ReversePaint:
            //     return (colorPaletteSO.color12 + 2 * Color.white) / 3f;
            default:
                Logger.LogWarning($"No color defined for tile type: {type}");
                return white;
        }
    }
}
