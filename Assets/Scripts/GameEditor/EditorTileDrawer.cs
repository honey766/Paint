#if UNITY_EDITOR
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
    [SerializeField]
    GameObject color1Paint, color2Paint, reversePaint, whitePaint, spray, directedSpray, 
                                ice, mirror, stamp, justBlock;
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
            case TileType.DirectedSpray:
                return Instantiate(directedSpray, (Vector2)pos, Quaternion.identity, objectParent);
            case TileType.Ice:
                return Instantiate(ice, (Vector2)pos, Quaternion.identity, objectParent);
            case TileType.Mirror:
                return Instantiate(mirror, (Vector2)pos, Quaternion.identity, objectParent);
            case TileType.Brush:
                return Instantiate(stamp, (Vector2)pos, Quaternion.identity, objectParent);
            case TileType.WhitePaint:
                return Instantiate(whitePaint, (Vector2)pos, Quaternion.identity, objectParent);
            case TileType.JustBlock:
                return Instantiate(justBlock, (Vector2)pos, Quaternion.identity, objectParent);
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

    public void SetDirectedSpray(GameObject obj, int encodedValue)
    {
        EditorDataFormat.DecodeDirectedSpray(encodedValue,
                out int paintCount, out Vector2Int direction, out bool doPaintReverse);
        Transform triangle = obj.transform.GetChild(1);
        SpriteRenderer spriterTriangle = triangle.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer spriterOutline = triangle.GetChild(1).GetComponent<SpriteRenderer>();

        SetTextOfObject(obj, paintCount.ToString());

        triangle.rotation = CustomTools.GetRotationByDirection(direction);

        if (doPaintReverse)
        {
            spriterTriangle.color = new Color(0.5f, 0.25f, 0.67f, 0.5f);
            spriterOutline.color = new Color(0.6f, 0.45f, 0.7f, 0.9f);
        }
        else
        {
            spriterTriangle.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            spriterOutline.color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
        }
    }

    public void SetMirror(GameObject obj, int encodedValue)
    {
        if (encodedValue == 0) // isBottomLeftToTopRight == false
            obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        else
            obj.transform.rotation = Quaternion.identity;
    }

    public void SetBrush(GameObject obj, int encodedValue)
    {
        SpriteRenderer spriter = obj.GetComponent<SpriteRenderer>();
        if (encodedValue == 1)
            spriter.color = new Color(1, 0.45f, 0.41f, 0.6f);
        else if (encodedValue == 2)
            spriter.color = new Color(0.24f, 0.57f, 1, 0.6f);
    }
}
#endif