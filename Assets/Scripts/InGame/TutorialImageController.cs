using UnityEngine;
using UnityEngine.UI;

public class TutorialImageController : MonoBehaviour
{
    [SerializeField] private int page = 3;
    [SerializeField] private int tutorialNum = 1;
    [SerializeField] private Image tutorialImg;
    [SerializeField] private Sprite[] tutorialSprites;
    private int curPage;

    private void Awake()
    {
        curPage = 0;
        SetTutoImage();
    }

    public void OnLeftClick()
    {
        if (curPage <= 0) return;
        curPage--;
        SetTutoImage();
    }

    public void OnRightClick()
    {
        curPage++;
        if (curPage >= page)
        {
            if (tutorialNum == 1)
            {
                GameManager.Instance.isGaming = true;
                GameObject movetutorialCanvas = Resources.Load<GameObject>("Prefabs/MoveTutorialCanvas");
                Instantiate(movetutorialCanvas);
            }
            else if (tutorialNum == 2)
            {
                GameManager.Instance.SecondTutorialEvent();
                GameManager.Instance.isGaming = true;
            }
            else if (tutorialNum == 3)
            {
                GameManager.Instance.GoToNextStage();
            }
            Destroy(gameObject);
        }
        else SetTutoImage();
    }

    private void SetTutoImage()
    {
        if (tutorialNum == 1)
            tutorialImg.sprite = tutorialSprites[curPage];
    }
}
