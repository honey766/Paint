using UnityEngine;

public class EditorCameraMove : MonoBehaviour
{
    public Vector2 initPos = Vector2.zero;

    [Header("이동속도")]
    public float curSpeed = 0f;
    float holdTime = 0f;
    float prevHoldTime = 5f;
    float releaseTime = 0f;
    public float minSpeed = 5f;
    public float maxSpeed = 15f;
    public float timeToMaxSpeed = 3f;

    [Header("마우스 휠 조작")]
    public float defaultScale = 15f;
    public float minScale = 6f;
    public float maxScale = 30f;
    public float zoomSpeed = 10f;

    [Header("마우스 드래그")]
    public float dragSpeed = 2f;
    
    private Camera cam;
    private Vector3 lastMousePosition;
    private bool isDraggingWheel, isDraggingRight;


    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = defaultScale;
        isDraggingWheel = isDraggingRight = false;
        transform.position = new Vector3(initPos.x, initPos.y, -10);
    }


    void Update()
    {
        HandleKeyboardMovement();
        HandleMouseDrag();
        HandleMouseScroll();
    }

    private void HandleKeyboardMovement()
    {
        if (holdTime < timeToMaxSpeed) curSpeed = minSpeed + (maxSpeed - minSpeed) / timeToMaxSpeed * holdTime;
        else curSpeed = maxSpeed;
        curSpeed *= cam.orthographicSize / 10f;

        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        transform.position += new Vector3(hor, ver, 0f) * curSpeed * Time.deltaTime;

        if (hor != 0f || ver != 0f) {
            if (holdTime == 0 && prevHoldTime < 0.5f && releaseTime < 0.1f) // 더블클릭
                holdTime = 5f;
            holdTime += Time.deltaTime;
            releaseTime = 0;
        }
        else {
            if (holdTime > 0f)
                prevHoldTime = holdTime;
            holdTime = 0f;
            releaseTime += Time.deltaTime;
        }
    }

    private void HandleMouseDrag()
    {
        if (!IsMouseOverGameWindow()) return;

        // 우클릭 시작
        if (!isDraggingRight && !isDraggingWheel)
        {
            if (Input.GetMouseButtonDown(1))
            {
                isDraggingRight = true;
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonDown(2))
            {
                isDraggingWheel = true;
                lastMousePosition = Input.mousePosition;
            }
        }

        // 우클릭 드래그 중
        if (isDraggingRight || isDraggingWheel)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 mouseDelta = lastMousePosition - currentMousePosition;

            // 마우스 좌표를 월드 좌표로 변환 (orthographic 카메라 기준)
            Vector3 worldDelta = new Vector3(
                mouseDelta.x * (cam.orthographicSize * 2f / Screen.height),
                mouseDelta.y * (cam.orthographicSize * 2f / Screen.height),
                0f
            );

            transform.position += worldDelta * dragSpeed;
            lastMousePosition = currentMousePosition;
        }

        // 클릭 종료
        if (Input.GetMouseButtonUp(1))
            isDraggingRight = false;
        if (Input.GetMouseButtonUp(2))
            isDraggingWheel = false;
    }

    private void HandleMouseScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (IsMouseOverGameWindow() && scroll != 0) {
            float speedModifier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 3.0f : 1.0f;
            cam.orthographicSize -= scroll * zoomSpeed * speedModifier;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minScale, maxScale);
        }
    }


    // 마우스가 게임 화면 내부에 있는지 확인
    private bool IsMouseOverGameWindow()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height;
    }
}