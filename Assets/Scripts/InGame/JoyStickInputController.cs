using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoyStickInputController : MonoBehaviour
{
    private struct MouseRecord
    {
        public float time;
        public Vector2 pos;
    }

    [Header("UI")]
    [SerializeField] private RectTransform canvasRect; // 변환할 Canvas의 RectTransform
    [SerializeField] private RectTransform dotTr;
    [SerializeField] private Sprite dot, chev, doubleChev;
    private Image dotImage;

    [Header("Input")]
    [SerializeField] private float thresholdDistSqr = 1000;
    [SerializeField] private float firstRepeatDelayMin = 0.2f, firstRepeatDelayMax = 0.4f;
    [SerializeField] private float holdRepeatIntervalMin = 0.1f, holdRepeatIntervalMax = 0.3f;
    [SerializeField] private float queueRecordDuration = 0.1f; // 최대 기록 시간 (초)
    private float firstRepeatDelay, holdRepeatInterval, lastInputTime;
    private bool isFirstInput;
    private bool clickedOnUI;
    private Vector2Int direction;
    private Queue<MouseRecord> records = new Queue<MouseRecord>();

    private void Awake()
    {
        dotImage = dotTr.GetComponent<Image>();
        direction = Vector2Int.zero;
    }

    private void Start()
    {
        SetDelay();
    }

    private void OnDisable()
    {
        if (dotTr != null)
            dotTr.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
            clickedOnUI = false;

        if (!Input.GetMouseButton(0) || clickedOnUI)
        {
            if (dotTr.gameObject.activeSelf)
                dotTr.gameObject.SetActive(false);
            direction = Vector2Int.zero;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (CustomTools.IsPointerOverUIElement())
            {
                clickedOnUI = true;
                return;
            }
            clickedOnUI = false;
            dotTr.gameObject.SetActive(true);
            dotTr.sizeDelta = new Vector2(45, 45);
            dotImage.sprite = dot;
            lastInputTime = 0;
            isFirstInput = true;
            records.Clear();
        }

        Vector2 mousePos = Input.mousePosition;
        records.Enqueue(new MouseRecord { time = Time.time, pos = mousePos });

        Vector2 mousePosAgo;
        if (!GetMousePosAgo(out mousePosAgo)) return;

        Vector2Int inputDir = GetDirection(mousePos - mousePosAgo);
        // 첫 방향 전환 or 다음 방향 전환
        if ((mousePosAgo - mousePos).sqrMagnitude >= thresholdDistSqr && direction != inputDir)
        {
            dotTr.sizeDelta = new Vector2(120, 120);
            isFirstInput = direction == Vector2Int.zero;
            direction = inputDir;
            if (direction == Vector2Int.zero || Time.time - lastInputTime > holdRepeatInterval)
            {
                lastInputTime = Time.time;
                PlayerController.Instance.MoveOnce(direction);
            }
            ChangeDotSprite();
        }
        else if (direction != Vector2Int.zero) // 꾹 누르는 중
        {
            if (isFirstInput && Time.time - lastInputTime > firstRepeatDelay ||
                !isFirstInput && Time.time - lastInputTime > holdRepeatInterval)
            {
                isFirstInput = false;
                lastInputTime = Time.time;
                PlayerController.Instance.MoveOnce(direction);
                ChangeDotSprite();
            }
        }
        SetDotPosition();
    }

    private bool GetMousePosAgo(out Vector2 mousePos)
    {
        MouseRecord older = new MouseRecord();
        MouseRecord newer;
        older.time = -1;

        // queueRecordDuration 전의 입력 중 가장 최신의 입력을 받아옴
        while (records.Count > 0 && records.Peek().time < Time.time - queueRecordDuration)
        {
            older = records.Peek();
            records.Dequeue();
        }
        if (records.Count == 0)
        {
            mousePos = Vector2.zero;
            return false; // queueRecordDuration 이후에 입력된 것이 없으면 종료
        }

        newer = records.Peek(); // queueRecordDuration 이후의 입력 중 가장 과거의 입력
        if (older.time < 0)
        {
            // queueRecordDuration 이후의 입력만 있는 경우
            mousePos = newer.pos;
        }
        else
        {
            // queueRecordDuration 전의 입력도 있는 경우 보간함
            float t = Mathf.InverseLerp(older.time, newer.time, queueRecordDuration);
            mousePos = Vector2.Lerp(older.pos, newer.pos, t);
        }
        return true;
    }

    private Vector2Int GetDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x < 0) return Vector2Int.left;
            else return Vector2Int.right;
        }
        else
        {
            if (dir.y < 0) return Vector2Int.down;
            else return Vector2Int.up;
        }
    }

    private void ChangeDotSprite()
    {
        dotImage.sprite = isFirstInput ? chev : doubleChev;
        if (direction == Vector2Int.up) dotTr.rotation = Quaternion.Euler(0, 0, -90);
        else if (direction == Vector2Int.right) dotTr.rotation = Quaternion.Euler(0, 0, 180);
        else if (direction == Vector2Int.down) dotTr.rotation = Quaternion.Euler(0, 0, 90);
        else dotTr.rotation = Quaternion.identity;
    }

    private void SetDotPosition()
    {
        Vector2 canvasPosition;

        // 스크린 좌표 -> 캔버스 좌표
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            null, // Overlay 모드에서는 카메라 필요 없음
            out canvasPosition
        );

        Vector2 dotPosition = canvasPosition + Vector2.up * 150;
        if (dotPosition.y > canvasRect.rect.height / 2f)
            dotPosition.y -= 300f;
        dotTr.anchoredPosition = dotPosition;
    }

    public void SetDelay()
    {
        float t = PersistentDataManager.Instance.moveLatencyRate;
        if (t < 0.7f) t = t / 0.7f * 0.5f;
        else t = 0.5f + (t - 0.7f) / 0.3f * 0.5f;
        firstRepeatDelay = Mathf.Lerp(firstRepeatDelayMin, firstRepeatDelayMax, t);
        holdRepeatInterval = Mathf.Lerp(holdRepeatIntervalMin, holdRepeatIntervalMax, t);
    }
}