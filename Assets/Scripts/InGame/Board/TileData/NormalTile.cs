using UnityEngine;

public class NormalTile : TileData
{
    public override void OnBlockEnter(BlockData block, Vector2Int pos, Vector2Int direction, TileType color, float moveTime)
    {
        if (block.HasColor)
        {
            Type = Type.AddColorToNormalTile(color);
            WaitAndDrawTile(moveTime / 2f);
        }
    }

    public void SetTileColor(TileType type, float waitTime)
    {
        if (type == TileType.None || type.IsSpecialTile())
            return;

        Type = type;
        WaitAndDrawTile(waitTime);
    }

    public void AddTileColor(TileType type, float waitTime)
    {
        if (type == TileType.None || type.IsSpecialTile())
            return;

        Type = Type.AddColorToNormalTile(type);
        WaitAndDrawTile(waitTime);
    }
}