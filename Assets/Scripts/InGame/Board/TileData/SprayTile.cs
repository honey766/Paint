using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SprayTile : TileData
{
    protected const float myTileColorChangeTime = 0.5f;
    protected const float colorOneTileSpeed = 0.06f;
    protected int paintCount;
    protected Vector2Int pos;
    protected WaitForSeconds waitColorOneTile;
    protected ParticleSystem particle;

    public override void OnBlockEnter(BlockData block, Vector2Int pos, Vector2Int direction, TileType color, float moveTime)
    {
        if (!block.HasColor)
            return;
        Color c = Board.Instance.GetColorByType(color);
        StartCoroutine(MyTileColorChange(c));
        if (color == TileType.Color1 || color == TileType.Color2)
            StartCoroutine(DoSprayTile(direction, color));
    }

    public override void Initialize(BoardSOTileData boardSOTileData)
    {
        base.Initialize(boardSOTileData);

        // particle = GetComponentInChildren<ParticleSystem>();
        // particle.Stop();
        if (boardSOTileData is BoardSOIntTileData intTileData)
        {
            pos = intTileData.pos;
            paintCount = intTileData.intValue < 0 ? 1_000_000_000 : intTileData.intValue;
            waitColorOneTile = new WaitForSeconds(colorOneTileSpeed);
        }
        else
        {
            Logger.LogError($"SprayTile에 잘못된 데이터 타입이 전달되었습니다. : {boardSOTileData}");
        }
    }

    protected IEnumerator MyTileColorChange(Color color)
    {
        spriter.color = color;
        yield return MyCoroutine.WaitFor(myTileColorChangeTime, (t) =>
        {
            spriter.color = Color.Lerp(color, Color.white, t);
        });
    }

    protected IEnumerator DoSprayTile(Vector2Int direction, TileType colorType)
    {
        Vector2Int curPos = pos;
        
        // DoParticleEffect(curPos, direction, colorType);
        // particle.transform.position = transform.position;
        // particle.Play();
        // particle.Emit(6);

        for (int i = 0; i < paintCount; i++)
        {
            curPos += direction;
            // 타일이 없으면 즉시 종료
            if (!Board.Instance.board.TryGetValue(curPos, out TileData tileData))
                break;

            // DoParticleEffect(curPos, direction, colorType);

            if (tileData is NormalTile normalTile)
                normalTile.AddTileColor(colorType, 0);
            else if (tileData is DirectedSprayTile directedSprayTile)
                directedSprayTile.OnColorEnter(colorType);
            else if (tileData is ReversePaintTile)
                colorType = colorType.GetOppositeColor();

            if (Board.Instance.blocks.TryGetValue(curPos, out BlockData blockData))
            {
                if (blockData is MirrorBlock mirrorBlock)
                    ChangeDirectionDueToMirror(ref direction, mirrorBlock.isBottomLeftToTopRight);
            }

            if (Board.Instance.CheckGameClear())
                break;
            yield return waitColorOneTile;
        }
        // particle.Stop();
    }

    private void DoParticleEffect(Vector2Int curPos, Vector2Int direction, TileType color)
    {
        if (particle == null) return;
        var main = particle.main;
        if (color == TileType.Color1)
            main.startColor = Board.Instance.colorPaletteSO.color1;
        else
            main.startColor = Board.Instance.colorPaletteSO.color2;

        particle.transform.DOMove(Board.Instance.GetTilePos(curPos.x, curPos.y), colorOneTileSpeed).SetEase(Ease.Linear);

        Vector3 rotation;
        if (direction == Vector2Int.up) rotation = new Vector3(0, 0, 0);
        else if (direction == Vector2Int.right) rotation = new Vector3(0, 0, -90);
        else if (direction == Vector2Int.down) rotation = new Vector3(0, 0, 180);
        else rotation = new Vector3(0, 0, 90);
        particle.transform.rotation = Quaternion.Euler(rotation);
    }

    private void ChangeDirectionDueToMirror(ref Vector2Int direction, bool isBottomLeftToTopRight)
    {
        if (isBottomLeftToTopRight) // 대각선 (/ 모양)
        {
            if (direction == Vector2Int.up) direction = Vector2Int.right;
            else if (direction == Vector2Int.right) direction = Vector2Int.up;
            else if (direction == Vector2Int.down) direction = Vector2Int.left;
            else if (direction == Vector2Int.left) direction = Vector2Int.down;
        }
        else // 대각선 (\ 모양)
        {
            if (direction == Vector2Int.up) direction = Vector2Int.left;
            else if (direction == Vector2Int.left) direction = Vector2Int.up;
            else if (direction == Vector2Int.down) direction = Vector2Int.right;
            else if (direction == Vector2Int.right) direction = Vector2Int.down;
        }
    }
}