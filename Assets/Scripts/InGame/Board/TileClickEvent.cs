using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class TileClickEvent : MonoBehaviour
{
    private GameObject lastTile = null;
    private int layerMask;

    private void Awake()
    {
        layerMask = 1 << LayerMask.NameToLayer("Tile");
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0, layerMask); // 마우스 위치에서 2D 레이캐스트

            // 마우스가 타일 위에 있으면서 이전의 타일과 다른 타일일 때
            if (hit.collider != null && lastTile != hit.collider.gameObject)
            {
                lastTile = hit.collider.gameObject;
                Logger.Log("Click" + hit.collider.gameObject.name);

                // 타일의 좌표 알아내기
                string name = lastTile.name; // "Tile[2,3]"
                string inside = name.Substring(5, name.Length - 6); // "2,3"
                string[] parts = inside.Split(',');
                int i = int.Parse(parts[0]);
                int j = int.Parse(parts[1]);

                // 해당 좌표로 플레이어 이동 시도
                PlayerController.Instance.TryMoveTo(i, j);
            }
        }
    }
}
