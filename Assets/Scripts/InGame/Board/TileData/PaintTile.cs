using System.Collections;
using UnityEngine;

public class PaintTile : TileData
{
    public override void OnBlockEnter(BlockData block, Vector2Int pos, Vector2Int direction, TileType color, float moveTime)
    {
        if (block.HasMutableColor)
            StartCoroutine(ChangePlayerColorDelayed(block, moveTime / 2f));
    }

    private IEnumerator ChangePlayerColorDelayed(BlockData block, float delay)
    {
        yield return new WaitForSeconds(delay);

        switch (Type)
        {
            case TileType.Color1Paint:
                block.ChangeColor(TileType.Color1);
                break;
            case TileType.Color2Paint:
                block.ChangeColor(TileType.Color2);
                break;
        }
    }
}