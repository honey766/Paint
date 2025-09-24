using UnityEngine;

public class StampBlock : BlockData
{
    public override TileType Type { get; protected set; } = TileType.Stamp;
    public override bool HasMutableColor { get; protected set; } = true;
    public override bool HasColor { get; protected set; } = true;
    public override TileType Color { get; protected set; } = TileType.White;

    private SpriteRenderer spriter;

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);
        spriter = GetComponent<SpriteRenderer>();
    }

    protected override void ApplyColorChange(TileType color)
    {
        Color = color;
        spriter.color = Board.Instance.GetColorByType(color);
    }
}