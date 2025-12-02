/*
using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public class SafeAreaInsets
{
    public float top;
    public float bottom;
    public float left;
    public float right;
}

public class SafeAreaApplier : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern string GetSafeAreaInsets();
    
    [DllImport("__Internal")]
    private static extern void SubscribeSafeArea(string gameObjectName);
    
    private RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
#if UNITY_WEBGL && !UNITY_EDITOR
        ApplySafeArea();
        SubscribeSafeArea(gameObject.name);
#endif
    }
    
    void ApplySafeArea()
    {
        string json = GetSafeAreaInsets();
        SafeAreaInsets insets = JsonUtility.FromJson<SafeAreaInsets>(json);
        
        // RectTransform에 Safe Area 적용
        rectTransform.offsetMin = new Vector2(insets.left, insets.bottom);
        rectTransform.offsetMax = new Vector2(-insets.right, -insets.top);
    }
    
    // JavaScript에서 호출
    public void OnSafeAreaChanged(string json)
    {
        SafeAreaInsets insets = JsonUtility.FromJson<SafeAreaInsets>(json);
        rectTransform.offsetMin = new Vector2(insets.left, insets.bottom);
        rectTransform.offsetMax = new Vector2(-insets.right, -insets.top);
    }
}
*/