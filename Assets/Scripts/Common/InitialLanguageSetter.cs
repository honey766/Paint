using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;

public class InitialLanguageSetter : MonoBehaviour
{
    private const string LANGUAGE_KEY = "LANGUAGE_SET";

    private IEnumerator Start()
    {
        // Localization 시스템 초기화 대기
        yield return LocalizationSettings.InitializationOperation;

        string localeCode;
        bool isFirstLanguageSet;
        if (PlayerPrefs.HasKey(LANGUAGE_KEY))
        {
            // 첫 실행 시 언어 설정
            isFirstLanguageSet = true;
            localeCode = GetLocaleCodeBySystemLanguage();
            PersistentDataManager.Instance.SaveCurLanguageData(localeCode);
        }
        else
        {
            isFirstLanguageSet = false;
            localeCode = PersistentDataManager.Instance.GetCurLanguageCode();
        }

        foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == localeCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                if (isFirstLanguageSet)
                {
                    PlayerPrefs.SetInt(LANGUAGE_KEY, 1);
                    PlayerPrefs.Save();
                }
                break;
            }
        }
    }

    private string GetLocaleCodeBySystemLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean:
                return "ko";
            default:
                return "en";
        }
    }
}