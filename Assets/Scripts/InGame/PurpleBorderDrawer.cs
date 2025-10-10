using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PurpleBorderDrawer : MonoBehaviour
{
    [SerializeField] private GameObject borderPrefab;
    [SerializeField] private GameObject borderCornerPrefab;
    [SerializeField] private Sprite purple1, purple1Both;
    [SerializeField] private Sprite purple2;
    [SerializeField] private Sprite purple3;
    [SerializeField] private Sprite purple4;

    //private Vector2Int gridSize;
    private bool[,] isMyColor;
    private TileType[,] answer;
    private int n, m;
    private bool up, right, down, left;
    private SpriteRenderer curSpriter;
    private Transform curBorderTr;
    private Quaternion upRot, rightRot, downRot, leftRot;
    private float alpha;
    Vector2Int[] dir;
    Quaternion[] rot;

    public void InitBorder(int n, int m, Dictionary<Vector2Int, TileType> target, float alpha)
    {
        this.n = n; this.m = m;
        answer = new TileType[n, m];
        isMyColor = new bool[n, m];
        this.alpha = alpha;

        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                if (target.TryGetValue(new Vector2Int(i, j), out TileType type))
                    answer[i, j] = type;
                else
                    answer[i, j] = TileType.None;

        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                isMyColor[i, j] = answer[i, j] == TileType.Color12;

        upRot = Quaternion.identity;
        rightRot = Quaternion.Euler(0, 0, -90);
        downRot = Quaternion.Euler(0, 0, 180);
        leftRot = Quaternion.Euler(0, 0, 90);
        dir = new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };
        rot = new Quaternion[] { upRot, rightRot, downRot, leftRot };

        AGG();
    }

    private void AGG()
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (!isMyColor[i, j])
                    continue;

                up = ExistsAnswer(i, j, Vector2Int.up);
                right = ExistsAnswer(i, j, Vector2Int.right);
                down = ExistsAnswer(i, j, Vector2Int.down);
                left = ExistsAnswer(i, j, Vector2Int.left);
                int existsCount = new[] { up, right, down, left }.Count(b => b);

                GameObject border = Instantiate(borderPrefab, Board.Instance.GetTilePos(i, j), Quaternion.identity, transform);
                curSpriter = border.GetComponent<SpriteRenderer>();
                curBorderTr = border.transform;
                curSpriter.color = new Color(1, 1, 1, alpha);

                if (existsCount == 1)
                {
                    DrawWhenCount1();
                }
                else if (existsCount == 2)
                {
                    DrawWhenCount2(i, j);
                }
                else if (existsCount == 3)
                {
                    DrawWhenCount3(i, j);
                }
                else if (existsCount == 4)
                {
                    DrawWhenCount4(i, j);
                }
            }
        }
    }

    private bool ExistsAnswer(int i, int j, Vector2Int direction)
    {
        i += direction.x;
        j += direction.y;
        if (i < 0 || i >= n || j < 0 || j >= m)
            return false;
        return isMyColor[i, j];
    }

    private void DrawWhenCount1()
    {
        curSpriter.sprite = purple3;
        DoRotate(up, right, down);
    }

    private void DrawWhenCount2(int i, int j)
    {
        // ㅡ 모양
        if (up && down || left && right)
        {
            curSpriter.sprite = purple1Both;
            DoRotate(up, right, false);
        }
        // ㄱ 모양
        else
        {
            Vector2Int upVec = up ? Vector2Int.up : Vector2Int.zero;
            Vector2Int rightVec = right ? Vector2Int.right : Vector2Int.zero;
            Vector2Int downVec = down ? Vector2Int.down : Vector2Int.zero;
            Vector2Int leftVec = left ? Vector2Int.left : Vector2Int.zero;
            Vector2Int diagVec = upVec + rightVec + downVec + leftVec;
            if (!ExistsAnswer(i, j, diagVec))
                Instantiate(borderCornerPrefab, curBorderTr);
            curSpriter.sprite = purple2;
            DoRotate(left && up, up && right, right && down);
        }
    }

    private void DrawWhenCount3(int i, int j)
    {
        curSpriter.sprite = purple1;

        int idx = 0;
        if (!left) idx = 1;
        else if (!up) idx = 2;
        else if (!right) idx = 3;

        for (int k = 0; k < 2; k++)
        {
            int curIdx = (idx + k) % 4;
            if (!ExistsAnswer(i, j, dir[curIdx]))
            {
                GameObject cornerObj = Instantiate(borderCornerPrefab, curBorderTr);
                cornerObj.transform.rotation = rot[k];
            }
        }

        DoRotate(!down, !left, !up);
    }

    private void DrawWhenCount4(int i, int j)
    {
        curSpriter.sprite = null;

        for (int k = 0; k < 4; k++)
        {
            if (!ExistsAnswer(i, j, dir[k]))
            {
                GameObject cornerObj = Instantiate(borderCornerPrefab, curBorderTr);
                cornerObj.transform.rotation = rot[k];
            }
        }
    }
    
    private void DoRotate(bool up, bool right, bool down)
    {
        if (up) curBorderTr.rotation = upRot;
        else if (right) curBorderTr.rotation = rightRot;
        else if (down) curBorderTr.rotation = downRot;
        else curBorderTr.rotation = leftRot;
    }
}
