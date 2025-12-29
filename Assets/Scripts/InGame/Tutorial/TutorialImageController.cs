using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Localization.Settings;

public class TutorialImageController : MonoBehaviour
{
    [SerializeField] private int tutorialNum = 1;
    [Header("Only Tutorial1")]
    [SerializeField] private int page = 3;
    [SerializeField] private GameObject leftButton, rightButton, exitButton;
    [SerializeField] private GameObject[] tuto1_1Images;
    [SerializeField] private GameObject[] tuto1_2Images;
    [SerializeField] private GameObject[] tuto1_3Images;
    [SerializeField] private TextMeshProUGUI tuto1_text;
    [SerializeField] private GameObject movetutorialCanvas;
    [SerializeField] private RectTransform[] pageDots;
    [SerializeField] private RectTransform tuto2PurpleBackground, tuto3_1PurpleBackground, tuto3_2PurpleBackground;
    private int curPage;
    
    private static readonly string[] tuto1_text_content = new string[3]
    {
        "페인트 통<sprite=5> 위를 지나면\n타일<sprite=0><size=60%> </size>에 <color=#FE7269>페인트</color>를 칠해요.",

        "<color=#FE7269>빨간색</color><sprite=2><size=60%> </size>과 <color=#3D91FF>파란색</color><sprite=3> 모두를\n"
        + "타일<sprite=0><size=60%> </size>에 칠하면 타일이\n"
        + "<color=#E473FF>보라색</color><sprite=4><size=60%> </size>이 돼요.",

        "<color=#E473FF>보라색 테두리</color><sprite=1><size=60%> </size>에 <color=#E473FF>보라색</color>을\n채워 넣어야 해요."
    };

    private void Awake()
    {
        curPage = 0;
        SetTutoImage();
        if (tutorialNum != 1)
            foreach (var entry in pageDots)
                entry.gameObject.SetActive(false);
    }

    public void OnLeftClick()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        if (curPage <= 0) return;

        if (tutorialNum == 1)
            pageDots[curPage].DOSizeDelta(Vector2.one * 25, 0.2f);
        curPage--;
        SetTutoImage();
    }

    public void OnRightClick()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        if (tutorialNum == 1 && curPage == page - 1)
            return;

        if (tutorialNum == 1)
            pageDots[curPage].DOSizeDelta(Vector2.one * 25, 0.2f);
        curPage++;

        if (curPage >= page)
        {
            if (tutorialNum == 3)
            {
                GameManager.Instance.GoToNextLevel();
            }
        }
        else SetTutoImage();
    }

    public void OnExitClick()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        GameManager.Instance.isGaming = true;
        if (tutorialNum == 1)
        {
            Instantiate(movetutorialCanvas);
            FindAnyObjectByType<TutorialController>().FirstTutorialImageCloseEvent();
        }
        else if (tutorialNum == 2)
        {
            FindAnyObjectByType<TutorialController>().SecondTutorialEvent();
        }
        Destroy(gameObject);
    }

    private string GetTutoText(int curPage)
    {
        curPage++;
        return LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "Tuto" + curPage, LocalizationSettings.SelectedLocale);
    }
    private void SetTutoImage()
    {
        if (tutorialNum == 1)
        {
            // tuto1_text.text = tuto1_text_content[curPage];
            tuto1_text.text = GetTutoText(curPage);
            foreach (GameObject obj in tuto1_1Images)
                obj.SetActive(curPage == 0);
            foreach (GameObject obj in tuto1_2Images)
                obj.SetActive(curPage == 1);
            foreach (GameObject obj in tuto1_3Images)
                obj.SetActive(curPage == 2);
            leftButton.SetActive(curPage > 0);
            rightButton.SetActive(curPage < 2);
            exitButton.SetActive(curPage == 2);
            pageDots[curPage].DOSizeDelta(Vector2.one * 40, 0.2f);
            if (LocalizationSettings.SelectedLocale.Identifier.Code == "en")
            {
                if (curPage == 1)
                {
                    tuto2PurpleBackground.anchoredPosition = new Vector2(-32.5f, 370);
                    tuto2PurpleBackground.localScale = Vector3.one * 0.4f;
                }
                else if (curPage == 2)
                {
                    tuto3_1PurpleBackground.anchoredPosition = new Vector2(-174, 436);
                    tuto3_1PurpleBackground.localScale = new Vector3(0.83f, 0.372f, 0.372f);
                    tuto3_2PurpleBackground.anchoredPosition = new Vector2(-129.3f, 355.5f);
                    tuto3_2PurpleBackground.localScale = new Vector3(1.05f, 0.372f, 0.372f);
                }
            }
        }
    }
}
