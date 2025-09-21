using System.Collections;
using UnityEngine;

public class PaintTile : TileData
{
    public override void OnPlayerEnter(PlayerController player, float moveTime)
    {
        StartCoroutine(ChangePlayerColorDelayed(player, moveTime / 2f));
    }

    private IEnumerator ChangePlayerColorDelayed(PlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);

        switch (Type)
        {
            case TileType.Color1Paint:
                player.ChangeColor(TileType.Color1);
                break;
            case TileType.Color2Paint:
                player.ChangeColor(TileType.Color2);
                break;
        }
    }
}