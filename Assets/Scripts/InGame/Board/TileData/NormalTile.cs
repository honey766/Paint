using UnityEngine;
using System.Collections;

public class NormalTile : TileData
{
    private static readonly int AddColorID = Shader.PropertyToID("_AddColor");
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int RatioID = Shader.PropertyToID("_ratio");
    private static readonly int RandomNoiseID = Shader.PropertyToID("_randomNoise");

    public override void OnBlockEnter(BlockData block, Vector2Int pos, Vector2Int direction, TileType color, float moveTime)
    {
        if (block.HasColor)
        {
            Type = Type.AddColorToNormalTile(color);
            WaitAndDrawTile(moveTime / 2f);
        }
    }

    public void SetTileColor(TileType type, float waitTime)
    {
        if (type == TileType.None || type.IsSpecialTile())
            return;

        Type = type;
        WaitAndDrawTile(waitTime);
    }

    public void AddTileColorForSprayTile(TileType addType)
    {
        if (addType == TileType.None || addType.IsSpecialTile())
            return;

        TileType prevType = Type;
        Type = Type.AddColorToNormalTile(addType);

        Color curColor;
        float paintTimeRate = 1f;
        float noiseRate = 1f;
        if (prevType == Type)
        {
            paintTimeRate = 1.6f;
            noiseRate = 0.7f;
            if (Type == TileType.Color12)
            {
                float weight = addType == TileType.Color1 ? 2.5f : 3.5f;
                curColor = (weight * Board.Instance.GetColorByType(Type) + Board.Instance.GetColorByType(addType)) / (weight + 1);
            }
            else
                curColor = (Color.white + 4 * Board.Instance.GetColorByType(Type)) / 5f;
        }
        else
        {
            curColor = spriter.material.GetColor(AddColorID);
        }   
        DrawTile(curColor, paintTimeRate, noiseRate);
    }

    public void WaitAndDrawTile(float waitTime)
    {
        Invoke(nameof(DrawTile), waitTime);
    }

    public void DrawTile()
    {
        Color curColor = spriter.material.GetColor(AddColorID);
        DrawTile(curColor, 1, 1);
    }

    public void DrawTile(Color curColor, float paintTimeRate, float noiseRate)
    {
        // 필수 컴포넌트 검증
        if (spriter == null)
        {
            Debug.LogWarning($"Cannot draw tile: SpriteRenderer is null for {Type}");
            return;
        }

        // 이 타일 타입이 그려져야 하는지 확인
        if (!Type.ShouldDrawTile())
        {
            Debug.LogWarning($"DrawTile called for {Type}, but this type doesn't support drawing");
            return;
        }

        StopAllCoroutines();
        Board.Instance.boardTypeForRedo[pos] = Type;
        //AudioManager.Instance.PlaySfx(SfxType.ColorTile);

        Color prevAddColor = spriter.material.GetColor(AddColorID);
        //spriter.material.SetColor(BaseColorID, curColor);
        StartCoroutine(BaseColorCoroutine(prevAddColor, curColor));

        spriter.material.SetColor(AddColorID, Board.Instance.GetColorByType(Type));
        spriter.material.SetFloat(RatioID, 0);
        spriter.material.SetFloat(RandomNoiseID, Random.Range(0f, 1.2f * noiseRate));

        if (gameObject.activeInHierarchy)
            StartCoroutine(DrawTileCoroutine(paintTimeRate));
        else
		    spriter.material.SetFloat(RatioID, 1);
    }

    protected IEnumerator DrawTileCoroutine(float paintTimeRate)
    {
        if (spriter != null)
        {
            yield return MyCoroutine.WaitFor(paintTime * paintTimeRate * Random.Range(0.85f, 1f), (t) =>
            {
                spriter.material.SetFloat(RatioID, t + 0.01f);
            });
        }
    }

    protected IEnumerator BaseColorCoroutine(Color prevAddColor, Color curColor)
    {
        if (spriter != null)
        {
            yield return MyCoroutine.WaitFor(paintTime * 0.4f * Random.Range(0.85f, 1f), (t) =>
            {
                spriter.material.SetColor(BaseColorID, Color.Lerp(prevAddColor, curColor, t));
            });
        }
    }
}