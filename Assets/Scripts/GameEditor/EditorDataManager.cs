#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class EditorTileInfo
{
    public GameObject tile;
    public SpriteRenderer spriter;
    public TileType type;

    public EditorTileInfo(GameObject tile, TileType type)
    {
        this.tile = tile;
        this.spriter = tile.GetComponent<SpriteRenderer>();
        this.type = type;
    }
}

public class EditorTargetInfo
{
    public GameObject target;
    public TileType type;

    public EditorTargetInfo(GameObject target, TileType type)
    {
        this.target = target;
        this.type = type;
    }
}

public class EditorDataManager : MonoBehaviour
{
    [SerializeField] private EditorTileDrawer tileDrawer;
    [Header("기존의 BoardSO를 불러오려면 등록해 주세요")]
    [SerializeField] private BoardSO sourceBoardSO;
    private Vector2Int startPos;
    private Dictionary<Vector2Int, EditorTileInfo> tiles;
    private Dictionary<Vector2Int, GameObject> specialTileObjects;
    private Dictionary<Vector2Int, int> extraIntTileDatas;
    private Dictionary<Vector2Int, float> extraFloatTileDatas;
    private Dictionary<Vector2Int, EditorTargetInfo> targets;

    private void Start()
    {
        tiles = new Dictionary<Vector2Int, EditorTileInfo>();
        specialTileObjects = new Dictionary<Vector2Int, GameObject>();
        extraIntTileDatas = new Dictionary<Vector2Int, int>();
        extraFloatTileDatas = new Dictionary<Vector2Int, float>();
        targets = new Dictionary<Vector2Int, EditorTargetInfo>();

        if (sourceBoardSO != null)
        {
            LoadBoardSO();
        }
        else
        {
            startPos = new Vector2Int(100000, 100000);
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    AddTile(new Vector2Int(i, j));
        }
    }

    public void Input(Vector2Int pos)
    {
        switch (ToggleManager.Instance.tool)
        {
            case TileEditingTool.GenerateTile: // 타일 생성
                AddTile(pos);
                break;
            case TileEditingTool.ChangeTileColor1: // Color1 색칠
                ChangeTileColor(pos, TileType.Color1);
                break;
            case TileEditingTool.ChangeTileColor2: // Color2 색칠
                ChangeTileColor(pos, TileType.Color2);
                break;
            case TileEditingTool.ChangeTileColor12: // Color12 색칠
                ChangeTileColor(pos, TileType.Color12);
                break;
            case TileEditingTool.ChangeTileColorBlack: // Black 색칠
                ChangeTileColor(pos, TileType.Black);
                break;
            case TileEditingTool.DeleteTile: // 타일 삭제
                DeleteTile(pos);
                break;
            case TileEditingTool.SetStartPos: // 시작 위치 설정
                SetStartPos(pos);
                break;
            case TileEditingTool.AddTarget: // target영역 추가
                AddTarget(pos, TileType.Color12);
                break;
            case TileEditingTool.AddColor1Paint: // Color1 페인트 추가
                AddObject(pos, TileType.Color1Paint);
                break;
            case TileEditingTool.AddColor2Paint: // Color2 페인트 추가
                AddObject(pos, TileType.Color2Paint);
                break;
            case TileEditingTool.AddReversePaint:
                AddObject(pos, TileType.ReversePaint);
                break;
            case TileEditingTool.AddSpray:
                AddObject(pos, TileType.Spray);
                break;
        }
    }

    public void Save()
    {
        if (!CanSave()) return;

        string name = (sourceBoardSO == null ? "Stage1Board1" : sourceBoardSO.name) 
                      +" (Assets/Resources/ScriptableObjects/Board/Stage{N}에 저장해 주세요)";
	    string path = "Assets/Resources/ScriptableObjects/Board/";
	    string fullPath = EditorUtility.SaveFilePanelInProject(
                          "Save Board Data", name, "asset", "Save board data to an asset file.", path);
        if (!string.IsNullOrEmpty(path))
        {
            BoardSO boardData = GetBoardSOData();
            AssetDatabase.CreateAsset(boardData, fullPath);
            AssetDatabase.SaveAssets();
            Logger.Log("Board Data saved.");
        }
    }

    private bool CanSave()
    {
        if (startPos == new Vector2Int(100000, 100000))
        {
            Logger.LogWarning("플레이어가 시작할 위치를 선택해 주세요");
            return false;
        }
        if (tiles.Count == 0)
        {
            Logger.LogWarning("타일을 생성해 주세요");
            return false;
        }

        return true;
    }

    private void AddTile(Vector2Int pos)
    {
        if (tiles.ContainsKey(pos))
            return;

        GameObject tileObj = tileDrawer.AddTile(pos);
        tiles[pos] = new EditorTileInfo(tileObj, TileType.White);
    }

    private void ChangeTileColor(Vector2Int pos, TileType color)
    {
        if (tiles.TryGetValue(pos, out EditorTileInfo tileInfo))
        {
            if (!tileInfo.type.IsSpecialTile())
                ChangeTileColor(pos, tileInfo, color);
        }
    }

    private void DeleteTile(Vector2Int pos)
    {
        if (!tiles.TryGetValue(pos, out EditorTileInfo tileInfo))
            return;

        // 타겟 위치라면 타겟 삭제
        if (targets.TryGetValue(pos, out EditorTargetInfo targetInfo))
        {
            tileDrawer.DeleteTile(targetInfo.target);
            targets.Remove(pos);
            tileDrawer.UpdateTarget(pos, targets);
        }
        // 타일이 색칠되어 있지 않다면 타일 삭제
        else if (tileInfo.type == TileType.White)
        {
            tileDrawer.DeleteTile(tileInfo.tile);
            tiles.Remove(pos);
            if (startPos == pos)
            {
                tileDrawer.DeleteStartText();
                startPos = new Vector2Int(100000, 100000);
            }
        }
        else
        {
            // 특수타일이라면 특수타일 오브젝트 삭제 후 White로 바꿈
            if (tileInfo.type.IsSpecialTile())
            {
                DeleteExtraData(pos, tileInfo.type);

                tileDrawer.DeleteTile(specialTileObjects[pos]);
                specialTileObjects.Remove(pos);
            }
            ChangeTileColor(pos, tileInfo, TileType.White);
        }
    }

    private void ChangeTileColor(Vector2Int pos, EditorTileInfo tileInfo, TileType color)
    {
        tileInfo.type = color;
        tileDrawer.ChangeTileColor(tileInfo);
        tiles[pos] = tileInfo;
    }

    private void SetStartPos(Vector2Int pos)
    {
        if (tiles.ContainsKey(pos))
        {
            startPos = pos;
            tileDrawer.SetStartText(pos);
        }
    }

    private void AddTarget(Vector2Int pos, TileType color)
    {
        if (targets.ContainsKey(pos) || !tiles.ContainsKey(pos))
            return;

        GameObject targetObj = tileDrawer.AddTarget(pos, color, targets);
        targets[pos] = new EditorTargetInfo(targetObj, color);
    }

    private void AddObject(Vector2Int pos, TileType type)
    {
        if (!tiles.TryGetValue(pos, out EditorTileInfo tileInfo))
            return;
        if (!CanParseInputField(type))
            return;

        if (tileInfo.type.IsSpecialTile())
        {
            DeleteExtraData(pos, tileInfo.type);
            if (specialTileObjects.TryGetValue(pos, out GameObject obj))
                Destroy(obj);
        }

        Logger.Log($"Created {type} at {pos}");

        AddObjectExtraData(pos, type);
        tileInfo.type = type;
        tiles[pos] = tileInfo;
        

        GameObject tileObj = tileDrawer.AddObject(pos, type);
        specialTileObjects[pos] = tileObj;

        // 특수타일별 추가 처리
        switch (type)
        {
            case TileType.Color1Paint:
            case TileType.Color2Paint:
            case TileType.ReversePaint:
                tileDrawer.ChangeTileColor(tileInfo);
                break;
            case TileType.Spray:
                tileDrawer.SetTextOfObject(tileObj, extraIntTileDatas[pos].ToString());
                break;
        }
    }

    private bool CanParseInputField(TileType type)
    {
        string str = ToggleManager.Instance.inputField.text;
        bool success = true;
        if (type.NeedsIntData())
            if ((success = int.TryParse(str, out int intValue)) == false)
                Logger.LogWarning($"inputField를 int형으로 변환할 수 없습니다. : {str}");
        else if (type.NeedsFloatData())
            if ((success = float.TryParse(str, out float floatValue)) == false)
                Logger.LogWarning($"inputField를 float형으로 변환할 수 없습니다. : {str}");
        return success;
    }

    private void AddObjectExtraData(Vector2Int pos, TileType type)
    {
        string str = ToggleManager.Instance.inputField.text;
        if (type.NeedsIntData())
            if (int.TryParse(str, out int intValue))
                extraIntTileDatas[pos] = intValue;
        else if (type.NeedsFloatData())
            if (float.TryParse(str, out float floatValue))
                extraFloatTileDatas[pos] = floatValue;
    }

    private void DeleteExtraData(Vector2Int pos, TileType type)
    {
        if (type.NeedsIntData()) extraIntTileDatas.Remove(pos);
        else if (type.NeedsFloatData()) extraFloatTileDatas.Remove(pos);
    }

    private BoardSO GetBoardSOData()
    {
        int minX, maxX, minY, maxY;
        minX = minY = 100000000;
        maxX = maxY = -100000000;
        List<BoardSOTileData> boardTileList = new List<BoardSOTileData>();
        List<BoardSOTileData> targetTileList = new List<BoardSOTileData>();

        foreach (var pos in tiles.Keys)
        {
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
        }
        foreach (var entry in tiles)
        {
            Vector2Int pos = entry.Key - new Vector2Int(minX, minY);
            if (entry.Value.type.NeedsIntData())
                boardTileList.Add(new BoardSOIntTileData(pos, entry.Value.type, extraIntTileDatas[pos]));
            else if (entry.Value.type.NeedsFloatData())
                boardTileList.Add(new BoardSOFloatTileData(pos, entry.Value.type, extraFloatTileDatas[pos])); 
            else
                boardTileList.Add(new BoardSOTileData(pos, entry.Value.type));
        }
        foreach (var entry in targets)
        {
            Vector2Int pos = entry.Key - new Vector2Int(minX, minY);
            targetTileList.Add(new BoardSOTileData(pos, entry.Value.type));
        }

        BoardSO board = new BoardSO();
        board.n = maxX - minX + 1;
        board.m = maxY - minY + 1;
        board.startPos = startPos - new Vector2Int(minX, minY);
        board.boardTileList = boardTileList;
        board.targetTileList = targetTileList;
        return board;
    }

    private void LoadBoardSO()
    {
        foreach (var entry in sourceBoardSO.boardTileList)
        {
            AddTile(entry.pos);
            ChangeTileColor(entry.pos, tiles[entry.pos], entry.type);
            if (entry.type.NeedsIntData() && entry is BoardSOIntTileData intTileData)
                ToggleManager.Instance.inputField.text = intTileData.intValue.ToString();
            if (entry.type.IsSpecialTile())
                AddObject(entry.pos, entry.type);
        }
        foreach (var entry in sourceBoardSO.targetTileList)
        {
            AddTarget(entry.pos, TileType.Color12);
        }
        SetStartPos(sourceBoardSO.startPos);
    }
}
#endif