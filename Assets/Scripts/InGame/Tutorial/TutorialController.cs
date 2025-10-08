using UnityEngine;
using DG.Tweening;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialColor12Border;
    [SerializeField] private GameObject tutorialArrow;
    [SerializeField] private GameObject tutorialBorderTooltip;
    [SerializeField] private GameObject tutorialCanvas1, tutorialCanvas2, tutorialCanvas3;
    [SerializeField] private GameObject tutorialAnswerButton;
    private MeshRenderer color12Border;
    private GameObject tutorialColor12BorderTempObj;
    private GameObject moveTutorialCanvasObj;
    private bool tutorialIsOpenedAnswerButton;
    private int firstTutorialArrowStatus;
    [SerializeField] private int tutorialLevel;

    private void Awake()
    {
        color12Border = GameObject.Find("Color12BorderDrawer").GetComponent<MeshRenderer>();
    }

    public void TutorialClearEvent(int star)
    {
        tutorialLevel++;
        if (tutorialLevel == 1)
        {
            GameManager.Instance.isGaming = false;
            PersistentDataManager.Instance.LoadTutorialLevel(2);
            Instantiate(tutorialCanvas2);
            moveTutorialCanvasObj = GameObject.Find("MoveTutorialCanvas(Clone)");
            moveTutorialCanvasObj.SetActive(false);
            tutorialArrow.SetActive(false);
            GameManager.Instance.Start();
            color12Border.enabled = false;
            GameManager.Instance.isGaming = false; // Start에서 isGaming이 true가 되므로 한 번 더 false
            PlayerController.Instance.MoveEvent = null;
        }
        else if (tutorialLevel == 2)
        {
            PersistentDataManager.Instance.LoadTutorialLevel(3);
            GameManager.Instance.Start();
            ThirdTutorialEvent();
        }
        else if (tutorialLevel == 3)
        {
            Instantiate(tutorialCanvas3);
            GameObject moveTutorialCanvas = GameObject.Find("MoveTutorialCanvas(Clone)");
            Destroy(moveTutorialCanvas);
            Destroy(tutorialBorderTooltip);
            Destroy(tutorialAnswerButton);
            GameManager.Instance.isGaming = false;
            PersistentDataManager.Instance.SetStageClearData(star);
        }
    }
    public void FirstTutorialEvent()
    {
        GameManager.Instance.isGaming = false;
        tutorialLevel = 0;
        Instantiate(tutorialCanvas1);
    }
    public void FirstTutorialImageCloseEvent()
    {
        tutorialArrow = Instantiate(tutorialArrow);
        tutorialArrow.transform.position = Board.Instance.GetTilePos(0, -1);
        firstTutorialArrowStatus = 0;
        PlayerController.Instance.MoveEvent += (pos) =>
        {
            Logger.Log($"HIHI!! {pos}");
            // 처음 상태
            if (firstTutorialArrowStatus == 0)
            {
                // 오른쪽 끝까지 간 경우, 파란색 획득
                if (pos == new Vector2Int(4, 0))
                {
                    firstTutorialArrowStatus = 10;
                }
                // 왼쪽 끝까지 간 경우, 빨간색 획득
                else if (pos == new Vector2Int(0, 0))
                {
                    firstTutorialArrowStatus = 1;
                    tutorialArrow.transform.DOMove(Board.Instance.GetTilePos(4, -1), 0.2f);
                }
            }
            else if (firstTutorialArrowStatus == 1)
            {
                // 빨간 상태에서 오른쪽 끝까지 감
                if (pos == new Vector2Int(4, 0))
                {
                    firstTutorialArrowStatus = 2;
                    tutorialArrow.transform.DOMove(Board.Instance.GetTilePos(1, -1), 0.2f);
                }
            }
            else if (firstTutorialArrowStatus == 10)
            {
                // 파란 상태에서 왼쪽 끝까지 감
                if (pos == new Vector2Int(0, 0))
                {
                    firstTutorialArrowStatus = 11;
                    tutorialArrow.transform.DOMove(Board.Instance.GetTilePos(3, -1), 0.2f);
                }
            }
        };
    }
    public void SecondTutorialEvent()
    {
        moveTutorialCanvasObj.SetActive(true);
        tutorialColor12BorderTempObj = Instantiate(tutorialColor12Border);
        tutorialColor12BorderTempObj.transform.position = Board.Instance.GetTilePos(2, 1);
        tutorialArrow.SetActive(true);
        tutorialArrow.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
        tutorialArrow.transform.position = Board.Instance.GetTilePos(2, 0);
        tutorialBorderTooltip = Instantiate(tutorialBorderTooltip);
        tutorialAnswerButton = Instantiate(tutorialAnswerButton);
        tutorialAnswerButton.SetActive(false);
        tutorialIsOpenedAnswerButton = false;
    }
    public void ThirdTutorialEvent()
    {
        Destroy(tutorialColor12BorderTempObj);
        Invoke("ThirdTutorialEventAfterSeconds", 0.1f);
        tutorialArrow.transform.position = Board.Instance.GetTilePos(1, -1);
        tutorialAnswerButton.SetActive(true);
        tutorialAnswerButton.GetComponent<TutorialAnswerButton>().OnTutorial3();
        tutorialAnswerButton.SetActive(false);
        tutorialIsOpenedAnswerButton = false;
    }
    private void ThirdTutorialEventAfterSeconds()
    {
        tutorialColor12BorderTempObj = Instantiate(tutorialColor12Border);
        tutorialColor12Border.transform.position = Board.Instance.GetTilePos(1, 0);
    }
    public void ShowTutorialAnswerButton()
    {
        if (tutorialIsOpenedAnswerButton == false)
        {
            tutorialIsOpenedAnswerButton = true;
            tutorialAnswerButton.SetActive(true);
            RectTransform rect = tutorialAnswerButton.transform.GetChild(0).GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(-200, rect.offsetMin.y);
            DOTween.To(
                () => rect.offsetMin.x,
                x => rect.offsetMin = new Vector2(x, rect.offsetMin.y),
                0f,
                0.6f
            );
        }
    }
}