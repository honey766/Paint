using UnityEngine;

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
