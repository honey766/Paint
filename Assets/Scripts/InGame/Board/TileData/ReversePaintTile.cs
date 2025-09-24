using System.Collections;
using UnityEngine;

public class ReversePaintTile : TileData
{
    public override void OnBlockEnter(BlockData block, Vector2Int pos, Vector2Int direction, TileType color, float moveTime)
    {
        if (block.HasMutableColor)
            StartCoroutine(ChangePlayerColorDelayed(block, moveTime / 2f));
    }

    private IEnumerator ChangePlayerColorDelayed(BlockData block, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        switch (block.Color)
        {
            case TileType.Color1:
                block.ChangeColor(TileType.Color2);
                break;
            case TileType.Color2:
                block.ChangeColor(TileType.Color1);
                break;
        }
    }
}