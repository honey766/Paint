using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor;

[System.Serializable]
public struct TypePrefabPair
{
    public TileType type;
    public GameObject prefab;
}

public class HintDrawer : MonoBehaviour
{
    [SerializeField] private List<TypePrefabPair> prefabs;
    [SerializeField] private GameObject outlinePrefab;
    [SerializeField] private GameObject leftButton, rightButton;
    [SerializeField] private GameObject pageDotPrefab;
    [SerializeField] private Transform pageDotsParent;
    [SerializeField] private Sprite playerBursh, playerErasor;
    [SerializeField] private Sprite brushBursh, brushErasor;

    private int curPage, numOfPages;
    private RectTransform parentRect;
    private float width, height, heightOffset, tileSize;
    private int n, m;
    private Dictionary<TileType, GameObject> prefabDict;
    private GameObject[] pageObjs;
    private RectTransform[] pageDotRects;

    private const float TileOutlineOffset = 0.015f;
    private Transform tiles, blocks;
    private BoardSO curBoardSO;
    private Dictionary<Vector2Int, TileData>.KeyCollection board;

    public void Draw(BoardSO[] boardSO)
    {
        numOfPages = boardSO.Length;
        n = boardSO[0].n; m = boardSO[0].m;
        Init();

        pageObjs = new GameObject[numOfPages];
        pageDotRects = new RectTransform[numOfPages];
        for (int i = 0; i < numOfPages; i++)
        {
            pageObjs[i] = Draw(boardSO[i]);
            pageObjs[i].SetActive(i == 0);
            if (numOfPages > 1)
                pageDotRects[i] = Instantiate(pageDotPrefab, pageDotsParent).GetComponent<RectTransform>();
        }
        if (numOfPages > 1) pageDotRects[0].sizeDelta = Vector2.one * 40;
    }

    private void Init()
    {
        board = Board.Instance.board.Keys;
        curPage = 0;
        
        prefabDict = new();
        foreach (var entry in prefabs)
            prefabDict[entry.type] = entry.prefab;

        float widthOffset = numOfPages > 1 ? 180 : 80;
        heightOffset = numOfPages > 1 ? 60 : 0;
        parentRect = GetComponent<RectTransform>();
        width = parentRect.rect.width - widthOffset;
        height = parentRect.rect.height - 220 - heightOffset;
        tileSize = Mathf.Min(width / Mathf.Max(5.5f, n), height / Mathf.Max(5.5f, m));

        leftButton.SetActive(false);
        rightButton.SetActive(numOfPages > 1);
    }

    private GameObject Draw(BoardSO boardSO)
    {
        curBoardSO = boardSO;

        Transform canvas = new GameObject("HintCanvas", typeof(RectTransform)).transform;
        canvas.SetParent(transform, false);

        tiles = new GameObject("Tiles", typeof(RectTransform)).transform;
        tiles.SetParent(canvas, false);

        Transform lines = new GameObject("Lines", typeof(RectTransform)).transform;
        lines.SetParent(canvas, false);

        blocks = new GameObject("Blocks", typeof(RectTransform)).transform;
        blocks.SetParent(canvas, false);

        foreach (BoardSOTileData myTile in boardSO.boardTileList)
            DrawTile(myTile);
        DrawTile(new BoardSOTileData(boardSO.startPos, TileType.Player));

        DrawTileOutline(lines.transform);

        return canvas.gameObject;
    }
    
    private void DrawTile(BoardSOTileData myTile)
    {
        Vector2 pos = GetPosByGrid(myTile.pos);
        Transform parent = myTile.type.IsBlock() ? blocks : tiles;
        GameObject tile = Instantiate(prefabDict[myTile.type], parent);
        tile.GetComponent<RectTransform>().anchoredPosition = pos;
        tile.transform.localScale = Vector2.one * tileSize / 100f;
        SetTileExtraLogic(tile, myTile);
    }

    private void SetTileExtraLogic(GameObject tile, BoardSOTileData tileData)
    {
        switch (tileData.type)
        {
            case TileType.White:
            case TileType.Color1:
            case TileType.Color2:
            case TileType.Color12:
            case TileType.Black:
                tile.GetComponent<Image>().color = Board.Instance.GetColorByType(tileData.type);
                break;
            case TileType.DirectedSpray:
                if (tileData is BoardSOIntTileData intTileData)
                {
                    EditorDataFormat.DecodeDirectedSpray(intTileData.intValue,
                        out int paintCount, out Vector2Int direction, out bool doPaintReverse);
                    tile.transform.rotation = CustomTools.GetRotationByDirection(direction);
                    tile.transform.GetChild(1).rotation = Quaternion.identity;
                }
                break;
            case TileType.Mirror:
                if (tileData is BoardSOIntTileData intTileData2)
                {
                    bool isBottomLeftToTopRight = intTileData2.intValue == 1;
                    if (isBottomLeftToTopRight)
                        tile.transform.localScale = new Vector3(-1, 1, 1) * tileSize / 100f;
                }
                break;
            case TileType.Brush:
                if (tileData is BoardSOIntTileData intTileData3)
                {
                    TileType brushColor = TileType.None;
                    if (intTileData3.intValue == 1) brushColor = TileType.Color1;
                    else if (intTileData3.intValue == 2) brushColor = TileType.Color2;
                    // tile.GetComponent<Image>().color = Board.Instance.GetColorByType(bruchColor);
                    ApplyBrushAndPlayerColor(tile.transform.GetChild(0), brushColor, false);
                }
                break;
            case TileType.Player:
                ApplyBrushAndPlayerColor(tile.transform.GetChild(0), curBoardSO.startPlayerColor, true);
                // tile.transform.GetChild(1).GetComponent<Image>().color
                //     = Board.Instance.GetColorByType(curBoardSO.startPlayerColor);
                // Sprite toolSprite = curBoardSO.startPlayerColor == TileType.White ? playerErasor : playerBursh;
                // tile.transform.GetChild(2).GetComponent<Image>().sprite = toolSprite;
                break;
        }
    }
    
    private void ApplyBrushAndPlayerColor(Transform tile, TileType color, bool isPlayer)
    {
        tile.GetChild(1).GetComponent<Image>().color = Board.Instance.GetColorByType(color);

        Sprite toolSprite;
        if (isPlayer)
            toolSprite = color == TileType.White ? playerErasor : playerBursh;
        else
            toolSprite = color == TileType.White ? brushErasor : brushBursh;
        
        tile.GetChild(2).GetComponent<Image>().sprite = toolSprite;
        if (isPlayer)
        {
            Vector2 size = color == TileType.White ? new Vector2(82.8125f, 82.8125f) 
                                                    : new Vector2(60.9375f, 17.1875f);
            tile.GetChild(2).GetComponent<RectTransform>().sizeDelta = size;
        }
    }

    private Vector2 GetPosByGrid(Vector2Int pos)
        => new Vector2((pos.x - (n - 1) / 2f) * tileSize, (heightOffset - 100) / 2f + (pos.y - (m - 1) / 2f) * tileSize);
    private Vector2 GetPosByGrid(int i, int j)
        => new Vector2((i - (n - 1) / 2f) * tileSize, (heightOffset - 100) / 2f + (j - (m - 1) / 2f) * tileSize);

    public void DrawTileOutline(Transform parent)
    {
        Quaternion leftRot = Quaternion.Euler(0, 0, 90);

        for (int i = 0; i < n; i++)
            for (int j = 0; j < m + 1; j++)
                if (ExistsTile(i, j - 1) || ExistsTile(i, j))
                    InstantiateLine(GetPosByGrid(i, j) + Vector2.down * TileOutlineOffset * tileSize, leftRot, parent);

        for (int i = 0; i < n + 1; i++)
            for (int j = 0; j < m; j++)
                if (ExistsTile(i - 1, j) || ExistsTile(i, j))
                    InstantiateLine(GetPosByGrid(i, j) + Vector2.left * TileOutlineOffset * tileSize, Quaternion.identity, parent);
    }

    private bool ExistsTile(int i, int j)
    {
        if (i < 0 || i >= n || j < 0 || j >= m)
            return false;
        return board.Contains(new Vector2Int(i, j));
    }

    private void InstantiateLine(Vector2 pos, Quaternion rot, Transform parent)
    {
        GameObject line = Instantiate(outlinePrefab, pos, rot, parent);
        line.GetComponent<RectTransform>().anchoredPosition = pos;
        line.transform.localScale = Vector2.one * tileSize / 100f;
    }


    #region Button
    public void OnRightClick()
    {
        if (curPage >= numOfPages - 1) return;
        OnButtonClick(1);
    }

    public void OnLeftClick()
    {
        if (curPage <= 0) return;
        OnButtonClick(-1);
    }

    private void OnButtonClick(int diff)
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);

        pageObjs[curPage].SetActive(false);
        if (pageDotRects.Length > curPage)
            pageDotRects[curPage].DOSizeDelta(Vector2.one * 25, 0.2f);

        curPage += diff;

        rightButton.SetActive(curPage < numOfPages - 1);
        leftButton.SetActive(curPage > 0);
        pageObjs[curPage].SetActive(true);
        if (pageDotRects.Length > curPage)
            pageDotRects[curPage].DOSizeDelta(Vector2.one * 40, 0.2f);
    }
    #endregion
}