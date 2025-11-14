using UnityEngine;

public class DirectedSprayTile : SprayTile
{
    private Vector2Int direction;
    private bool doPaintReverse;

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);

        spraySpriter = transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        if (boardSOTileData is BoardSOIntTileData intTileData)
        {
            EditorDataFormat.DecodeDirectedSpray(intTileData.intValue,
                                                 out paintCount, out direction, out doPaintReverse);

            if (paintCount < 0) paintCount = maxSprayCount;
            waitColorOneTile = new WaitForSeconds(colorOneTileSpeed);
            SetChildTriangleRotationAndColor();
        }
        else
        {
            Logger.LogError($"DirectedSprayTile에 잘못된 데이터 타입이 전달되었습니다. : {boardSOTileData}");
        }
    }

    public override void OnBlockEnter(BlockData block, Vector2Int pos, Vector2Int direction, TileType color, float moveTime)
    {
        TileType colorType = doPaintReverse ? color.GetOppositeColor() : color;
        base.OnBlockEnter(block, pos, this.direction, colorType, moveTime);
        
        // if (!block.HasColor)
        //     return;
        // if (color != TileType.Color1 && color != TileType.Color2)
        //     return;

        // TileType colorType = doPaintReverse ? color.GetOppositeColor() : color;
        // ColorDirectlyForRedo(this.direction, colorType);
        // Color c = Board.Instance.GetColorByType(colorType);
        // StartCoroutine(MyTileColorChange(c));
        // StartSpray(this.direction, colorType);
    }

    // public void OnColorEnter(TileType colorType)
    // {
    //     colorType = doPaintReverse ? colorType.GetOppositeColor() : colorType;
    //     Color color = Board.Instance.GetColorByType(colorType);
    //     StartCoroutine(MyTileColorChange(color));
    //     if (colorType == TileType.Color1 || colorType == TileType.Color2)
    //         StartCoroutine(DoSprayTile(direction, colorType));
    // }

    private void SetChildTriangleRotationAndColor()
    {
        Transform child = transform.GetChild(0);
        child.rotation = CustomTools.GetRotationByDirection(direction);
        spraySpriter.transform.rotation = Quaternion.identity;

        // SpriteRenderer spriterTriangle = child.GetChild(0).GetComponent<SpriteRenderer>();
        // if (doPaintReverse)
        // {
        //     spriterTriangle.color = new Color(0.5f, 0.25f, 0.67f, 1f);
        // }
        // else
        // {
        //     spriterTriangle.color = new Color(0.3764706f, 0.3921569f, 0.4f, 1f);
        // }
    }
}
