using UnityEngine;
using System.Text.RegularExpressions;

public class TileClickEvent : MonoBehaviour
{
    public GameObject lastTile = null;
    private int layerMask;
    private bool isMouseDown;

    private void Awake()
    {
        layerMask = 1 << LayerMask.NameToLayer("Tile");
        Init();
    }

    public void Init()
    {
        lastTile = null;
        isMouseDown = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !CustomTools.IsPointerOverUIElement())
            isMouseDown = true;
        if (Input.GetMouseButtonUp(0))
            isMouseDown = false;

        if (isMouseDown)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0, layerMask); // 마우스 위치에서 2D 레이캐스트

            // 마우스가 타일 위에 있으면서 이전의 타일과 다른 타일일 때
            if (hit.collider != null && lastTile != hit.collider.gameObject)
            {
                lastTile = hit.collider.gameObject;
                Logger.Log("Click" + hit.collider.gameObject.name);

                // 타일의 좌표 알아내기
                string name = lastTile.name; // "Color2Paint(5,5)"
                Match match = Regex.Match(name, @"\((\d+),(\d+)\)");
                if (match.Success)
                {
                    int i = int.Parse(match.Groups[1].Value);
                    int j = int.Parse(match.Groups[2].Value);

                    // 해당 좌표로 플레이어 이동 시도
                    PlayerController.Instance.TryMoveTo(i, j);
                }
            }
        }
    }
}
