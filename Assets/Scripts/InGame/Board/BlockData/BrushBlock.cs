using UnityEngine;
using DG.Tweening;

public class BrushBlock : BlockData
{
    public override TileType Type { get; protected set; } = TileType.Brush;
    public override bool HasMutableColor { get; protected set; } = true;
    public override bool HasColor { get; protected set; } = true;
    public override TileType Color { get; protected set; } = TileType.None;
    public override bool IsTransparent { get; protected set; } = false;

    [SerializeField] private Sprite brush, erasor;
    private SpriteRenderer spriter;
    private SpriteRenderer toolSpriter;

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);
        spriter = transform.GetChild(1).GetComponent<SpriteRenderer>();
        toolSpriter = transform.GetChild(2).GetComponent<SpriteRenderer>();

        if (boardSOTileData is BoardSOIntTileData intTileData)
        {
            if (intTileData.intValue == 1) Color = TileType.Color1;
            else if (intTileData.intValue == 2) Color = TileType.Color2;
            ApplyColorChange(Color);
        }
        else
        {
            Logger.LogError($"BrushBlock에 잘못된 데이터 타입이 전달되었습니다. : {boardSOTileData}");
        }
    }

    public override void AdjustAlphaBasedOnTileBelow(TileData tile)
    {
        if (IsTransparent || Type == TileType.Player)
            return;
        if (HaveToChangeAlpha(tile))
        {
            if (tile.Type == TileType.Spray || tile.Type == TileType.DirectedSpray)
                mySpriter.DOFade(0.7f, 0.15f);
            else
                mySpriter.DOFade(0.85f, 0.15f);
        }
        else
        {
            mySpriter.DOFade(1f, 0.15f);
        }
    }

    protected override void ApplyColorChange(TileType color)
    {
        Color = color;
        spriter.color = Board.Instance.GetColorByType(color);
        
        if (Color == TileType.White) toolSpriter.sprite = erasor;
        else toolSpriter.sprite = brush;
    }
}