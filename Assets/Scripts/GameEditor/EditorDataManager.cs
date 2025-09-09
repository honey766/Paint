#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public struct EditorTileInfo
{
    public GameObject tile;
    public TileColor color;

    public EditorTileInfo(GameObject tile, TileColor color)
    {
        this.tile = tile;
        this.color = color;
    }
}

public struct EditorPaintInfo
{
    public GameObject paint;
    public TileColor color;

    public EditorPaintInfo(GameObject paint, TileColor color)
    {
        this.paint = paint;
        this.color = color;
    }
}

public class EditorDataManager : MonoBehaviour
{
    [SerializeField] private EditorTileDrawer tileDrawer;
    [Header("기존의 BoardSO를 불러오려면 등록해 주세요")]
    [SerializeField] private BoardSO sourceBoardSO;
    private Vector2Int startPos;
    private Dictionary<Vector2Int, EditorTileInfo> tiles;
    private Dictionary<Vector2Int, EditorPaintInfo> paints;

    private void Start()
    {
        tiles = new Dictionary<Vector2Int, EditorTileInfo>();
        paints = new Dictionary<Vector2Int, EditorPaintInfo>();
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
                ChangeTileColor(pos, TileColor.Color1);
                break;
            case TileEditingTool.ChangeTileColor2: // Color2 색칠
                ChangeTileColor(pos, TileColor.Color2);
                break;
            case TileEditingTool.ChangeTileColor12: // Color12 색칠
                ChangeTileColor(pos, TileColor.Color1 | TileColor.Color2);
                break;
            case TileEditingTool.ChangeTileColorBlack: // Black 색칠
                ChangeTileColor(pos, TileColor.Black);
                break;
            case TileEditingTool.DeleteTile: // 타일 삭제
                DeleteTile(pos);
                break;
            case TileEditingTool.SetStartPos: // 시작 위치 설정
                SetStartPos(pos);
                break;
            case TileEditingTool.AddColor1Paint: // Color1 페인트 추가
                AddPaint(pos, TileColor.Color1);
                break;
            case TileEditingTool.AddColor2Paint: // Color2 페인트 추가
                AddPaint(pos, TileColor.Color2);
                break;
            case TileEditingTool.AddBlackPaint: // Black 페인트 추가
                AddPaint(pos, TileColor.Black);
                break;
        }
    }

    public void Save()
    {
        if (!CanSave()) return;

        string name = (sourceBoardSO == null ? "Board1" : sourceBoardSO.name) 
                      +" (Assets/ScriptableObjects/Board에 저장해 주세요)";
        string path = EditorUtility.SaveFilePanelInProject("Save Board Data", name, "asset", "Save board data to an asset file.");
        if (!string.IsNullOrEmpty(path))
        {
            BoardSO boardData = GetBoardSOData();
            AssetDatabase.CreateAsset(boardData, path);
            AssetDatabase.SaveAssets();
            Debug.Log("Board Data saved.");
        }
    }

    private bool CanSave()
    {
        if (startPos == new Vector2Int(100000, 100000))
        {
            Debug.LogWarning("플레이어가 시작할 위치를 선택해 주세요");
            return false;
        }
        if (tiles.Count == 0)
        {
            Debug.LogWarning("타일을 생성해 주세요");
            return false;
        }
        if (paints.Count == 0)
        {
            Debug.LogWarning("페인트가 지정되어 있지 않아 저장할 수 없습니다.");
            return false;
        }

        return true;
    }

    private void AddTile(Vector2Int pos)
    {
        if (tiles.ContainsKey(pos))
            return;

        GameObject tileObj = tileDrawer.AddTile(pos);
        tiles[pos] = new EditorTileInfo(tileObj, TileColor.None);
    }

    private void ChangeTileColor(Vector2Int pos, TileColor color)
    {
        if (paints.ContainsKey(pos))
            return;

        if (tiles.TryGetValue(pos, out EditorTileInfo tileValue))
        {
            ChangeTileColor(pos, tileValue, color);
        }
    }

    private void DeleteTile(Vector2Int pos)
    {
        // 페인트 먼저 삭제
        if (paints.TryGetValue(pos, out EditorPaintInfo paintValue))
        {
            tileDrawer.DeleteTile(paintValue.paint);
            paints.Remove(pos);
        }
        // 타일
        else if (tiles.TryGetValue(pos, out EditorTileInfo tileValue))
        {
            // 타일이 색칠되어 있지 않다면 타일 삭제
            if (tileValue.color == TileColor.None)
            {
                tileDrawer.DeleteTile(tileValue.tile);
                tiles.Remove(pos);
                if (startPos == pos)
                {
                    tileDrawer.DeleteStartText();
                    startPos = new Vector2Int(100000, 100000);
                }
            }
            // 타일에 색칠되어 있다면 색깔 삭제
            else
            {
                ChangeTileColor(pos, tileValue, TileColor.None);
            }
        }
    }

    private void SetStartPos(Vector2Int pos)
    {
        if (tiles.ContainsKey(pos))
        {
            startPos = pos;
            tileDrawer.SetStartText(pos);
        }
    }

    private void AddPaint(Vector2Int pos, TileColor color)
    {
        bool existPaint = false;
        EditorPaintInfo paintValue;
        if (paints.TryGetValue(pos, out paintValue))
        {
            if (paintValue.color == color)
                return;
            existPaint = true;
        }

        if (tiles.TryGetValue(pos, out EditorTileInfo tileValue))
        {
            if (existPaint)
                Destroy(paintValue.paint);

            GameObject paintObj = tileDrawer.AddPaint(pos, color);
            paints[pos] = new EditorPaintInfo(paintObj, color);
            
            // 페인트 자리는 실제 타일의 색은 상관이 없고 미관상 색칠함
            ChangeTileColor(pos, tileValue, color);
        }
    }

    private void ChangeTileColor(Vector2Int pos, EditorTileInfo tileValue, TileColor color)
    {
        tileValue.color = color;
        tileDrawer.ChangeTileColor(tileValue);
        tiles[pos] = tileValue;
    }

    private BoardSO GetBoardSOData()
    {
        int minX, maxX, minY, maxY;
        minX = minY = 100000000;
        maxX = maxY = -100000000;
        List<TileData> tileList = new List<TileData>();
        List<PaintData> paintList = new List<PaintData>();

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
            tileList.Add(new TileData(pos, entry.Value.color));
        }
        foreach (var entry in paints)
        {
            Vector2Int pos = entry.Key - new Vector2Int(minX, minY);
            paintList.Add(new PaintData(pos, entry.Value.color));
        }

        BoardSO board = new BoardSO();
        board.n = maxX - minX + 1;
        board.m = maxY - minY + 1;
        board.startPos = startPos - new Vector2Int(minX, minY);
        board.tileList = tileList;
        board.paintList = paintList;
        return board;
    }

    private void LoadBoardSO()
    {
        foreach (var entry in sourceBoardSO.tileList)
        {
            AddTile(entry.pos);
            ChangeTileColor(entry.pos, tiles[entry.pos], entry.color);
        }
        foreach (var entry in sourceBoardSO.paintList)
        {
            AddPaint(entry.pos, entry.color);
        }
        SetStartPos(sourceBoardSO.startPos);
    }
}
#endif