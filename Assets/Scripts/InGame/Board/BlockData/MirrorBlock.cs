using DG.Tweening;
using UnityEngine;

public class MirrorBlock : BlockData
{
    public override TileType Type { get; protected set; } = TileType.Mirror;
    public override bool HasMutableColor { get; protected set; } = false;
    public override bool HasColor { get; protected set; } = false;
    public override TileType Color { get; protected set; } = TileType.None;
    protected override void ApplyColorChange(TileType color) { }

    public bool isBottomLeftToTopRight;
    private SpriteRenderer mirrorSpriter;
    private Color defaultColor = new Color(0.68f, 0.87f, 0.8f, 0.6f);
    protected const float mirrorColorChangeTime = 1.2f;

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);

        mirrorSpriter = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (boardSOTileData is BoardSOIntTileData intTileData)
        {
            isBottomLeftToTopRight = intTileData.intValue == 1;
            if (isBottomLeftToTopRight)
                transform.localScale = new Vector3(-1, 1, 1) * transform.localScale.x;
        }
        else
        {
            Logger.LogError($"MirrorBlock에 잘못된 데이터 타입이 전달되었습니다. : {boardSOTileData}");
        }
    }

    public void OnMirrorEnter(TileType color)
    {
        mirrorSpriter.DOKill();
        mirrorSpriter.color = Board.Instance.GetColorByType(color);
        mirrorSpriter.DOColor(defaultColor, mirrorColorChangeTime);
    }
}