using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Threading.Tasks;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialColor12Border;
    [SerializeField] private GameObject tutorialArrow;
    [SerializeField] private GameObject tutorialTouchAnimation;
    [SerializeField] private GameObject tutorialBorderTooltip;
    [SerializeField] private GameObject tutorialCanvas1, tutorialCanvas2, tutorialCanvas3;
    [SerializeField] private GameObject tutorial1_1_2Answer, tutorial1_1_3Answer;
    // [SerializeField] private GameObject tutorialAnswerButton;
    //private MeshRenderer color12Border;
    private GameObject color12Lines;
    private GameObject tutorialColor12BorderTempObj;
    private GameObject moveTutorialCanvasObj;
    private GameObject tutorialArrowObj;
    private GameObject tutorialTouchAnimationObj;
    private GameObject tutorial1_1_2AnswerObj, tutorial1_1_3AnswerObj;
    //private GameObject highlightHintTextObj;
    private GameObject hintButton, hintExplainText, redoButton, redoExplainText, restartExplainText;
    // private bool tutorialIsOpenedAnswerButton;
    private int firstTutorialArrowStatus;
    private RectTransform moveCountTutoRect;
    [SerializeField] private int tutorialLevel;

    private Task tuto2LoadTask, tuto3LoadTask;

    private void Awake()
    {
        PersistentDataManager.Instance.PreLoadTutorialLevel();

        //color12Border = GameObject.Find("Color12BorderDrawer").GetComponent<MeshRenderer>();
        color12Lines = GameObject.Find("PurpleLines");

        hintButton = GameObject.Find("HintButton");
        hintExplainText = hintButton.transform.GetChild(1).gameObject;
        hintButton.SetActive(false);
        hintExplainText.SetActive(true);

        redoButton = GameObject.Find("RedoButton");
        redoExplainText = redoButton.transform.GetChild(2).gameObject;
        redoButton.SetActive(false);
        redoExplainText.SetActive(true);

        restartExplainText = GameObject.Find("RestartButton").transform.GetChild(2).gameObject;
        restartExplainText.SetActive(true);

        tutorialArrowObj = null;
        tutorialTouchAnimationObj = null;
        moveCountTutoRect = null;
        GameManager.Instance.HideStar();
    }

    /// <summary>
    /// 튜토리얼 마지막 클리어라면 true
    /// </summary>
    public async Task<bool> TutorialClearEvent(int star)
    {
        tutorialLevel++;
        if (tutorialLevel == 1)
        {
            GameManager.Instance.isGaming = false;
            await PersistentDataManager.Instance.LoadTutorialLevelAsync(2);
            Instantiate(tutorialCanvas2);
            moveTutorialCanvasObj = GameObject.Find("MoveTutorialCanvas(Clone)");
            moveTutorialCanvasObj.GetComponent<MoveTutorialTooltip>().EnteredTutorialTwo();
            moveTutorialCanvasObj.SetActive(false);
            if (tutorialArrowObj != null)
                tutorialArrowObj.SetActive(false);
            if (tutorialTouchAnimationObj != null)
                Destroy(tutorialTouchAnimationObj);
            GameManager.Instance.Start();
            //color12Border.enabled = false;
            color12Lines.SetActive(false);
            GameManager.Instance.isGaming = false; // Start에서 isGaming이 true가 되므로 한 번 더 false
            PlayerController.Instance.MoveEvent = null;
            hintButton.gameObject.SetActive(true);
            redoButton.SetActive(true);
        }
        else if (tutorialLevel == 2)
        {
            await PersistentDataManager.Instance.LoadTutorialLevelAsync(3);
            Transform blackLineParent = GameObject.Find("BlackLines").transform;
            foreach (Transform child in blackLineParent)
                child.gameObject.SetActive(false);
            GameManager.Instance.Start();
            ThirdTutorialEvent();
        }
        else if (tutorialLevel == 3)
        {
            // Instantiate(tutorialCanvas3);
            tutorialArrowObj.SetActive(false);
            GameObject moveTutorialCanvas = GameObject.Find("MoveTutorialCanvas(Clone)");
            Destroy(moveTutorialCanvas);
            GameManager.Instance.isGaming = false;
            PersistentDataManager.Instance.SetStageClearData(star);
            PersistentDataManager.Instance.ReleaseTutorialLevelAsync();
        }

        return tutorialLevel == 3;
    }
    public void FirstTutorialEvent()
    {
        GameManager.Instance.isGaming = false;
        tutorialLevel = 0;
        Instantiate(tutorialCanvas1);
    }
    public void FirstTutorialImageCloseEvent()
    {
        tutorialTouchAnimationObj = Instantiate(tutorialTouchAnimation);
        tutorialArrowObj = Instantiate(tutorialArrow);
        if (PersistentDataManager.Instance.isTileTouch) tutorialArrowObj.SetActive(false);
        else tutorialTouchAnimationObj.SetActive(false);
            
        firstTutorialArrowStatus = 0;
        Transform arrowTr = tutorialArrowObj.transform;
        Vector2 touchOffset = new Vector2(0.1f, 0.6f);

        tutorialTouchAnimationObj.transform.position = Board.Instance.GetTilePos(0, -1) + touchOffset;
        arrowTr.position = Board.Instance.GetTilePos(0, -1);

        PlayerController.Instance.MoveEvent += (pos) =>
        {
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

                    if (PersistentDataManager.Instance.isTileTouch)
                    {
                        Destroy(tutorialTouchAnimationObj);
                        tutorialTouchAnimationObj = Instantiate(tutorialTouchAnimation);
                    }
                    tutorialTouchAnimationObj.transform.position = Board.Instance.GetTilePos(4, -1) + touchOffset;
                    if (!PersistentDataManager.Instance.isTileTouch)
                        arrowTr.DOMove(Board.Instance.GetTilePos(4, -1), 0.2f);
                    else
                        arrowTr.position = Board.Instance.GetTilePos(4, -1);
                }
            }
            else if (firstTutorialArrowStatus == 1)
            {
                // 빨간 상태에서 오른쪽 끝까지 감
                if (pos == new Vector2Int(4, 0))
                {
                    firstTutorialArrowStatus = 2;

                    if (PersistentDataManager.Instance.isTileTouch)
                    {
                        Destroy(tutorialTouchAnimationObj);
                        tutorialTouchAnimationObj = Instantiate(tutorialTouchAnimation);
                    }
                    tutorialTouchAnimationObj.transform.position = Board.Instance.GetTilePos(1, -1) + touchOffset;
                    if (!PersistentDataManager.Instance.isTileTouch)
                        arrowTr.DOMove(Board.Instance.GetTilePos(1, -1), 0.2f);
                    else
                        arrowTr.position = Board.Instance.GetTilePos(1, -1);
                }
            }
            else if (firstTutorialArrowStatus == 10)
            {
                // 파란 상태에서 왼쪽 끝까지 감
                if (pos == new Vector2Int(0, 0))
                {
                    firstTutorialArrowStatus = 11;

                    if (PersistentDataManager.Instance.isTileTouch)
                    {
                        Destroy(tutorialTouchAnimationObj);
                        tutorialTouchAnimationObj = Instantiate(tutorialTouchAnimation);
                    }
                    tutorialTouchAnimationObj.transform.position = Board.Instance.GetTilePos(3, -1) + touchOffset;
                    if (!PersistentDataManager.Instance.isTileTouch)
                        arrowTr.DOMove(Board.Instance.GetTilePos(3, -1), 0.2f);
                    else
                        arrowTr.position = Board.Instance.GetTilePos(3, -1);
                }
            }
        };
    }
    public void RestartWhenFirstTutorial()
    {
        if (tutorialLevel != 0) return;
        firstTutorialArrowStatus = 0;
        tutorialTouchAnimationObj.transform.position = Board.Instance.GetTilePos(0, -1) + new Vector2(0.1f, 0.6f);
        tutorialArrowObj.transform.position = Board.Instance.GetTilePos(0, -1);
    }
    public void SettingsExitWhenFirstTutorial()
    {
        if (tutorialLevel != 0) return;
        tutorialTouchAnimationObj.SetActive(PersistentDataManager.Instance.isTileTouch);
        tutorialArrowObj.SetActive(!PersistentDataManager.Instance.isTileTouch);
    }
    public void SecondTutorialEvent()
    {
        moveTutorialCanvasObj.SetActive(true);
        moveTutorialCanvasObj.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
            "<color=#D64EF5>보라색 테두리</color> 안에만 <color=#D64EF5>보라색</color>을 칠해야 해요.\n"
          + "<color=#FF736A>빨강</color>, <color=#3C91FF>파랑</color>은 어떤 곳에든 칠할 수 있어요.";
        tutorialColor12BorderTempObj = Instantiate(tutorialColor12Border);
        tutorialColor12BorderTempObj.transform.position = Board.Instance.GetTilePos(2, 1);
        if (tutorialArrowObj != null) tutorialArrowObj.SetActive(true);
        else tutorialArrowObj = Instantiate(tutorialArrow);
        tutorialArrowObj.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
        tutorialArrowObj.transform.localScale = new Vector3(1, -1, 1);
        tutorialArrowObj.transform.position = Board.Instance.GetTilePos(2, 2);
    }
    public void ThirdTutorialEvent()
    {
        Destroy(tutorialColor12BorderTempObj);
        Invoke("ThirdTutorialEventAfterSeconds", 0.1f);
        tutorialArrowObj.transform.position = Board.Instance.GetTilePos(1, 1);
        RectTransform moveTutoText = moveTutorialCanvasObj.transform.GetChild(0).GetChild(1).GetComponent<RectTransform>();
        moveTutoText.offsetMin = new Vector2(130, moveTutoText.offsetMin.y); // Left
        moveTutoText.offsetMax = new Vector2(-130, moveTutoText.offsetMax.y); // Right
        moveTutoText.GetComponent<TextMeshProUGUI>().text =
            "플레이어의 이동횟수가 적을수록 ↗\n"
          + "더 많은 <color=#B6A33F>별</color>을 획득해요.  ";
        moveCountTutoRect = moveTutoText.transform.parent.GetChild(2).GetComponent<RectTransform>();
        moveCountTutoRect.gameObject.SetActive(true);
        if (tutorial1_1_2AnswerObj != null)
            Destroy(tutorial1_1_2AnswerObj);
        GameManager.Instance.ShowStarForTutorial();
    }
    private void ThirdTutorialEventAfterSeconds()
    {
        tutorialColor12BorderTempObj = Instantiate(tutorialColor12Border);
        tutorialColor12Border.transform.position = Board.Instance.GetTilePos(1, 0);
    }

    public void ShowHint()
    {
        if (tutorialLevel == 1)
        {
            if (tutorial1_1_2AnswerObj == null)
                tutorial1_1_2AnswerObj = Instantiate(tutorial1_1_2Answer);
            else tutorial1_1_2AnswerObj.SetActive(true);
        }
        else
        {
            if (tutorial1_1_3AnswerObj == null)
                tutorial1_1_3AnswerObj = Instantiate(tutorial1_1_3Answer);
            else tutorial1_1_3AnswerObj.SetActive(true);
        }
    }
}