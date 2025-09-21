using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileTypeMapping
{
    public TileType type;
    public GameObject prefab; // 이 프리팹은 NormalTile, PaintTile 등의 스크립트를 이미 가지고 있어야 합니다.
}

[CreateAssetMenu(fileName = "TileFactoryConfig", menuName = "ScriptableObjects/TileFactoryConfig")]
public class TileFactoryConfigSO : ScriptableObject
{
    public List<TileTypeMapping> tileMappings;
}