using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private Sprite[] bgmSpr, sfxSpr, noticeSpr;
    [SerializeField] private Slider bgmSlider, sfxSlider, moveDurationSlider;
    [SerializeField] private Image bgmImage, sfxImage, noticeImage;

    private bool isNotice;

    private void OnEnable()
    {
        int bgmV = LoadBGM();
        int sfxV = LoadSFX();
        int moveDurationV = LoadMoveDuration();
        isNotice = LoadNotice();

        bgmSlider.value = bgmV / 100f + 0.0001f;
        OnBGMChanged(bgmV);
        sfxSlider.value = sfxV / 100f + 0.0001f;
        OnSFXChanged(sfxV);
        moveDurationSlider.value = moveDurationV / 100f + 0.0001f;
        noticeImage.sprite = isNotice ? noticeSpr[1] : noticeSpr[0];
        Debug.Log($"Enter bgm : {bgmSlider.value}, sfx : {sfxSlider.value}");
    }

    public void OnSettingExit()
    {
        SaveBgm((int)(bgmSlider.value * 100));
        SaveSfx((int)(sfxSlider.value * 100));
        SaveMoveDuration((int)(moveDurationSlider.value * 100));
        SaveNotice(isNotice);
        PlayerPrefs.Save();
        Destroy(gameObject);
    }

    public void OnBGMChanged(float value)
    {
        bgmImage.sprite = value == 0 ? bgmSpr[0] : bgmSpr[1];
    }

    public void OnSFXChanged(float value)
    {
        sfxImage.sprite = value == 0 ? sfxSpr[0] : sfxSpr[1];
    }

    public void OnNoticeClicked()
    {
        isNotice = !isNotice;
        noticeImage.sprite = isNotice ? noticeSpr[1] : noticeSpr[0];
    }


    public static int LoadBGM()
    {
        if (PlayerPrefs.HasKey("bgm"))
        {
            return PlayerPrefs.GetInt("bgm");
        }
        else
        {
            PlayerPrefs.SetInt("bgm", 100);
            return 100;
        }
    }

    public static int LoadSFX()
    {
        if (PlayerPrefs.HasKey("sfx"))
        {
            return PlayerPrefs.GetInt("sfx");
        }
        else
        {
            PlayerPrefs.SetInt("sfx", 100);
            return 100;
        }
    }

    public static int LoadMoveDuration()
    {
        if (PlayerPrefs.HasKey("moveDuration"))
        {
            return PlayerPrefs.GetInt("moveDuration");
        }
        else
        {
            PlayerPrefs.SetInt("moveDuration", 100);
            return 100;
        }
    }

    public static bool LoadNotice()
    {
        if (PlayerPrefs.HasKey("notice"))
        {
            return PlayerPrefs.GetInt("notice") == 1;
        }
        else
        {
            PlayerPrefs.SetInt("notice", 1);
            return true;
        }
    }

    public void SaveBgm(int bgm) => PlayerPrefs.SetInt("bgm", bgm);
    public void SaveSfx(int sfx) => PlayerPrefs.SetInt("sfx", sfx);
    public void SaveMoveDuration(int moveDuration) => PlayerPrefs.SetInt("moveDuration", moveDuration);
    public void SaveNotice(bool notice) => PlayerPrefs.SetInt("notice", notice ? 1 : 0);
}
