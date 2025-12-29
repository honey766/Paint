using UnityEngine;
using UnityEngine.Localization.Settings;

public class Tutorial4Crepas : MonoBehaviour
{
    void Start()
    {
        if (LocalizationSettings.SelectedLocale.Identifier.Code == "en")
            GetComponent<RectTransform>().anchoredPosition = new Vector2(-54.8f, -142.4f);
    }
}
