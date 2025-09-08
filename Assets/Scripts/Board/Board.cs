using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public static class TileColorExtensions
{
    /// <summary>
    /// 타일에 특정 색을 추가
    /// </summary>
    public static void AddColor(ref this TileColor tile, TileColor color)
    {
        tile |= color;
    }

    /// <summary>
    /// 타일에서 특정 색을 제거
    /// </summary>
    public static void RemoveColor(ref this TileColor tile, TileColor color)
    {
        tile &= ~color;
    }
}

// 보드의 각 칸은 TileColor(enum, [Flags])로 관리한다.
// [Flags]를 쓰면 여러 enum값을 비트 OR(|)로 조합할 수 있다.
// 예: TileColor.Blue | TileColor.Red → "Blue, Red"
[Flags]
public enum TileColor
{
    None = 0,
    Color1 = 1 << 0,
    Color2 = 1 << 1,
    Change = 1 << 2 // 플레이어 색깔을 바꾸는 타일
}

public class Board : SingletonBehaviour<Board>
{
    public Transform tileParent;
    public GameObject tile;
    public Transform changeFlagParent;
    public GameObject changeFlag;

    public TileColor[,] board;  // 현재 보드 상태
    public TileColor[,] answer; // 목표 보드 상태
    public HashSet<Vector2Int> tileSet; // 타일 위치
    private int n, m;            // 세로, 가로 크기
    private GameObject[,] tileObjs; // 각 타일 오브젝트
    private SpriteRenderer[,] tileRends; // 각 타일의 스프라이트 렌더러

    [Header("색칠 색")]
    public Color gray;
    public ColorPaletteSO colorPallete;
    public GridBorderDrawer color1Border, color2Border, color12Border;
    public float paintTime = 0.2f;

    private BoardSO boardSO;

    public void InitBoard(BoardSO boardSO)
    {
        this.boardSO = boardSO;
        this.n = boardSO.n; this.m = boardSO.m;
        board = new TileColor[n, m];
        answer = new TileColor[n, m];
        tileObjs = new GameObject[n, m];
        tileRends = new SpriteRenderer[n, m];
        tileSet = new HashSet<Vector2Int>();
        InitBoard();
    }

    public void InitBoard()
    {
        foreach (var entry in boardSO.tileList)
        {
            Vector2Int grid = entry.pos;
            Vector2 pos = new Vector2(grid.x, grid.y) - new Vector2((n - 1) / 2f, (m - 1) / 2f);
            GameObject obj = Instantiate(tile, pos, Quaternion.identity, tileParent);
            obj.name = $"Tile[{grid.x},{grid.y}]";
            tileObjs[grid.x, grid.y] = obj;
            tileRends[grid.x, grid.y] = obj.GetComponent<SpriteRenderer>();

            board[grid.x, grid.y] = TileColor.None;
            answer[grid.x, grid.y] = entry.color;

            tileSet.Add(entry.pos);
        }
        // for (int i = 0; i < n; i++)
        // {
        //     for (int j = 0; j < m; j++)
        //     {
        //         Vector2 pos = new Vector2(i, j) - new Vector2((n - 1) / 2f, (m - 1) / 2f);
        //         GameObject obj = Instantiate(tile, pos, Quaternion.identity, tileParent);
        //         obj.name = $"Tile[{i},{j}]";
        //         tileObjs[i, j] = obj;
        //         tileRends[i, j] = obj.GetComponent<SpriteRenderer>();

        //         board[i, j] = TileColor.None;
        //         answer[i, j] = TileColor.None;
        //     }
        // }

        InitTileMats();

        foreach (var entry in boardSO.paintList)
        {
            Vector2Int grid = entry.pos;
            board[grid.x, grid.y] = entry.isColor1 ? TileColor.Color1 : TileColor.Color2;
            DrawTile(grid.x, grid.y);

            board[grid.x, grid.y].AddColor(TileColor.Change);

            Vector2 pos = new Vector2(grid.x, grid.y) - new Vector2((n - 1) / 2f, (m - 1) / 2f);
            Instantiate(changeFlag, pos, Quaternion.identity, changeFlagParent);
        }
        // for (int i = 0; i < 2; i++)
        // {
        //     int randI = Random.Range(0, n);
        //     int randJ = Random.Range(0, m);
        //     board[randI, randJ] = flagColor[i];
        //     DrawTile(randI, randJ);

        //     board[randI, randJ].AddColor(TileColor.Change);

        //     Vector2 pos = new Vector2(randI, randJ) - new Vector2((n - 1) / 2f, (m - 1) / 2f);
        //     Instantiate(changeFlag, pos, Quaternion.identity, changeFlagParent);
        // }

        color1Border.InitBorder(colorPallete.color1, TileColor.Color1, n, m, answer);
        color2Border.InitBorder(colorPallete.color2, TileColor.Color2, n, m, answer);
        color12Border.InitBorder(colorPallete.color12, TileColor.Color1 | TileColor.Color2, n, m, answer);
    }

    /// <summary>
    /// (i, j) 타일의 실제 좌표를 반환
    /// </summary>
    public Vector2 GetTilePos(int i, int j)
    {
        return new Vector2(i, j) - new Vector2((n - 1) / 2f, (m - 1) / 2f);
    }

    /// <summary>
    /// board와 answer가 같은지 검사
    /// </summary>
    public bool IsClear()
    {
        foreach (Vector2Int pos in tileSet)
            if (!board[pos.x, pos.y].HasFlag(TileColor.Change)
                && board[pos.x, pos.y] != answer[pos.x, pos.y])
                return false;
        return true;
    }

    /// <summary>
    /// (i,j) 타일에 color 색을 추가 시도. isAddColor가 false라면 색을 or로 추가하지 않고 =으로 대입함
    /// 이미 해당 색이 있거나 보드 범위 밖이면 false, 성공적으로 색이 추가되면 true 반환.
    /// </summary>
    public bool ColorTile(int i, int j, TileColor color, bool addColor = true)
    {
        if (i < 0 || i >= n || j < 0 || j >= m) return false;               // 범위 체크
        if (addColor && (board[i, j] & (TileColor.Change | color)) != 0) return false;  // 이미 해당 색이 있거나 색 바꾸는 타일임

        if (addColor) board[i, j].AddColor(color);  // 색 추가
        else board[i, j] = color; // 색 대입
        DrawTile(i, j);

        return true;
    }

    /// <summary>
    /// (i, j) 색을 바꾸는 타일이 아니며 타일에 특정 색이 포함되어 있는지 검사
    /// </summary>
    public bool HasColor(int i, int j, TileColor color)
    {
        if (i < 0 || i >= n || j < 0 || j >= m) return false;
        return !board[i, j].HasFlag(TileColor.Change) && board[i, j].HasFlag(color);
    }

    /// <summary>
    /// (i, j) 플레이어의 색을 바꾸는 타일인지 검사하고 바꾸는 색을 반환.
    /// 색을 바꾸지 않는다면 TileColor.None 반환
    /// </summary>
    public TileColor IsChangeTile(int i, int j)
    {
        if (i < 0 || i >= n || j < 0 || j >= m)
            return TileColor.None;
        if (board[i, j].HasFlag(TileColor.Change))
            return board[i, j] & ~TileColor.Change;
        return TileColor.None;
    }

    public bool IsInBounds(int i, int j)
    {
        return 0 <= i && i < n && 0 <= j && j < m;
    }

    private void InitTileMats()
    {
        foreach (Vector2Int pos in tileSet)
        {
            tileRends[pos.x, pos.y].material.SetColor("_BaseColor", gray);
            tileRends[pos.x, pos.y].material.SetColor("_AddColor", gray);
        }
    }

    /// <summary>
    /// 타일 상태에 따라 타일을 그림 (RGB 순서)
    /// </summary>
    public void DrawTile(int i, int j)
    {
        Color curColor = tileRends[i, j].material.GetColor("_AddColor");
        tileRends[i, j].material.SetColor("_BaseColor", curColor);
        tileRends[i, j].material.SetFloat("_ratio", 0);
        tileRends[i, j].material.SetFloat("_randomNoise", Random.Range(0f, 1f));

        // switch (board[i, j])
        // {
        //     case TileColor.None:
        //         tileObjs[i, j].GetComponent<SpriteRenderer>().color = gray;
        //         break;
        //     case TileColor.Red:
        //         tileObjs[i, j].GetComponent<SpriteRenderer>().color = red;
        //         break;
        //     case TileColor.Green:
        //         tileObjs[i, j].GetComponent<SpriteRenderer>().color = green;
        //         break;
        //     case TileColor.Blue:
        //         tileObjs[i, j].GetComponent<SpriteRenderer>().color = blue;
        //         break;
        //     case TileColor.Red | TileColor.Green:
        //         tileObjs[i, j].GetComponent<SpriteRenderer>().color = redGreen;
        //         break;
        //     case TileColor.Red | TileColor.Blue:
        //         tileObjs[i, j].GetComponent<SpriteRenderer>().color = redBlue;
        //         break;
        //     case TileColor.Green | TileColor.Blue:
        //         tileObjs[i, j].GetComponent<SpriteRenderer>().color = greenBlue;
        //         break;
        //     case TileColor.Red | TileColor.Green | TileColor.Blue:
        //         tileObjs[i, j].GetComponent<SpriteRenderer>().color = redGreenBlue;
        //         break;
        // }

        switch (board[i, j])
        {
            case TileColor.None:
                tileRends[i, j].material.SetColor("_AddColor", gray);
                break;
            case TileColor.Color1:
                tileRends[i, j].material.SetColor("_AddColor", colorPallete.color1);
                break;
            case TileColor.Color2:
                tileRends[i, j].material.SetColor("_AddColor", colorPallete.color2);
                break;
            case TileColor.Color1 | TileColor.Color2:
                tileRends[i, j].material.SetColor("_AddColor", colorPallete.color12);
                break;
        }
        StartCoroutine(ColorTileCoroutine(i, j));
    }

    private IEnumerator ColorTileCoroutine(int i, int j)
    {

        yield return MyCoroutine.WaitFor(paintTime, //PlayerController.Instance.moveTimePerTile,
        (t) =>
        {
            tileRends[i, j].material.SetFloat("_ratio", t + 0.01f);
        });
    }
}
