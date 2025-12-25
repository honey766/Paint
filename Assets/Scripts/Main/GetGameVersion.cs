using UnityEngine;
using TMPro;

public class GetGameVersion : MonoBehaviour
{
    void Start()
    {
        TextMeshProUGUI tmpro = GetComponent<TextMeshProUGUI>();
        RectTransform rect = GetComponent<RectTransform>();
        string gameVersion = Application.version;
        tmpro.text = gameVersion;

        #if UNITY_STANDALONE
        tmpro.alignment = TextAlignmentOptions.BottomRight;
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        #else
        tmpro.alignment = TextAlignmentOptions.Bottom;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        #endif
    }
}