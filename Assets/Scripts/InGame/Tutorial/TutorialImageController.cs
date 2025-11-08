using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialImageController : MonoBehaviour
{
    [SerializeField] private int tutorialNum = 1;
    [Header("Only Tutorial1")]
    [SerializeField] private int page = 3;
    [SerializeField] private GameObject leftButton, rightButton, exitButton;
    [SerializeField] private GameObject tuto1_1AnimImage;
    [SerializeField] private GameObject tuto1_2AnimImage;
    [SerializeField] private GameObject tuto1_3Image;
    [SerializeField] private TextMeshProUGUI tuto1_text;
    [SerializeField] private GameObject movetutorialCanvas;
    [SerializeField] private RectTransform[] pageDots;
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

    private void SetTutoImage()
    {
        if (tutorialNum == 1)
        {
            tuto1_text.text = tuto1_text_content[curPage];
            tuto1_1AnimImage.SetActive(curPage == 0);
            tuto1_2AnimImage.SetActive(curPage == 1);
            tuto1_3Image.SetActive(curPage == 2);
            leftButton.SetActive(curPage > 0);
            rightButton.SetActive(curPage < 2);
            exitButton.SetActive(curPage == 2);
            pageDots[curPage].DOSizeDelta(Vector2.one * 40, 0.2f);
        }
    }
}
