using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileOutlineDrawer : MonoBehaviour
{
    [SerializeField] private GameObject outlinePrefab;
    [SerializeField] private GameObject shadowLeft, shadowLeftCorner, shadowBottom;
    [SerializeField] private Sprite shadowBottomLeftVoid, shadowBottomBothVoid;
    private Dictionary<Vector2Int, TileData>.KeyCollection board;
    private Quaternion leftRot;
    private int n, m;

    public void InitOutlineAndShadow(int n, int m, Dictionary<Vector2Int, TileData>.KeyCollection board)
    {
        this.board = board;
        this.n = n; this.m = m;
        leftRot = Quaternion.Euler(0, 0, 90);

        for (int i = 0; i < n; i++)
            for (int j = 0; j < m + 1; j++)
                if (ExistsTile(i, j - 1) || ExistsTile(i, j))
                    Instantiate(outlinePrefab, Board.Instance.GetTilePos(i,j), leftRot, transform);

        for (int i = 0; i < n + 1; i++)
            for (int j = 0; j < m; j++)
                if (ExistsTile(i - 1, j) || ExistsTile(i, j))
                    Instantiate(outlinePrefab, Board.Instance.GetTilePos(i, j), Quaternion.identity, transform);

        InitShadow();
        InitShadow();
    }

    private bool ExistsTile(int i, int j)
    {
        if (i < 0 || i >= n || j < 0 || j >= m)
            return false;
        return board.Contains(new Vector2Int(i, j));
    }

    private void InitShadow()
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (!ExistsTile(i, j)) continue;
                // check empty
                bool left = !ExistsTile(i - 1, j);
                bool down = !ExistsTile(i, j - 1);
                bool right = !ExistsTile(i + 1, j);
                bool leftDown = left && down && !ExistsTile(i - 1, j - 1);
                bool rightDown = right && down && !ExistsTile(i + 1, j - 1);

                // side
                if (left) Instantiate(shadowLeft, Board.Instance.GetTilePos(i, j), Quaternion.identity, transform);
                if (right)
                {
                    GameObject obj = Instantiate(shadowLeft, Board.Instance.GetTilePos(i, j), Quaternion.identity, transform);
                    obj.transform.localScale = new Vector3(-1, 1, 1);
                }

                // bottom
                if (down)
                {
                    GameObject obj = Instantiate(shadowBottom, Board.Instance.GetTilePos(i, j), Quaternion.identity, transform);
                    SpriteRenderer spr = obj.GetComponent<SpriteRenderer>();
                    if (leftDown && rightDown) spr.sprite = shadowBottomBothVoid;
                    else if (leftDown) spr.sprite = shadowBottomLeftVoid;
                    else if (rightDown)
                    {
                        spr.sprite = shadowBottomLeftVoid;
                        obj.transform.localScale = new Vector3(-1, 1, 1);
                    }
                }

                // corner
                if (leftDown)
                    Instantiate(shadowLeftCorner, Board.Instance.GetTilePos(i, j), Quaternion.identity, transform);
                if (rightDown)
                {
                    GameObject obj = Instantiate(shadowLeftCorner, Board.Instance.GetTilePos(i, j), Quaternion.identity, transform);
                    obj.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }
    }
}
