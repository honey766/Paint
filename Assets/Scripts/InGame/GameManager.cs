using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : SingletonBehaviour<GameManager>
{
    public bool isGaming;
    public GameObject gameClearObj;
    public CameraSizeController cameraSizeController;
    [SerializeField] private StageSO stageSO;

    [Header("InGame씬에서 바로 실행하기 (밑에 bool변수 true)")]
    public bool startGameDirectlyAtInGameScene;
    public BoardSO boardSO;

    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Three Star UI")]
    [SerializeField] private TextMeshProUGUI[] star321LimitText;
    private Color[] star321LimitTextColor;
    [SerializeField] private StarDropper[] star321Dropper;
    [SerializeField] private TextMeshProUGUI MoveCountText;
    [SerializeField] private Slider moveCountSlider;
    private const float Star3SliderValue = 0.522f, Star2SliderValue = 0.268f, Star1SliderValue = 0.018f, Star1SliderStopValue = 0.05f;
    private float starSpacing;

    [Header("Warning")]
    [SerializeField] private GameObject color12Warning;

    [Header("Movement Mode")]
    [SerializeField] private TileClickEvent tileTouchScript;
    [SerializeField] private JoyStickInputController joyStickScript;


    private int stage, level, star;

    private void Awake()
    {
        m_IsDestroyOnLoad = true;
        Init();
    }

    private void Start()
    {
        if (!startGameDirectlyAtInGameScene)
        {
            stage = PersistentDataManager.Instance.stage;
            level = PersistentDataManager.Instance.level;
            boardSO = PersistentDataManager.Instance.boardSO;
        }

        stageText.text = $"Stage{stage}-{level}";
        Board.Instance.InitBoard(boardSO);
        PlayerController.Instance.InitPlayer(boardSO);
        cameraSizeController.AdjustCameraSize(boardSO);

        star = 3;
 
        Invoke("InitializeUISetup", 0.05f);

        SetMovementMode();

        isGaming = true;

        if (stage == 1 && level == 1) // 튜토리얼
        {
            isGaming = false;
            GameObject tutorialCanvas = Resources.Load<GameObject>("Prefabs/TutorialCanvas1");
            Instantiate(tutorialCanvas);
        }
    }

    // 보라색 테두리 경고문구, 별 슬라이더의 별 위치 조정
    private void InitializeUISetup()
    {
        float canvasWidth = star321LimitText[0].GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
        starSpacing = (canvasWidth / 2f - 100) / 2f;
        Logger.Log($"canvasWidth : {canvasWidth}, starSpacing : {starSpacing}");

        float aspect = (float)Screen.height / Screen.width;
        float ratio = Mathf.Lerp(0.4f, 0.8f, Mathf.InverseLerp(0.8f, 2, aspect));
        color12Warning.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta =
            new Vector2(canvasWidth * ratio, canvasWidth * ratio / 1976 * 852);

        star321LimitText[0].text = boardSO.limitStepForThreeStar.ToString();
        star321LimitText[1].text = boardSO.limitStepForTwoStar.ToString();
        if (boardSO.limitStepForOneStar > 0) star321LimitText[2].text = boardSO.limitStepForOneStar.ToString();
        else star321LimitText[2].text = "∞";

        star321LimitTextColor = new Color[3];
        for (int i = 0; i < 3; i++)
        {
            star321LimitText[i].GetComponent<RectTransform>().anchoredPosition
                = new Vector2(starSpacing * i, star321LimitText[i].GetComponent<RectTransform>().anchoredPosition.y);
            star321Dropper[i].GetComponent<RectTransform>().anchoredPosition
                = new Vector2(starSpacing * i, star321Dropper[i].GetComponent<RectTransform>().anchoredPosition.y);
            star321LimitTextColor[i] = star321LimitText[i].color;
        }
    }

    private void InitStatus()
    {
        for (int i = 0; i < 3; i++)
        {
            star321LimitText[i].gameObject.SetActive(true);
            star321LimitText[i].color = star321LimitTextColor[i];
            star321Dropper[i].gameObject.SetActive(true);
            star321Dropper[i].transform.rotation = Quaternion.identity;
            star321Dropper[i].GetComponent<Image>().color = new Color(0, 0, 0, 1);
            star321Dropper[i].GetComponent<RectTransform>().anchoredPosition
                = new Vector2(starSpacing * i, -128.47f);
            star321Dropper[i].Init();
        }
        star = 3;
        tileTouchScript.Init();
        UpdateMoveCount(0);
    }

    public void UpdateMoveCount(int moveCount)
    {
        MoveCountText.text = moveCount.ToString();
        float sliderTargetValue;

        if (moveCount <= boardSO.limitStepForThreeStar)
        {
            sliderTargetValue = Mathf.Lerp(1, Star3SliderValue, (float)moveCount / boardSO.limitStepForThreeStar);
        }
        else if (moveCount <= boardSO.limitStepForTwoStar)
        {
            DropStar(3);
            float ratio = (float)(moveCount - boardSO.limitStepForThreeStar) / (boardSO.limitStepForTwoStar - boardSO.limitStepForThreeStar);
            sliderTargetValue = Mathf.Lerp(Star3SliderValue, Star2SliderValue, ratio);
        }
        else if (boardSO.limitStepForOneStar > 0)
        {
            DropStar(2);
            if (moveCount <= boardSO.limitStepForOneStar)
            {
                float ratio = (float)(moveCount - boardSO.limitStepForTwoStar) / (boardSO.limitStepForOneStar - boardSO.limitStepForTwoStar);
                sliderTargetValue = Mathf.Lerp(Star2SliderValue, Star1SliderValue, ratio);
            }
            else
            {
                DropStar(1);
                sliderTargetValue = 0;
                GameOver();
            }
        }
        else // infinite
        {
            DropStar(2);
            moveCount = Mathf.Min(moveCount, boardSO.limitStepForTwoStar + 150);
            float ratio = (moveCount - boardSO.limitStepForTwoStar) / 150f;
            ratio = Mathf.Sin(ratio * Mathf.PI / 2f);
            sliderTargetValue = Mathf.Lerp(Star2SliderValue, Star1SliderStopValue, ratio);
        }

        moveCountSlider.DOValue(sliderTargetValue, 0.3f).SetEase(Ease.OutQuad);
    }

    // 3/2/1별을 떨어트리면 n을 3/2/1을 입력
    private void DropStar(int n)
    {
        if (star < n) return;

        star = n - 1;
        n = 3 - n;
        if (star321Dropper[n].DropStar())
        {
            Color color = star321LimitTextColor[n];
            color.a = 0;
            star321LimitText[n].DOColor(color, 0.4f);
        }
    }

    public void GameOver()
    {
        Logger.Log("GAMEOVER!~~");
    }

    public void GameClear()
    {
        isGaming = false;
        gameClearObj.SetActive(true);
        PersistentDataManager.Instance.SetStageClearData(star);
        if (stage == 1 && level == 1) // 튜토리얼
        {
            GameObject tutorialCanvas = Resources.Load<GameObject>("Prefabs/TutorialCanvas2");
            Instantiate(tutorialCanvas);
            GameObject moveTutorialCanvas = GameObject.Find("MoveTutorialCanvas(Clone)");
            Destroy(moveTutorialCanvas);
        }
        Logger.Log("Game Clear");
    }

    public void Pause()
    {
        isGaming = false;
        UIManager.Instance.OpenMenu(true);
    }

    public void Resume()
    {
        isGaming = true;
        gameClearObj.SetActive(false);
    }

    public void Restart()
    {
        Board.Instance.InitBoardWhenRestart(boardSO);
        PlayerController.Instance.InitPlayer(boardSO);
        InitStatus();
        if (color12Warning.activeSelf)
        {
            color12Warning.GetComponent<RectTransform>().DOAnchorPosY(-310, rectDuration).SetEase(gogoEase)
                          .OnComplete(() => color12Warning.SetActive(false));
        }
        Resume();
    }

    public void SelectLevel()
    {
        UIManager.Instance.ScreenTransition(() =>
        {
            SceneManager.LoadScene("Main");
            UIManager.Instance.GoToChoiceLevelWhenComeToMainScene();
        });
    }

    public void GoToNextStage()
    {
        int nextStage = level == stageSO.numOfLevelOfStage[stage - 1] ? stage + 1 : stage;
        int nextLevel = level == stageSO.numOfLevelOfStage[stage - 1] ? 1 : level + 1;
        if (PersistentDataManager.Instance.LoadStageAndLevel(nextStage, nextLevel))
        {
            if (level == stageSO.numOfLevelOfStage[stage - 1])
                PlayerPrefs.SetInt("LastSelectedCard", stage);
            UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("InGame"));
        }
        else
        {
            Logger.Log($"Failed to go to Next Stage {stage}-{level}");
        }
    }

    public void SettingsExit()
    {
        Invoke("SetMovementMode", 0.02f);
    }

    private void SetMovementMode()
    {
        bool isTileTouch = PersistentDataManager.Instance.isTileTouch;
        tileTouchScript.enabled = isTileTouch;
        joyStickScript.enabled = !isTileTouch;
        if (!isTileTouch) joyStickScript.SetDelay();
    }

    public void Color12Warning()
    {
        if (color12Warning.activeSelf)
            return;

        color12Warning.SetActive(true);
        RectTransform rect = color12Warning.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, -310);
        rect.DOAnchorPosY(115, rectDuration).SetEase(gogoEase);
    }
    public float rectDuration = 0.7f;
    public Ease gogoEase;
}
