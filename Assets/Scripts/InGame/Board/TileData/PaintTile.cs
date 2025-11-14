using System.Collections;
using UnityEngine;

public class PaintTile : TileData
{
    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);
        SpriteRenderer paintSpriter = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (boardSOTileData.type == TileType.Color1Paint)
            paintSpriter.sprite = Resources.Load<Sprite>("Images/Color1PaintNew");
        else if (boardSOTileData.type == TileType.Color2Paint)
            paintSpriter.sprite = Resources.Load<Sprite>("Images/Color2Paint");
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

        switch (Type)
        {
            case TileType.Color1Paint:
                block.ChangeColor(TileType.Color1);
                break;
            case TileType.Color2Paint:
                block.ChangeColor(TileType.Color2);
                break;
            case TileType.WhitePaint:
                block.ChangeColor(TileType.White);
                break;
        }
    }
}