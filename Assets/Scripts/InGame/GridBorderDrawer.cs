using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridBorderDrawer : MonoBehaviour
{
    private Vector2Int gridSize = new Vector2Int(10, 10);
    public float lineWidth = 0.1f; // 선의 두께를 정하는 변수
    public float eachBorderOffset;

    private TileType myColor;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;

    private bool[,] isMyColor;
    private TileType[,] answer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    public void InitBorder(Color borderColor, TileType myColor, int n, int m, Dictionary<Vector2Int, TileType> target)
    {
        answer = new TileType[n, m];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                if (target.TryGetValue(new Vector2Int(i, j), out TileType type))
                    answer[i, j] = type;
                else
                    answer[i, j] = TileType.None;
                
        this.myColor = myColor;
        meshRenderer.material.color = borderColor;
        gridSize = new Vector2Int(n, m);
        isMyColor = new bool[gridSize.x, gridSize.y];
        transform.position = -new Vector2((gridSize.x - 1) / 2f, (gridSize.y - 1) / 2f);

        for (int i = 0; i < gridSize.x; i++)
            for (int j = 0; j < gridSize.y; j++)
                isMyColor[i, j] = answer[i, j] == myColor;

        DrawBorders();
    }

    void DrawBorders()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        Vector3 topOffset = Vector3.down * lineWidth / 2f;
        Vector3 bottomOffset = Vector3.up * lineWidth / 2f;
        Vector3 leftOffset = Vector3.right * lineWidth / 2f;
        Vector3 rightOffset = Vector3.left * lineWidth / 2f;

        // 1. 각 타일의 변을 기준으로 선분을 생성
        foreach (Vector2Int pos in Board.Instance.target.Keys)
        {
            int i = pos.x, j = pos.y;
            if (isMyColor[i, j])
            {
                Vector3 cornerBL = new Vector3(i - 0.5f, j - 0.5f, 0);
                Vector3 cornerBR = new Vector3(i + 0.5f, j - 0.5f, 0);
                Vector3 cornerTL = new Vector3(i - 0.5f, j + 0.5f, 0);
                Vector3 cornerTR = new Vector3(i + 0.5f, j + 0.5f, 0);

                if (j + 1 >= gridSize.y || !isMyColor[i, j + 1])
                {
                    //if (!Board.Instance.tileSet.Contains(new Vector2Int(i, j + 1)))
                    if (IsNoColor(i, j + 1))
                        AddLineQuad(vertices, triangles, cornerTL, cornerTR, lineWidth + eachBorderOffset, topOffset + Vector3.up * eachBorderOffset / 2f);
                    else
                        AddLineQuad(vertices, triangles, cornerTL, cornerTR, lineWidth, topOffset);
                }
                if (j - 1 < 0 || !isMyColor[i, j - 1])
                {
                    if (IsNoColor(i, j - 1))
                        AddLineQuad(vertices, triangles, cornerBL, cornerBR, lineWidth + eachBorderOffset, bottomOffset + Vector3.down * eachBorderOffset / 2f);
                    else
                        AddLineQuad(vertices, triangles, cornerBL, cornerBR, lineWidth, bottomOffset);
                }
                if (i - 1 < 0 || !isMyColor[i - 1, j])
                {
                    if (IsNoColor(i - 1, j))
                        AddLineQuad(vertices, triangles, cornerBL, cornerTL, lineWidth + eachBorderOffset, leftOffset + Vector3.left * eachBorderOffset / 2f);
                    else
                        AddLineQuad(vertices, triangles, cornerBL, cornerTL, lineWidth, leftOffset);
                }
                if (i + 1 >= gridSize.x || !isMyColor[i + 1, j])
                {
                    if (IsNoColor(i + 1, j))
                        AddLineQuad(vertices, triangles, cornerBR, cornerTR, lineWidth + eachBorderOffset, rightOffset + Vector3.right * eachBorderOffset / 2f);
                    else
                        AddLineQuad(vertices, triangles, cornerBR, cornerTR, lineWidth, rightOffset);
                }
            }
        }

        // 2. 모서리 연결부(오목한 부분)를 추가
        for (int i = 1; i < gridSize.x; i++)
        {
            for (int j = 1; j < gridSize.y; j++)
            {
                bool hasTopLeft = isMyColor[i - 1, j];
                bool hasTopRight = isMyColor[i, j];
                bool hasBottomLeft = isMyColor[i - 1, j - 1];
                bool hasBottomRight = isMyColor[i, j - 1];

                // 세 개의 칸이 빨간색일 때, 끊어진 모서리를 채운다.
                // 윗 오른쪽 모서리
                if (hasTopLeft && hasTopRight && hasBottomRight && !hasBottomLeft)
                {
                    AddCornerQuad(vertices, triangles, new Vector3(i - 0.5f, j - 0.5f, 0), 0);
                }
                // 윗 왼쪽 모서리
                else if (hasTopRight && hasTopLeft && hasBottomLeft && !hasBottomRight)
                {
                    AddCornerQuad(vertices, triangles, new Vector3(i - 0.5f, j - 0.5f, 0), 90);
                }
                // 아래 왼쪽 모서리
                else if (hasBottomRight && hasBottomLeft && hasTopLeft && !hasTopRight)
                {
                    AddCornerQuad(vertices, triangles, new Vector3(i - 0.5f, j - 0.5f, 0), 180);
                }
                // 아래 오른쪽 모서리
                else if (hasBottomLeft && hasBottomRight && hasTopRight && !hasTopLeft)
                {
                    AddCornerQuad(vertices, triangles, new Vector3(i - 0.5f, j - 0.5f, 0), 270);
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
    }

    private void AddLineQuad(List<Vector3> vertices, List<int> triangles, Vector3 start, Vector3 end, float width, Vector3 offset)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0) * (width / 2f);

        int vertIndex = vertices.Count;

        // 사각형의 네 꼭짓점
        vertices.Add(start - perpendicular + offset);
        vertices.Add(start + perpendicular + offset);
        vertices.Add(end + perpendicular + offset);
        vertices.Add(end - perpendicular + offset);

        // 첫 번째 삼각형
        triangles.Add(vertIndex + 0);
        triangles.Add(vertIndex + 1);
        triangles.Add(vertIndex + 2);

        // 두 번째 삼각형
        triangles.Add(vertIndex + 0);
        triangles.Add(vertIndex + 2);
        triangles.Add(vertIndex + 3);
    }

    // 오목한 모서리를 채우는 함수
    private void AddCornerQuad(List<Vector3> vertices, List<int> triangles, Vector3 center, float rotationDegrees)
    {
        int vertIndex = vertices.Count;

        // 회전을 위해 쿼터니언을 사용
        Quaternion rotation = Quaternion.Euler(0, 0, rotationDegrees);

        // 네 꼭짓점
        vertices.Add(center + rotation * new Vector3(lineWidth, 0, 0)); // Bottom-Right
        vertices.Add(center + rotation * new Vector3(0, 0, 0)); // Bottom-Left
        vertices.Add(center + rotation * new Vector3(0, lineWidth, 0)); // Top-Left
        vertices.Add(center + rotation * new Vector3(lineWidth, lineWidth, 0));  // Top-Right

        // 두 삼각형
        triangles.Add(vertIndex + 0);
        triangles.Add(vertIndex + 1);
        triangles.Add(vertIndex + 2);

        triangles.Add(vertIndex + 0);
        triangles.Add(vertIndex + 2);
        triangles.Add(vertIndex + 3);
    }

    private bool IsNoColor(int i, int j)
    {
        if (0 <= i && i < gridSize.x && 0 <= j && j < gridSize.y)
            return answer[i, j] == TileType.None;
        return true;
    }
}