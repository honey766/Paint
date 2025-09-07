using UnityEngine;

public class CameraSizeController : MonoBehaviour
{
    [Tooltip("원하는 여백(padding) 값")]
    public float padding = 1f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("카메라 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        if (cam.orthographic == false)
        {
            Debug.LogError("이 스크립트는 Orthographic 카메라에서만 작동합니다.");
            return;
        }
    }

    public void AdjustCameraSize(BoardSO boardSO)
    {
        // 1. 두 좌표 사이의 거리 계산
        int n = boardSO.n;
        int m = boardSO.m;

        // 2. 화면 비율(Aspect Ratio) 계산
        float screenAspect = (float)Screen.width / Screen.height;

        // 3. 카메라 size(세로 길이의 절반) 계산
        float requiredSizeY = (m / 2f) + padding;
        float requiredSizeX = (n / 2f) / screenAspect + padding;

        // 4. 가로, 세로 중 더 큰 값을 기준으로 최종 size 결정
        // (화면 비율을 고려하여 가로 길이도 세로 size로 변환)
        cam.orthographicSize = Mathf.Max(requiredSizeY, requiredSizeX);
    }
}
