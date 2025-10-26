using UnityEngine;

public class BrushBlock : BlockData
{
    public override TileType Type { get; protected set; } = TileType.Brush;
    public override bool HasMutableColor { get; protected set; } = true;
    public override bool HasColor { get; protected set; } = true;
    public override TileType Color { get; protected set; } = TileType.None;

    private SpriteRenderer spriter;

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);
        spriter = GetComponent<SpriteRenderer>();

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

    protected override void ApplyColorChange(TileType color)
    {
        Color = color;
        spriter.color = Board.Instance.GetColorByType(color);
    }
}