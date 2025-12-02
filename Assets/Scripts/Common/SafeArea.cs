#if !UNITY_WEBGL || UNITY_EDITOR
using UnityEngine;
using TMPro;

// https://dev-junwoo.tistory.com/124 [개발초보 JW의 성장일기:티스토리]
public class SafeArea : MonoBehaviour
{
    Vector2 minAnchor;
    Vector2 maxAnchor;

    private void Start()
    {
        var Myrect = GetComponent<RectTransform>();

        minAnchor = Screen.safeArea.min;
        maxAnchor = Screen.safeArea.max;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;

        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        Myrect.anchorMin = minAnchor;
        Myrect.anchorMax = maxAnchor;
        Logger.Log($"{Myrect.anchorMin},A, {Myrect.anchorMax}");
    }
}
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SafeAreaPayload
{
    public float top;
    public float bottom;
    public float left;
    public float right;
    public float windowWidth;  // JS에서 보낸 이름과 똑같아야 함
    public float windowHeight; // JS에서 보낸 이름과 똑같아야 함
}

public class SafeArea : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern string GetSafeAreaInsets();
    
    [DllImport("__Internal")]
    private static extern void SubscribeSafeArea(string gameObjectName);
    
    private RectTransform target;
    
    void Start()
    {
        target = GetComponent<RectTransform>();
        
        ApplySafeArea();
        SubscribeSafeArea(gameObject.name);
    }
    
    void ApplySafeArea()
    {
        string json = GetSafeAreaInsets();
        SafeAreaPayload d = JsonUtility.FromJson<SafeAreaPayload>(json);
        
        // RectTransform에 Safe Area 적용
        Apply(d);
    }

    private void Apply(SafeAreaPayload d) 
    {
        // 방어 코드: 너비가 0이면 나눗셈 에러 나므로 리턴
        if (target == null || d.windowWidth <= 0 || d.windowHeight <= 0) return;

        // CSS 픽셀끼리 나누므로 정확한 비율(0.0 ~ 1.0)이 나옴
        float axMin = d.left / d.windowWidth;
        float ayMin = d.bottom / d.windowHeight;
        float axMax = 1f - (d.right / d.windowWidth);
        float ayMax = 1f - (d.top / d.windowHeight);

        target.anchorMin = new Vector2(axMin, ayMin);
        target.anchorMax = new Vector2(axMax, ayMax);

        // 오프셋/포지션/스케일은 초기화
        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
        target.anchoredPosition = Vector2.zero;
        target.localScale = Vector3.one;
    }
    
    // JavaScript에서 호출
    public void OnSafeAreaChanged(string json)
    {
        SafeAreaPayload d = JsonUtility.FromJson<SafeAreaPayload>(json);
        Apply(d);
    }
}
#endif