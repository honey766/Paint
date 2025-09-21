public class NormalTile : TileData
{
    public override void OnPlayerEnter(PlayerController player, float moveTime)
    {
        Type = Type.AddColorToNormalTile(player.myColor);
        WaitAndDrawTile(moveTime / 2f);
    }

    public void SetTileColor(TileType type, float waitTime)
    {
        if (type == TileType.None || type.IsSpecialTile())
            return;

        Type = type;
        WaitAndDrawTile(waitTime);
    }
}