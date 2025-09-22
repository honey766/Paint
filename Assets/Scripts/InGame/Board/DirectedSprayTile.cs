using UnityEngine;

public class DirectedSprayTile : SprayTile
{
    private Vector2Int direction;
    private bool doPaintReverse;

    public override void OnPlayerEnter(PlayerController player, float moveTime)
    {
        TileType colorType = doPaintReverse ? player.myColor.GetOppositeColor() : player.myColor;
        Color color = Board.Instance.GetColorByType(colorType);
        StartCoroutine(MyTileColorChange(color));
        if (colorType == TileType.Color1 || colorType == TileType.Color2)
            StartCoroutine(DoSprayTile(direction, colorType));
    }

    public void OnColorEnter(TileType colorType)
    {
        colorType = doPaintReverse ? colorType.GetOppositeColor() : colorType;
        Color color = Board.Instance.GetColorByType(colorType);
        StartCoroutine(MyTileColorChange(color));
        if (colorType == TileType.Color1 || colorType == TileType.Color2)
            StartCoroutine(DoSprayTile(direction, colorType));
    }

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);

        if (boardSOTileData is BoardSOIntTileData intTileData)
        {
            pos = intTileData.pos;
            EditorDataFormat.DecodeDirectedSpray(intTileData.intValue,
                                                 out paintCount, out direction, out doPaintReverse);

            if (paintCount < 0) paintCount = 1_000_000_000;
            waitColorOneTile = new WaitForSeconds(colorOneTileSpeed);
            SetChildTriangleRotationAndColor();
        }
        else
        {
            Logger.LogError("DirectedSprayTile에 잘못된 데이터 타입이 전달되었습니다.");
        }
    }

    private void SetChildTriangleRotationAndColor()
    {
        Transform child = transform.GetChild(0);
        child.rotation = CustomTools.GetRotationByDirection(direction);

        SpriteRenderer spriterTriangle = child.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer spriterOutline = child.GetChild(1).GetComponent<SpriteRenderer>();
        if (doPaintReverse)
        {
            spriterTriangle.color = new Color(0.5f, 0.25f, 0.67f, 0.5f);
            spriterOutline.color = new Color(0.6f, 0.45f, 0.7f, 0.9f);
        }
        else
        {
            spriterTriangle.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            spriterOutline.color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
        }
    }
}
