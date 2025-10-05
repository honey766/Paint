using UnityEngine;
using UnityEngine.UI;

public class TutorialImageController : MonoBehaviour
{
    [SerializeField] private int page = 3;
    [SerializeField] private bool isTutorial1;
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
            if (isTutorial1)
            {
                GameManager.Instance.isGaming = true;
                GameObject movetutorialCanvas = Resources.Load<GameObject>("Prefabs/MoveTutorialCanvas");
                Instantiate(movetutorialCanvas);
            }
            Destroy(gameObject);
        }
        else SetTutoImage();
    }

    private void SetTutoImage()
    {
        tutorialImg.sprite = tutorialSprites[curPage];
    }
}
