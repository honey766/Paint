using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using TMPro;

public class Settings : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private Sprite[] bgmSpr;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Image bgmImage;

    [Header("SFX")]
    [SerializeField] private Sprite[] sfxSpr;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Image sfxImage;

    [Header("Movement Mode")]
    [SerializeField] private GameObject tileTouchOutline;
    [SerializeField] private GameObject joyStickOutline;
    [SerializeField] private TextMeshProUGUI movementInformationText;

    [Header("Move Latency Rate")]
    [SerializeField] private Slider moveLatencyRateSlider;
    [SerializeField] private GameObject  moveLatencyBlockObj, moveLatencyRateObj;

    [Header("Notice")]
    [SerializeField] private Sprite[] noticeSpr;
    [SerializeField] private Image noticeImage;

    private bool isMoveTooltipActivated;

    int moveLatencyRate;
    private bool isTileTouch;
    private bool isNotice;

    private void OnEnable()
    {
        isMoveTooltipActivated = false;

        int bgmV = LoadBGM();
        int sfxV = LoadSFX();
        moveLatencyRate = LoadMoveLatencyRate();
        isTileTouch = LoadIsTileTouch();
        isNotice = LoadNotice();

        bgmSlider.value = bgmV / 100f + 0.0001f;
        OnBGMChanged(bgmV);
        sfxSlider.value = sfxV / 100f + 0.0001f;
        OnSFXChanged(sfxV);
        moveLatencyRateSlider.value = moveLatencyRate / 100f + 0.0001f;
        noticeImage.sprite = isNotice ? noticeSpr[1] : noticeSpr[0];
        SetMovementModeUI();
    }

    public void OnSettingExit()
    {
        SaveBgm((int)(bgmSlider.value * 100));
        SaveSfx((int)(sfxSlider.value * 100));
        SaveMoveLatencyRate((int)(moveLatencyRateSlider.value * 100));
        SaveIsTileTouch(isTileTouch);
        SaveNotice(isNotice);
        PersistentDataManager.Instance.SaveSettings(isTileTouch, moveLatencyRateSlider.value);
        PlayerPrefs.Save();
        if (SceneManager.GetActiveScene().name == "InGame")
            GameManager.Instance.SettingsExit();
        MoveTutorialTooltip moveTutorial = FindAnyObjectByType<MoveTutorialTooltip>();
        if (moveTutorial != null) moveTutorial.SetMoveTutorialText();
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

    public void OnMovementInformationButtonClicked()
    {
        isMoveTooltipActivated = !isMoveTooltipActivated;
        movementInformationText.transform.parent.gameObject.SetActive(isMoveTooltipActivated);
        if (isTileTouch) movementInformationText.text = "[타일 터치] 타일을 직접 터치해서\n                    해당 타일의 위치로 이동해요.";
        else movementInformationText.text = "[스와이프] 화면을 상하좌우로 스와이프해서\n                   캐릭터를 움직여요.";
    }

    public void OnTileTouchButtonClicked()
    {
        isTileTouch = true;
        SetMovementModeUI();
    }

    public void OnJoyStickButtonClicked()
    {
        isTileTouch = false;
        SetMovementModeUI();
    }

    private void SetMovementModeUI()
    {
        tileTouchOutline.SetActive(isTileTouch);
        joyStickOutline.SetActive(!isTileTouch);
        moveLatencyBlockObj.SetActive(isTileTouch);
        moveLatencyRateObj.SetActive(!isTileTouch);
    }

    public void OnNoticeClicked()
    {
        isNotice = !isNotice;
        noticeImage.sprite = isNotice ? noticeSpr[1] : noticeSpr[0];
    }


    public static int LoadBGM() => PlayerPrefs.GetInt("bgm", 100);
    public static int LoadSFX() => PlayerPrefs.GetInt("sfx", 100);
    public static int LoadMoveLatencyRate() => PlayerPrefs.GetInt("moveLatencyRate", 70);
    public static bool LoadIsTileTouch() => PlayerPrefs.GetInt("isTileTouch", 1) == 1;
    public static bool LoadNotice() => PlayerPrefs.GetInt("notice", 1) == 1;

    public void SaveBgm(int bgm) => PlayerPrefs.SetInt("bgm", bgm);
    public void SaveSfx(int sfx) => PlayerPrefs.SetInt("sfx", sfx);
    public void SaveMoveLatencyRate(int moveLatencyRate) => PlayerPrefs.SetInt("moveLatencyRate", moveLatencyRate);
    public void SaveIsTileTouch(bool isTileTouch) => PlayerPrefs.SetInt("isTileTouch", isTileTouch ? 1 : 0);
    public void SaveNotice(bool notice) => PlayerPrefs.SetInt("notice", notice ? 1 : 0);
}
