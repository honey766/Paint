using System.Collections;
using UnityEngine;

public class PaintBeamTile : TileData
{
    private const float myTileColorChangeTime = 0.5f;
    private const float colorOneTileSpeed = 0.08f;
    private int paintCount;
    private Vector2Int pos;
    private WaitForSeconds waitColorOneTile;

    public override void OnPlayerEnter(PlayerController player, float moveTime)
    {
        Color color = Board.Instance.GetColorByType(player.myColor);
        Vector2Int direction = player.movingDirection;
        StartCoroutine(MyTileColorChange(color));
        StartCoroutine(ColorTileBeam(direction));
    }

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);

        if (boardSOTileData is BoardSOIntTileData intTileData)
        {
            pos = intTileData.pos;
            paintCount = intTileData.intValue < 0 ? 1000000000 : intTileData.intValue;
            waitColorOneTile = new WaitForSeconds(colorOneTileSpeed);
        }
        else
        {
            Logger.LogError("PaintBeamTile에 잘못된 데이터 타입이 전달되었습니다.");
        }
    }

    private IEnumerator MyTileColorChange(Color color)
    {
        spriter.color = color;
        yield return MyCoroutine.WaitFor(myTileColorChangeTime, (t) =>
        {
            spriter.color = Color.Lerp(color, Color.white, t);
        });
    }

    private IEnumerator ColorTileBeam(Vector2Int direction)
    {
        Vector2Int curPos = pos;
        for (int i = 0; i < paintCount; i++)
        {
            curPos += direction;
            // 타일이 없으면 즉시 종료
            if (!Board.Instance.board.TryGetValue(curPos, out TileData tileData))
                break;

            if (tileData is NormalTile)
                tileData.OnPlayerEnter(PlayerController.Instance, 0);

            yield return waitColorOneTile;
        }
    }
}