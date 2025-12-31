using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using TMPro;

public class SettingsLanguage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI curLanText, otherLanText;
    [SerializeField] private GameObject selectButton;
    private string curLanguage;
    private bool isButtonOpen;

    private void Start()
    {
        curLanguage = PersistentDataManager.Instance.GetCurLanguageCode();
        curLanText.text = curLanguage == "ko" ? "한국어" : "English";
        otherLanText.text = curLanguage == "ko" ? "English" : "한국어";
        isButtonOpen = false;
    }

    public void OnMainButtonClick()
    {
        isButtonOpen = !isButtonOpen;
        selectButton.SetActive(isButtonOpen);
    }

    public void ChangeToOtherLanguage()
    {
        if (curLanguage == "ko")
            StartCoroutine(ChangeLocale("en"));
        else
            StartCoroutine(ChangeLocale("ko"));
    }

    private IEnumerator ChangeLocale(string localeCode)
    {
        // Localization 시스템 초기화 대기
        yield return LocalizationSettings.InitializationOperation;

        foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == localeCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                curLanguage = localeCode;
                curLanText.text = curLanguage == "ko" ? "한국어" : "English";
                otherLanText.text = curLanguage == "ko" ? "English" : "한국어";
                PersistentDataManager.Instance.SaveCurLanguageData(localeCode);
                OnMainButtonClick();
                yield break;
            }
        }

        Debug.LogWarning($"Locale not found: {localeCode}");
    }
}
