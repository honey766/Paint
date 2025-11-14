using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SwipeInputController : MonoBehaviour, IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler
{
    [SerializeField] private UnityEvent leftMove, rightMove;
    [SerializeField] private float slideThresholdX = 100f;
    private struct MouseRecord
    {
        public float time;
        public Vector2 pos;
    }
    private Queue<MouseRecord> records = new Queue<MouseRecord>();
    private int isSlided; // 0 : no, 1 : 왼쪽으로 했음, 2 : 오른쪽으로 했음
    bool isPointerDownInvoked;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDownInvoked = true;
        isSlided = 0;
        records.Enqueue(new MouseRecord { time = Time.time, pos = eventData.position });
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isPointerDownInvoked || !Input.GetMouseButton(0))
            return; // 마우스가 눌려 있지 않으면 리턴

        Logger.Log($"push {eventData.position.x}");
        records.Enqueue(new MouseRecord { time = Time.time, pos = eventData.position });
        Vector2 mousePosAgo;
        if (!GetMousePosAgo(out mousePosAgo)) return;

        if (Mathf.Abs(mousePosAgo.x - eventData.position.x) > slideThresholdX)
        {
            // 오른쪽으로 슬라이드
            if (mousePosAgo.x < eventData.position.x && isSlided != 2)
            {
                isSlided = 2;
                leftMove?.Invoke();
            }
            // 왼쪽으로 슬라이드
            else if (mousePosAgo.x > eventData.position.x && isSlided != 1)
            {
                isSlided = 1;
                rightMove?.Invoke();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDownInvoked = false;
        records.Clear();
    }

    private bool GetMousePosAgo(out Vector2 mousePos)
    {
        MouseRecord older = new MouseRecord();
        MouseRecord newer;
        older.time = -1;

        // 0.2초 전의 입력 중 가장 최신의 입력을 받아옴
        while (records.Count > 0 && records.Peek().time < Time.time - 0.2f)
        {
            older = records.Peek();
            records.Dequeue();
        }
        if (records.Count == 0)
        {
            mousePos = Vector2.zero;
            return false; // 0.2초 이내에 입력된 것이 없으면 종료
        }

        newer = records.Peek(); // 0.2초 이내의 입력 중 가장 과거의 입력
        if (older.time < 0)
        {
            // 0.2초 이내의 입력만 있는 경우
            mousePos = newer.pos;
        }
        else
        {
            // 0.2초 전의 입력도 있는 경우 보간함
            float t = Mathf.InverseLerp(older.time, newer.time, 0.2f);
            mousePos = Vector2.Lerp(older.pos, newer.pos, t);
        }
        return true;
    }
}
