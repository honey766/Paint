using UnityEngine;

public class MirrorBlock : BlockData
{
    public override TileType Type { get; protected set; } = TileType.Mirror;
    public override bool HasMutableColor { get; protected set; } = false;
    public override bool HasColor { get; protected set; } = false;
    public override TileType Color { get; protected set; } = TileType.None;
    protected override void ApplyColorChange(TileType color) { }

    public bool isBottomLeftToTopRight;

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);

        if (boardSOTileData is BoardSOIntTileData intTileData)
        {
            isBottomLeftToTopRight = intTileData.intValue == 1;
            if (!isBottomLeftToTopRight)
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }
        else
        {
            Logger.LogError($"MirrorBlock에 잘못된 데이터 타입이 전달되었습니다. : {boardSOTileData}");
        }
    }
}