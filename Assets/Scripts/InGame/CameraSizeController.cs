using System.Collections;
using UnityEngine;

public class CameraSizeController : MonoBehaviour
{
    // [Tooltip("원하는 여백(padding) 값")]
    // public float horPadding = 1f;
    // public float verPadding = 1.5f;

    [SerializeField] private RectTransform canvasParent;
    [SerializeField] private RectTransform boardVerArea, boardHorArea;

    private Camera cam;
    private BoardSO boardSO;
    private RectTransform canvasRect;

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

    // public void AdjustCameraSize(BoardSO boardSO, bool isTutorial = false)
    // {
    //     // 1. 두 좌표 사이의 거리 계산
    //     int n = boardSO.n;
    //     int m = boardSO.m;

    //     // 2. 화면 비율(Aspect Ratio) 계산
    //     float screenAspect = (float)Screen.width / Screen.height;

    //     // 3. 카메라 size(세로 길이의 절반) 계산
    //     float requiredSizeY = (m / 2f) + verPadding;
    //     float requiredSizeX = (n / 2f) / screenAspect + horPadding + (isTutorial ? 1.5f : 0);

    //     // 4. 가로, 세로 중 더 큰 값을 기준으로 최종 size 결정
    //     // (화면 비율을 고려하여 가로 길이도 세로 size로 변환)
    //     float cameraSize = Mathf.Max(requiredSizeY, requiredSizeX);
    //     cameraSize = Mathf.Ceil(cameraSize * 2f) / 2f; // 0.5단위
    //     cam.orthographicSize = cameraSize;
    // }

    public void AdjustCameraSize(BoardSO boardSO, bool isTutorial = false)
    {
        this.boardSO = boardSO;
        canvasRect = canvasParent.transform.parent.GetComponent<RectTransform>();
        StartCoroutine(Cema(isTutorial));
    }

    private IEnumerator Cema(bool isTutorial = false)
    {
        yield return null;
        // Logger.Log("====");
        // Logger.Log($"anchorMin:{boardArea.anchorMin},anchorMax:{boardArea.anchorMax}");
        // Logger.Log($"offsetMin:{boardArea.offsetMin},offsetMax:{boardArea.offsetMax}");
        // Logger.Log("====");
        // Logger.Log($"anchorMin:{canvasParent.anchorMin},anchorMax:{canvasParent.anchorMax}");
        // Logger.Log($"offsetMin:{canvasParent.offsetMin},offsetMax:{canvasParent.offsetMax}");
        float canvasAspect = canvasRect.rect.height / canvasRect.rect.width;

        Vector2 centerRateWithVerArea;
        float sizeWithVerArea = AA(boardVerArea, isTutorial, out centerRateWithVerArea);
        Vector2 centerRateWithHorArea;
        float sizeWithHorArea = AA(boardHorArea, isTutorial, out centerRateWithHorArea);

        Vector2 centerRate;
        float cameraSize;
        // 두 영역 중 카메라 사이즈가 더 작은 쪽을 택해야 보드가 더 커 보임
        if (sizeWithVerArea < sizeWithHorArea)
        {
            centerRate = centerRateWithVerArea;
            cameraSize = sizeWithVerArea;
        }
        else
        {
            centerRate = centerRateWithHorArea;
            cameraSize = sizeWithHorArea;
        }
        cam.orthographicSize = cameraSize;

        Vector2 cameraCenter;
        // Mathf.Lerp(centerX - size/canvasAspect, centerX + size/canvasAspect, (minX + maxX)/2) = 0
        // centerX - size/canvasAspect + (size/canvasAspect) * (minX + maxX) = 0
        cameraCenter.x = -cameraSize / canvasAspect * (-1 + centerRate.x * 2);
        cameraCenter.y = -cameraSize * (-1 + centerRate.y * 2);
        cam.transform.position = new Vector3(cameraCenter.x, cameraCenter.y, -10);
    }

    // return cameraSize
    private float AA(RectTransform boardArea, bool isTutorial, out Vector2 centerRate)
    {
        Vector2 canvasParentRate = new Vector2(canvasParent.anchorMax.x - canvasParent.anchorMin.x,
                                               canvasParent.anchorMax.y - canvasParent.anchorMin.y);

        float parentWidth = canvasRect.rect.width * canvasParentRate.x;
        float parentHeight = canvasRect.rect.height * canvasParentRate.y;
        //Logger.Log($"ww:{parentWidth},hh:{parentHeight}");

        Vector2 boardAnchorMin = new Vector2(boardArea.offsetMin.x / parentWidth, boardArea.offsetMin.y / parentHeight);
        Vector2 boardAnchorMax = new Vector2(1 + boardArea.offsetMax.x / parentWidth, 1 + boardArea.offsetMax.y / parentHeight);
        //Logger.Log($"boardMin:{boardAnchorMin},boardMax:{boardAnchorMax}");

        float minXRate = Mathf.Lerp(canvasParent.anchorMin.x, canvasParent.anchorMax.x, boardAnchorMin.x);
        float maxXRate = Mathf.Lerp(canvasParent.anchorMin.x, canvasParent.anchorMax.x, boardAnchorMax.x);
        float minYRate = Mathf.Lerp(canvasParent.anchorMin.y, canvasParent.anchorMax.y, boardAnchorMin.y);
        float maxYRate = Mathf.Lerp(canvasParent.anchorMin.y, canvasParent.anchorMax.y, boardAnchorMax.y);
        //Logger.Log($"X:{minXRate}~{maxXRate}, Y:{minYRate}~{maxYRate}");

        float canvasAspect = canvasRect.rect.height / canvasRect.rect.width;
        float xPixel = (maxXRate - minXRate) * canvasRect.rect.width;
        float yPixel = (maxYRate - minYRate) * canvasRect.rect.height;
        float n = Mathf.Max(6, boardSO.n);
        float m = Mathf.Max(6, boardSO.m + (isTutorial ? 2 : 0));
        float boardAreaAspect = yPixel / xPixel;
        float boardAspect = m / n;
        centerRate = new Vector2((minXRate + maxXRate) / 2f, (minYRate + maxYRate) / 2f);

        float cameraSize;
        //Logger.Log($"aspect board: {boardAspect}, boardArea:{boardAreaAspect}");
        if (boardAspect > boardAreaAspect) // 보드를 그릴 수 있는 영역보다 실제 보드의 세로비율이 더 긺
        {
            // 2 * size * (MaxY-minY) = m
            cameraSize = m / 2f / (maxYRate - minYRate);
            //Logger.Log($"zzz m:{m}, min:{minYRate}, max:{maxYRate}");
        }
        else // 보드의 가로비율이 더 긺
        {
            // 2 * size / canvasAspect * (MaxX-minX) = n
            cameraSize = n / 2f * canvasAspect / (maxXRate - minXRate);
            //Logger.Log($"zzz m:{n}, min:{minXRate}, max:{maxXRate}");
        }
        //Logger.Log($"cam : {cameraSize}");
        cameraSize = Mathf.Ceil(cameraSize * 2f) / 2f; // 0.5단위
        
        return cameraSize;
    }
}
