using UnityEngine;

public class JustBlock : BlockData
{
    public override TileType Type { get; protected set; } = TileType.Mirror;
    public override bool HasMutableColor { get; protected set; } = false;
    public override bool HasColor { get; protected set; } = false;
    public override TileType Color { get; protected set; } = TileType.None;
    public override bool IsTransparent { get; protected set; } = false;
    protected override void ApplyColorChange(TileType color) { }
}
