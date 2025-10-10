using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileOutlineDrawer : MonoBehaviour
{
    [SerializeField] private GameObject outlinePrefab;
    private Dictionary<Vector2Int, TileData>.KeyCollection board;
    private Quaternion leftRot;
    private int n, m;

    public void InitOutline(int n, int m, Dictionary<Vector2Int, TileData>.KeyCollection board)
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
                    Instantiate(outlinePrefab, Board.Instance.GetTilePos(i,j), Quaternion.identity, transform);
    }

    private bool ExistsTile(int i, int j)
    {
        if (i < 0 || i >= n || j < 0 || j >= m)
            return false;
        return board.Contains(new Vector2Int(i, j));
    }
}
