using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class EditorInputController : MonoBehaviour
{
    [SerializeField] private EditorDataManager editorData;
    [SerializeField] private TextMeshProUGUI curAxisText;
    private Vector2Int prevInputPos;
    private int prevToggleNum;

    private void Start()
    {
        prevInputPos = new Vector2Int(-1000, -1000);
        prevToggleNum = -1;
    }

    private void Update()
    {
        // ✅ UI 위에서 마우스 클릭 감지 시 리턴
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int curPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));
        curAxisText.text = $"({curPos.x}, {curPos.y})";

        if (Input.GetMouseButton(0))
        {
            if (!(curPos == prevInputPos && ToggleManager.Instance.toggleNum == prevToggleNum))
            {
                prevInputPos = curPos;
                prevToggleNum = ToggleManager.Instance.toggleNum;
                editorData.Input(curPos);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            Start();
        }
    }
}
