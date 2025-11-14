using System.Collections;
using UnityEngine;

public class ReversePaintTile : TileData
{
    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);
        Color color1 = Board.Instance.colorPaletteSO.color1;
        Color color2 = Board.Instance.colorPaletteSO.color2;
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = color1;
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = color2;
    }

    public override void OnBlockEnter(BlockData block, Vector2Int pos, Vector2Int direction, TileType color, float moveTime)
    {
        base.OnBlockEnter(block, pos, direction, color, moveTime);
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