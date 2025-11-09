using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using TMPro;
using System;

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

    [Header("Email")]
    private string recipientEmail = "parkhoboy@gmail.com,honey76684@gmail.com"; // 개발팀의 이메일 주소
    // 이메일 제목(subject)과 본문(body)을 미리 설정할 수 있습니다.
    // URL 인코딩이 필요하지만, Unity의 OpenURL은 기본적인 문자열에 대해서는 어느 정도 처리합니다.
    private string subject = "Game Support Inquiry";
    private string body = "Please describe your issue here...";

    [Header("Privacy Policy")]
    private string privacyPolicyUrl = "https://blog.naver.com/honey766/224033045398";

    private bool isMoveTooltipActivated;
    int moveLatencyRate;
    private bool isTileTouch;
    private bool isNotice;
    
    private void OnEnable()
    {
        isMoveTooltipActivated = false;

        int bgmV = PersistentDataManager.LoadBGM();
        int sfxV = PersistentDataManager.LoadSFX();
        moveLatencyRate = PersistentDataManager.LoadMoveLatencyRate();
        isTileTouch = PersistentDataManager.LoadIsTileTouch();
        isNotice = PersistentDataManager.LoadIsNoticeEnabled();

        bgmSlider.value = bgmV / 100f + 0.0001f;
        OnBGMChanged(bgmSlider.value);
        sfxSlider.value = sfxV / 100f + 0.0001f;
        OnSFXChanged(sfxSlider.value);
        moveLatencyRateSlider.value = moveLatencyRate / 100f + 0.0001f;
        noticeImage.sprite = isNotice ? noticeSpr[1] : noticeSpr[0];
        SetMovementModeUI();
    }

    public void OnSettingExit()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        PersistentDataManager.SaveBgm((int)(bgmSlider.value * 100));
        PersistentDataManager.SaveSfx((int)(sfxSlider.value * 100));
        PersistentDataManager.SaveMoveLatencyRate((int)(moveLatencyRateSlider.value * 100));
        PersistentDataManager.SaveIsTileTouch(isTileTouch);
        PersistentDataManager.SaveIsNoticeEnabled(isNotice);
        PersistentDataManager.Instance.SaveSettings(isTileTouch, moveLatencyRateSlider.value);
        PlayerPrefs.Save();
        if (SceneManager.GetActiveScene().name == "InGame")
            GameManager.Instance.SettingsExit();

        MoveTutorialTooltip moveTutorial = FindAnyObjectByType<MoveTutorialTooltip>();
        if (moveTutorial != null) moveTutorial.SetMoveTutorialText();

        TutorialController tc = FindAnyObjectByType<TutorialController>();
        if (tc != null) tc.SettingsExitWhenFirstTutorial();

        Destroy(gameObject);
    }

    public void OnBGMChanged(float value)
    {
        bgmImage.sprite = value == 0 ? bgmSpr[0] : bgmSpr[1];
        AudioManager.Instance.SetBGMVolume(value);
    }

    public void OnSFXChanged(float value)
    {
        sfxImage.sprite = value == 0 ? sfxSpr[0] : sfxSpr[1];
        AudioManager.Instance.SetSFXVolume(value);
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

    public void OpenEmailClient()
    {
        // 'mailto:' 프로토콜을 사용하여 이메일 클라이언트 실행을 지시합니다.
        // URL 형식: mailto:수신자주소?subject=제목&body=본문

        // 제목과 본문을 URL 안전 문자열로 인코딩합니다. (선택적이지만 권장)
        // Unity에는 표준 URL 인코더가 없으므로, 필요하다면 .NET의 HttpUtility.UrlEncode 등을 사용하거나 수동으로 처리해야 합니다.
        // 여기서는 간단하게 string.Format을 사용하여 필수적인 부분만 구성합니다.

        string mailToUrl = string.Format("mailto:{0}?subject={1}&body={2}",
            recipientEmail,
            Uri.EscapeDataString(subject), // 제목 인코딩
            Uri.EscapeDataString(body));    // 본문 인코딩

        // WWW.EscapeURL()은 Unity에서 URL 인코딩을 위한 함수입니다.

        Application.OpenURL(mailToUrl);

        Debug.Log("OpenURL called with: " + mailToUrl);
    }

    public void OpenPrivacyPolicy()
    {
        // 웹 브라우저를 열고 지정된 URL로 이동
        Application.OpenURL(privacyPolicyUrl);
    }
}
