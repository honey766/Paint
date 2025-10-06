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
    Spray, // 플레이어가 진입한 방향으로 n칸을 플레이어의 색으로 색칠하는 특수타일
    DirectedSpray, // 특정 방향으로만 색칠하는 Spray
    Ice, // 플레이어나 블록이 진입하면 해당 방향으로 타일 끝까지 미끄러짐
    WhitePaint,

    ////// Block //////
    Player = 10000,
    Mirror, // Spray가 만나면 Spray의 방향이 꺾임
    Brush, // 플레이어와 동일한 로직을 수행
    JustBlock // 그냥 기본 블록
}
#endregion


#region TileTypeExtensions
// TileType의 함수 익스텐션
public static class TileTypeExtensions
{
    private static HashSet<TileType> shouldDrawTileSet = new HashSet<TileType> {
        TileType.White, TileType.Color1, TileType.Color2, TileType.Color12, TileType.Black,
        TileType.Color1Paint, TileType.Color2Paint, TileType.ReversePaint
    };

    private static HashSet<TileType> blockSet = new HashSet<TileType> {
        TileType.Player, TileType.Mirror, TileType.Brush, TileType.JustBlock
    };

    public static bool IsNormalTile(this TileType tile)
    {
        return tile == TileType.White || tile == TileType.Black || tile == TileType.Color1 ||
               tile == TileType.Color2 || tile == TileType.Color12;
    }

    /// <summary>
    /// 타일에 특정 색을 추가
    /// </summary>
    public static TileType AddColorToNormalTile(this TileType tile, TileType color)
    {
        // NormalTile이 아닌 경우
        if (tile == TileType.None || IsSpecialTile(tile) || color == TileType.None || IsSpecialTile(color))
            return tile;

        if (tile == TileType.White || color == TileType.Black || color == TileType.White)
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
    /// Color1 <-> Color2 중 자신과 다른 Color을 반환. 둘 중 하나의 타입이 아니라면 TileType.None 반환
    /// </summary>
    public static TileType GetOppositeColor(this TileType tile)
    {
        if (tile == TileType.Color1) return TileType.Color2;
        if (tile == TileType.Color2) return TileType.Color1;
        return TileType.None;
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

    // 타일 위에 올라가는 BlockData형식의 타입인지 확인
    public static bool IsBlock(this TileType tile)
    {
        return blockSet.Contains(tile);
    }

    // 타일 생성 시에 Int데이터가 추가적으로 필요한 타입인지 검사
    public static bool NeedsIntData(this TileType tile)
    {
        return tile == TileType.Spray || tile == TileType.DirectedSpray ||
               tile == TileType.Mirror;
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
            Logger.Log($"TileFactory Initialized: {mapping.type}");
            _tilePrefabDict[mapping.type] = mapping.prefab;
        }
    }

    public static T CreateTile<T>(BoardSOTileData boardSOTileData) where T : MonoBehaviour
    {
        if (typeof(T) != typeof(TileData) && typeof(T) != typeof(BlockData))
        {
            Logger.LogError($"잘못된 타입 {typeof(T)}이 입력되었습니다. TileData 또는 BlockData를 넣어 주세요.");
            return null;
        }

        int i = boardSOTileData.pos.x;
        int j = boardSOTileData.pos.y;
        TileType type = boardSOTileData.type;

        if (type.IsBlock() && typeof(T) == typeof(TileData) || !type.IsBlock() && typeof(T) == typeof(BlockData))
            return null;
        if (!_tilePrefabDict.TryGetValue(type, out GameObject prefab))
        {
            Logger.LogError($"{boardSOTileData.type}에 해당하는 프리팹이 TileFactoryConfigSO에 설정되지 않았습니다.");
            return null;
        }

        Vector2 worldPos = Board.Instance.GetTilePos(i, j);
        Transform parent = type.IsBlock() ? Board.Instance.blockParent : Board.Instance.tileParent;
        GameObject tileInstance = UnityEngine.Object.Instantiate(prefab,
                                    worldPos, Quaternion.identity, parent);
        tileInstance.name = $"{type}({i},{j})";

        // 프리팹에 이미 컴포넌트가 있다고 가정
        T data = tileInstance.GetComponent<T>();
        if (data == null)
        {
            Logger.LogError($"[TileFactory] '{prefab.name}' 프리팹에 {typeof(T)}를 상속하는 컴포넌트가 없습니다. " +
                   $"(요청된 타입: {boardSOTileData.type}). " +
                   $"프리팹을 확인하고 {((typeof(T) == typeof(TileData)) ? "NormalTile" : "MirrorBlock")}  같은 스크립트를 추가해주세요.");
            return null;
        }

        // 초기화 메소드 호출
        if (data is TileData td) td.Initialize(boardSOTileData);
        else if (data is BlockData bd) bd.Initialize(boardSOTileData);
        return data;
    }
}
#endregion


#region TileData
public abstract class TileData : MonoBehaviour
{
    public TileType Type { get; protected set; }

    protected SpriteRenderer spriter;
    protected float paintTime = 0.25f;

    public abstract void OnBlockEnter(BlockData block, Vector2Int pos, Vector2Int direction, TileType color, float moveTime);

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