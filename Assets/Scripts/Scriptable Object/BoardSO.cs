using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardSOTileData
{
    public Vector2Int pos;
    public TileType type;
    public BoardSOTileData(Vector2Int pos, TileType type)
    {
        this.pos = pos;
        this.type = type;
    }
}

[Serializable]
public class BoardSOIntTileData : BoardSOTileData
{
    public int intValue;
    public BoardSOIntTileData(Vector2Int pos, TileType type, int intValue) : base(pos, type)
    {
        this.intValue = intValue;
    }
}

[Serializable]
public class BoardSOFloatTileData : BoardSOTileData
{
    public float floatValue;
    public BoardSOFloatTileData(Vector2Int pos, TileType type, float floatValue) : base(pos, type)
    {
        this.floatValue = floatValue;
    }
}

[CreateAssetMenu(fileName = "Board", menuName = "ScriptableObjects/Board")]
public class BoardSO : ScriptableObject
{
    public int n, m;
    public Vector2Int startPos;
    [SerializeReference]
    public List<BoardSOTileData> boardTileList = new List<BoardSOTileData>();
    [SerializeReference]
    public List<BoardSOTileData> targetTileList = new List<BoardSOTileData>();
}