using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : SingletonBehaviour<Board>
{
    public Transform tileParent;

    // public TileType[,] board;  // 현재 보드 상태
    // public TileType[,] answer; // 목표 보드 상태
    // public HashSet<Vector2Int> tileSet; // 타일 위치

    public Dictionary<Vector2Int, TileData> board; // 보드 상태
    public Dictionary<Vector2Int, TileType> target; // 목표 보드
    private int n, m; // 세로, 가로 크기

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
        base.Init();
        TileFactory.Initialize(tileFactoryConfig);
    }

    public void InitBoard(BoardSO boardSO)
    {
        this.boardSO = boardSO;
        InitProperties();
        InitBoard();
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
        target = new Dictionary<Vector2Int, TileType>();
    }

    private void InitBoard()
    {
        foreach (var entry in boardSO.boardTileList)
        {
            board[entry.pos] = TileFactory.CreateTile(entry);
        }


        foreach (var entry in boardSO.targetTileList)
        {
            target[entry.pos] = entry.type;
        }

        // color1Border.InitBorder(colorPaletteSO.color1, TileType.Color1, n, m, target);
        // color2Border.InitBorder(colorPaletteSO.color2, TileType.Color2, n, m, target);
        color12Border.InitBorder(colorPaletteSO.color12, TileType.Color12, n, m, target);
        // blackBorder.InitBorder(black, TileType.Black, n, m, target);
    }

    /// <summary>
    /// (i, j) 타일의 실제 좌표를 반환
    /// </summary>
    public Vector2 GetTilePos(int i, int j)
    {
        return new Vector2(i, j) - new Vector2((n - 1) / 2f, (m - 1) / 2f);
    }

    /// <summary>
    /// board의 target위치와 target이 같은지 검사
    /// </summary>
    public bool IsGameClear()
    {
        foreach (var entry in board)
        {
            if (entry.Value.Type == TileType.Color12)
            {
                bool isOkay = false;
                if (target.TryGetValue(entry.Key, out TileType targetTileType))
                    if (targetTileType == TileType.Color12)
                        isOkay = true;
                if (!isOkay)
                    return false;
            }
        }
        return true;
    }

    public bool IsInBounds(int i, int j)
    {
        return 0 <= i && i < n && 0 <= j && j < m;
    }
}
