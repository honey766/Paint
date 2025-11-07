using UnityEngine;
using UnityEngine.UI;

public class CanvasScaleModifier : MonoBehaviour
{
    private void Awake()
    {
        float ratio = (float)Screen.height / Screen.width;
        // float match = Mathf.InverseLerp(2.5f, 1.5f, ratio);
        GetComponent<CanvasScaler>().matchWidthOrHeight = ratio > 16f/9f ? 0 : 1;
        // Logger.Log($"{(float)Screen.height / Screen.width}\n{Screen.width},{Screen.height}");
        // Logger.Log($"ratio : {ratio}, match : {match}");
    }
}
