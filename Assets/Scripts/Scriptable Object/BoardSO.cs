using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TileData
{
    public Vector2Int pos;
    public TileColor color;

    public TileData(Vector2Int pos, TileColor color)
    {
        this.pos = pos;
        this.color = color;
    }
}

[Serializable]
public struct PaintData
{
    public Vector2Int pos;
    public TileColor color;

    public PaintData(Vector2Int pos, TileColor color)
    {
        this.pos = pos;
        this.color = color;
    }
}

[CreateAssetMenu(fileName = "Board", menuName = "ScriptableObjects/Board")]
public class BoardSO : ScriptableObject
{
    public int n, m;
    public Vector2Int startPos;
    public List<TileData> tileList = new List<TileData>();
    public List<PaintData> paintList = new List<PaintData>();

    // 런타임용 Dictionary 변환
    public Dictionary<Vector2Int, TileColor> GetTileDict()
    {
        var dict = new Dictionary<Vector2Int, TileColor>();
        foreach (var entry in tileList)
            dict[entry.pos] = entry.color;
        return dict;
    }

    public Dictionary<Vector2Int, TileColor> GetPaintDict()
    {
        var dict = new Dictionary<Vector2Int, TileColor>();
        foreach (var entry in paintList)
            dict[entry.pos] = entry.color;
        return dict;
    }
}