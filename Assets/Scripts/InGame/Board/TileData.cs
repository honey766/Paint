using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#region TileType
public enum TileType
{
    None = 0,
    White, Color1, Color2, Color12, Black,
    Color1Paint, Color2Paint, // 플레이어 색깔을 바꾸는 타일
    ReversePaint, // 플레이어의 색깔이 Color1 또는 Color2일 때 둘 중 다른 색으로 전환시키는 페인트
    Spray // 플레이어가 진입한 방향으로 n칸을 플레이어의 색으로 색칠하는 특수타일
}
#endregion


#region TileTypeExtensions
// TileType의 함수 익스텐션
public static class TileTypeExtensions
{
    private static HashSet<TileType> shouldDrawTileSet = new HashSet<TileType>{
        TileType.White, TileType.Color1, TileType.Color2, TileType.Color12, TileType.Black,
        TileType.Color1Paint, TileType.Color2Paint, TileType.ReversePaint
    };

    /// <summary>
    /// 타일에 특정 색을 추가
    /// </summary>
    public static TileType AddColorToNormalTile(this TileType tile, TileType color)
    {
        // NormalTile이 아닌 경우
        if (tile == TileType.None || IsSpecialTile(tile) || color == TileType.None || IsSpecialTile(color))
            return tile;

        if (tile == TileType.White || color == TileType.Black)
        {
            return color;
        }
        else if (tile == TileType.Color1 && color == TileType.Color2 ||
                 tile == TileType.Color2 && color == TileType.Color1)
        {
            return TileType.Color12;
        }

        return tile;
    }

    /// <summary>
    /// 단순한 타일이 아닌 특수타일인지 검사
    /// </summary>
    public static bool IsSpecialTile(this TileType tile)
    {
        return tile >= TileType.Color1Paint;
    }

    // DrawTile이 필요한 타일 타입들을 명시적으로 정의
    public static bool ShouldDrawTile(this TileType tile)
    {
        return shouldDrawTileSet.Contains(tile);
    }

    // 타일 생성 시에 Int데이터가 추가적으로 필요한 타입인지 검사
    public static bool NeedsIntData(this TileType tile)
    {
        return tile == TileType.Spray;
    }

    public static bool NeedsFloatData(this TileType tile)
    {
        return false;
    }
}
#endregion


#region TileFactory
public static class TileFactory
{
    private static Dictionary<TileType, GameObject> _tilePrefabDict;

    // 게임 시작 시 한 번만 호출하여 딕셔너리를 초기화하는 메소드
    public static void Initialize(TileFactoryConfigSO config)
    {
        if (_tilePrefabDict != null) return;
        _tilePrefabDict = new Dictionary<TileType, GameObject>();
        foreach (TileTypeMapping mapping in config.tileMappings)
        {
            _tilePrefabDict[mapping.type] = mapping.prefab;
        }
    }

    public static TileData CreateTile(BoardSOTileData boardSOTileData)
    {
        int i = boardSOTileData.pos.x;
        int j = boardSOTileData.pos.y;
        TileType type = boardSOTileData.type;

        if (!_tilePrefabDict.TryGetValue(type, out GameObject prefab))
        {
            Logger.LogError($"{boardSOTileData.type}에 해당하는 프리팹이 TileFactoryConfigSO에 설정되지 않았습니다.");
            return null;
        }

        Vector2 worldPos = Board.Instance.GetTilePos(i, j);
        GameObject tileInstance = UnityEngine.Object.Instantiate(prefab,
                                    worldPos, Quaternion.identity, Board.Instance.tileParent);
        tileInstance.name = $"{type}({i},{j})";

        // 프리팹에 이미 컴포넌트가 있다고 가정
        TileData tileData = tileInstance.GetComponent<TileData>();
        // tileInstance.name = $"{tileData.GetType().Name}({i},{j})";
        if (tileData == null)
        {
            Logger.LogError($"[TileFactory] '{prefab.name}' 프리팹에 TileData를 상속하는 컴포넌트가 없습니다. " +
                   $"(요청된 타입: {boardSOTileData.type}). " +
                   $"프리팹을 확인하고 NormalTile, PaintTile 같은 스크립트를 추가해주세요.");
            return null;
        }

        tileData.Initialize(boardSOTileData); // 초기화 메소드 호출
        return tileData;
    }
}
#endregion


#region TileData
public abstract class TileData : MonoBehaviour
{
    public TileType Type { get; protected set; }

    protected SpriteRenderer spriter;
    protected float paintTime = 0.25f;

    public abstract void OnPlayerEnter(PlayerController player, float moveTime);

    public virtual void Initialize(BoardSOTileData boardSOTileData)
    {
        Type = boardSOTileData.type;
        spriter = gameObject.GetComponent<SpriteRenderer>();
        spriter.material.SetColor("_BaseColor", Board.Instance.white);
        spriter.material.SetColor("_AddColor", Board.Instance.white);

        ValidateInitialization();
        if (Type.ShouldDrawTile())
            DrawTile();
    }

    // 초기화 검증 메서드
    protected virtual void ValidateInitialization()
    {
        if (spriter == null)
        {
            Logger.LogError($"SpriteRenderer component not found on {GetType().Name}");
        }
    }

    public void WaitAndDrawTile(float waitTime)
    {
        Invoke(nameof(DrawTile), waitTime);
    }

    public void DrawTile()
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

        Color curColor = spriter.material.GetColor("_AddColor");
        spriter.material.SetColor("_BaseColor", curColor);
        spriter.material.SetFloat("_ratio", 0);
        spriter.material.SetFloat("_randomNoise", Random.Range(0f, 1.2f));

        spriter.material.SetColor("_AddColor", Board.Instance.GetColorByType(Type));

        if (gameObject.activeInHierarchy)
            StartCoroutine(DrawTileCoroutine());
    }

    protected IEnumerator DrawTileCoroutine()
    {
        if (spriter != null)
        {
            yield return MyCoroutine.WaitFor(paintTime * Random.Range(0.85f, 1f), (t) =>
            {
                spriter.material.SetFloat("_ratio", t + 0.01f);
            });
        }
    }
}
#endregion