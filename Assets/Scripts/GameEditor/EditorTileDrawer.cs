using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EditorTileDrawer : MonoBehaviour
{
    [SerializeField] GameObject startText;
    [SerializeField] GameObject tile;
    [SerializeField] Transform tileParent;
    [SerializeField] GameObject target;
    [SerializeField] Transform targetParent;
    [SerializeField] GameObject color1Paint, color2Paint, reversePaint, spray;
    [SerializeField] Transform objectParent;
    [SerializeField] Color white, color1, color2, color12, black;

    private Vector2Int[] directions = {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    public GameObject AddTile(Vector2Int pos)
    {
        return Instantiate(tile, (Vector2)pos, Quaternion.identity, tileParent);
    }

    public void ChangeTileColor(EditorTileInfo tileInfo)
    {
        switch (tileInfo.type)
        {
            case TileType.White:
                tileInfo.spriter.color = white;
                break;
            case TileType.Color1:
            case TileType.Color1Paint:
                tileInfo.spriter.color = color1;
                break;
            case TileType.Color2:
            case TileType.Color2Paint:
                tileInfo.spriter.color = color2;
                break;
            case TileType.Color12:
                tileInfo.spriter.color = color12;
                break;
            case TileType.Black:
                tileInfo.spriter.color = black;
                break;
            case TileType.ReversePaint:
                tileInfo.spriter.color = (color12 + Color.white * 2f) / 3f;
                break;
        }
    }

    public void DeleteTile(GameObject tile)
    {
        Logger.Log($"Delete name :  {tile.name}");
        Destroy(tile);
    }

    public void SetStartText(Vector2Int pos)
    {
        if (!startText.activeSelf)
            startText.SetActive(true);
        startText.transform.position = (Vector2)pos;
    }

    public void DeleteStartText()
    {
        startText.SetActive(false);
    }

    public GameObject AddTarget(Vector2Int pos, TileType type, Dictionary<Vector2Int, EditorTargetInfo> targets)
    {
        GameObject targetObj = Instantiate(target, (Vector2)pos, Quaternion.identity, targetParent);
        // for (int i = 0; i < 4; i++)
        // {
        //     if (targets.TryGetValue(pos + directions[i], out EditorTargetInfo targetInfo))
        //     {
        //         targetObj.transform.GetChild(i).gameObject.SetActive(false);
        //         targetInfo.target.transform.GetChild((i + 2) % 4).gameObject.SetActive(false);
        //     }
        // }
        UpdateTarget(pos, targets, targetObj);
        return targetObj;
    }

    // pos 상하좌우 타일의 경계선을 업데이트
    public void UpdateTarget(Vector2Int pos, Dictionary<Vector2Int, EditorTargetInfo> targets, GameObject targetObj = null)
    {
        for (int i = 0; i < 4; i++)
        {
            if (targets.TryGetValue(pos + directions[i], out EditorTargetInfo targetInfo))
            {
                if (targetObj)
                {
                    targetObj.transform.GetChild(i).gameObject.SetActive(false);
                    targetInfo.target.transform.GetChild((i + 2) % 4).gameObject.SetActive(false);
                }
                else
                {
                    targetInfo.target.transform.GetChild((i + 2) % 4).gameObject.SetActive(true);
                }
            }
        }
    }

    public GameObject AddObject(Vector2Int pos, TileType type)
    {
        switch (type)
        {
            case TileType.Color1Paint:
                return Instantiate(color1Paint, (Vector2)pos, Quaternion.identity, objectParent);
            case TileType.Color2Paint:
                return Instantiate(color2Paint, (Vector2)pos, Quaternion.identity, objectParent);
            case TileType.ReversePaint:
                return Instantiate(reversePaint, (Vector2)pos, Quaternion.identity, objectParent);
            case TileType.Spray:
                return Instantiate(spray, (Vector2)pos, Quaternion.identity, objectParent);
        }
        return null;
    }

    public void SetTextOfObject(GameObject obj, string str)
    {
        TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            Logger.LogWarning($"[EditorTileDrawer] {obj}에 TextMeshProUGUI가 없어 실행에 실패했습니다.");
            return;
        }
        text.text = str;
    }
}
