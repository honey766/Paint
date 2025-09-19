public class PaintBeamTile : TileData
{
    private int paintCount;

    public override void OnPlayerEnter(PlayerController player, float moveTime)
    {
        Type = Type.AddColorToNormalTile(player.myColor);
        DrawTile();
    }

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);

        if (boardSOTileData is BoardSOIntTileData intTileData)
        {
            paintCount = intTileData.intValue;
        }
        else
        {
            Logger.LogError("PaintBeamTile에 잘못된 데이터 타입이 전달되었습니다.");
        }
    }
}