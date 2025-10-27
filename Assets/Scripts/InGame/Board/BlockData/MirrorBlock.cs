using DG.Tweening;
using UnityEngine;

public class MirrorBlock : BlockData
{
    public override TileType Type { get; protected set; } = TileType.Mirror;
    public override bool HasMutableColor { get; protected set; } = false;
    public override bool HasColor { get; protected set; } = false;
    public override TileType Color { get; protected set; } = TileType.None;
    public override bool IsTransparent { get; protected set; } = true;
    protected override void ApplyColorChange(TileType color) { }

    public bool isBottomLeftToTopRight;
    private SpriteRenderer mirrorSpriter, glassSpriter;
    private Color mirrorDefaultColor = new Color(0.44f, 0.71f, 0.62f, 0.65f);
    private Color glassDefaultColor;
    protected const float mirrorColorChangeTime = 1.2f;

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);

        mirrorSpriter = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        glassSpriter = transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        glassDefaultColor = glassSpriter.color;

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
        glassSpriter.DOKill();
        AudioManager.Instance.PlaySfx(SfxType.MirrorActivation, 0.5f);

        Color colorByType = Board.Instance.GetColorByType(color);
        mirrorSpriter.color = (UnityEngine.Color.white * 2f + colorByType) / 3f;
        Color glassColor = (glassDefaultColor * 1.5f + colorByType) / 2.5f;
        glassColor.a = 0.35f;
        glassSpriter.color = glassColor;
        
        mirrorSpriter.DOColor(mirrorDefaultColor, mirrorColorChangeTime);
        glassSpriter.DOColor(glassDefaultColor, mirrorColorChangeTime);
    }
}