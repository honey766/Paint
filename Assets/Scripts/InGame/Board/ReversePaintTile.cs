using System.Collections;
using UnityEngine;

public class ReversePaintTile : TileData
{
    public override void OnPlayerEnter(PlayerController player, float moveTime)
    {
        StartCoroutine(ChangePlayerColorDelayed(player, moveTime / 2f));
    }

    private IEnumerator ChangePlayerColorDelayed(PlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        switch (player.myColor)
        {
            case TileType.Color1:
                player.ChangeColor(TileType.Color2);
                break;
            case TileType.Color2:
                player.ChangeColor(TileType.Color1);
                break;
        }
    }
}