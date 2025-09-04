using System;
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
    Red = 1 << 0,
    Green = 1 << 1,
    Blue = 1 << 2,
    Change = 1 << 3 // 플레이어 색깔을 바꾸는 타일
    // 필요하면 계속 추가
}

public class Board : SingletonBehaviour<Board>
{
    public Transform tileParent;
    public GameObject tile;
    public Transform changeFlagParent;
    public GameObject changeFlag;
    public int n, m;            // 세로, 가로 크기
    public TileColor[,] board;  // 현재 보드 상태
    public TileColor[,] answer; // 목표 보드 상태
    private GameObject[,] tileObjs; // 각 타일

    [Header("실제 색칠 색")]
    public Color gray;
    public Color red, green, blue, redGreen, redBlue, greenBlue, redGreenBlue;

    [Header("배경 색")]
    public Color redBack;
    public Color greenBack, blueBack, redGreenBack, redBlueBack, greenBlueBack, redGreenBlueBack;

    // 임시로 게임 시작하자마자 보드 생성
    private void Start()
    {
        InitBoard(n, m);
    }

    public void InitBoard(int n, int m)
    {
        this.n = n; this.m = m;
        board = new TileColor[n, m];
        answer = new TileColor[n, m];
        tileObjs = new GameObject[n, m];
        InitBoard();
    }

    public void InitBoard()
    {
        float scale = 0.2f;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                Vector2 pos = new Vector2(i, j) - new Vector2((n - 1) / 2f, (m - 1) / 2f);
                GameObject obj = Instantiate(tile, pos, Quaternion.identity, tileParent);
                obj.name = $"Tile[{i},{j}]";
                tileObjs[i, j] = obj;

                board[i, j] = TileColor.None;
                answer[i, j] = TileColor.None;

                // 🎲 Perlin Noise 값
                float noise = Mathf.PerlinNoise(i * scale, j * scale);

                // 기본은 노이즈 기반
                if (noise < 0.33f) answer[i, j].AddColor(TileColor.Red);
                if (noise > 0.33f && noise < 0.66f) answer[i, j].AddColor(TileColor.Green);
                if (noise > 0.66f) answer[i, j].AddColor(TileColor.Blue);

                // 🎲 인접한 타일 따라가기 (70% 확률)
                if (i > 0 && Random.value < 0.7f)
                    answer[i, j] |= answer[i - 1, j];
                if (j > 0 && Random.value < 0.7f)
                    answer[i, j] |= answer[i, j - 1];

                // DrawBackgroundTile(i, j);
            }
        }

        TileColor[] flagColor = new TileColor[] { TileColor.Red, TileColor.Green, TileColor.Blue };
        for (int i = 0; i < 3; i++)
        {
            int randI = Random.Range(0, n);
            int randJ = Random.Range(0, m);
            board[randI, randJ] = flagColor[i];
            DrawTile(randI, randJ);

            board[randI, randJ].AddColor(TileColor.Change);

            Vector2 pos = new Vector2(randI, randJ) - new Vector2((n - 1) / 2f, (m - 1) / 2f);
            Instantiate(changeFlag, pos, Quaternion.identity, changeFlagParent);
        }
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
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                if (board[i, j] != answer[i, j])
                    return false;
        return true;
    }

    /// <summary>
    /// (i,j) 타일에 color 색을 추가 시도.
    /// 이미 해당 색이 있거나 보드 범위 밖이면 false, 성공적으로 색이 추가되면 true 반환.
    /// </summary>
    public bool ColorTile(int i, int j, TileColor color)
    {
        if (i < 0 || i >= n || j < 0 || j >= m) return false;               // 범위 체크
        if ((board[i, j] & (TileColor.Change | color)) != 0) return false;  // 이미 해당 색이 있거나 색 바꾸는 타일임

        board[i, j].AddColor(color);  // 색 추가
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

    /// <summary>
    /// 타일 상태에 따라 타일을 그림
    /// </summary>
    private void DrawBackgroundTile(int i, int j)
    {
        switch (answer[i, j])
        {
            case TileColor.None:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = gray;
                break;
            case TileColor.Red:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = redBack;
                break;
            case TileColor.Green:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = greenBack;
                break;
            case TileColor.Blue:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = blueBack;
                break;
            case TileColor.Red | TileColor.Green:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = redGreenBack;
                break;
            case TileColor.Red | TileColor.Blue:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = redBlueBack;
                break;
            case TileColor.Green | TileColor.Blue:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = greenBlueBack;
                break;
            case TileColor.Red | TileColor.Green | TileColor.Blue:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = redGreenBlueBack;
                break;
        }
    }

    /// <summary>
    /// 타일 상태에 따라 타일을 그림 (RGB 순서)
    /// </summary>
    public void DrawTile(int i, int j)
    {
        switch (board[i, j])
        {
            case TileColor.None:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = gray;
                break;
            case TileColor.Red:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = red;
                break;
            case TileColor.Green:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = green;
                break;
            case TileColor.Blue:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = blue;
                break;
            case TileColor.Red | TileColor.Green:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = redGreen;
                break;
            case TileColor.Red | TileColor.Blue:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = redBlue;
                break;
            case TileColor.Green | TileColor.Blue:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = greenBlue;
                break;
            case TileColor.Red | TileColor.Green | TileColor.Blue:
                tileObjs[i, j].GetComponent<SpriteRenderer>().color = redGreenBlue;
                break;
        }
    }

}
