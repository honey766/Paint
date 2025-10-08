using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : SingletonBehaviour<GameManager>
{
    public bool isGaming;
    public CameraSizeController cameraSizeController;
    [SerializeField] private StageSO stageSO;

    [Header("GameClear, GameOver")]
    public GameObject gameClearObj;
    [SerializeField] private TextMeshProUGUI goToNextLevelText; 
    [SerializeField] private GameObject goToExtraObj;
    [SerializeField] private GameObject gameOverObj;

    [Header("Tutorial")]
    [SerializeField] private GameObject TutorialController;
    private bool isFirstTutorial;

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
    [SerializeField] private float leftStarPosX;
    //private const float Star3SliderValue = 0.522f, Star2SliderValue = 0.268f, Star1SliderValue = 0.018f, Star1SliderStopValue = 0.05f;
    private const float Star3SliderValue = 0.582f, Star2SliderValue = 0.3f, Star1SliderValue = 0.018f, Star1SliderStopValue = 0.054f;
    private float starSpacing, starPosY;

    [Header("Warning")]
    [SerializeField] private Image color12WarningBackground;
    private TextMeshProUGUI color12WarningText;
    private Image color12WarningRestartButton;

    [Header("Movement Mode")]
    [SerializeField] private TileClickEvent tileTouchScript;
    [SerializeField] private JoyStickInputController joyStickScript;

    private int stage, level, star;

    private void Awake()
    {
        m_IsDestroyOnLoad = true;
        Init();

        color12WarningText = color12WarningBackground.transform.parent.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        color12WarningRestartButton = color12WarningText.transform.parent.GetChild(1).GetChild(0).GetComponent<Image>();
        starPosY = star321Dropper[0].GetComponent<RectTransform>().anchoredPosition.y;
        isFirstTutorial = true;
    }

    public void Start()
    {
        isGaming = true;

        if (!startGameDirectlyAtInGameScene)
        {
            stage = PersistentDataManager.Instance.stage;
            level = PersistentDataManager.Instance.level;
            boardSO = PersistentDataManager.Instance.boardSO;
        }

        stageText.text = (level < 0 ? "<color=#550000>Ex " : "") + $"Level{stage}-{Mathf.Abs(level)}";
        Board.Instance.InitBoard(boardSO);
        bool isTutorial = stage == 1 && level == 1;
        PlayerController.Instance.InitPlayer(boardSO, isTutorial);
        cameraSizeController.AdjustCameraSize(boardSO, isTutorial);

        star = 3;
 
        Invoke("InitializeUISetup", 0.05f);

        SetMovementMode();

        if (isTutorial)
        {
            stageText.text = "Tutorial";
            if (isFirstTutorial)
            {
                isFirstTutorial = false;
                TutorialController = Instantiate(TutorialController);
                TutorialController.GetComponent<TutorialController>().FirstTutorialEvent();
            }
        }
    }

    // 보라색 테두리 경고문구(주석처리됨), 별 슬라이더의 별 위치 조정
    private void InitializeUISetup()
    {
        float canvasWidth = star321LimitText[0].GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
        starSpacing = (canvasWidth / 2f - 100) / 2f;
        Logger.Log($"canvasWidth : {canvasWidth}, starSpacing : {starSpacing}");

        float aspect = (float)Screen.height / Screen.width;
        float ratio = Mathf.Lerp(0.4f, 0.8f, Mathf.InverseLerp(0.8f, 2, aspect));
        // color12Warning.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta =
        //     new Vector2(canvasWidth * ratio, canvasWidth * ratio / 1976 * 852);

        star321LimitText[0].text = boardSO.limitStepForThreeStar.ToString();
        star321LimitText[1].text = boardSO.limitStepForTwoStar.ToString();
        if (boardSO.limitStepForOneStar > 0) star321LimitText[2].text = boardSO.limitStepForOneStar.ToString();
        else star321LimitText[2].text = "∞";

        star321LimitTextColor = new Color[3];
        for (int i = 0; i < 3; i++)
        {
            star321LimitText[i].GetComponent<RectTransform>().anchoredPosition
                = new Vector2(leftStarPosX + starSpacing * i, star321LimitText[i].GetComponent<RectTransform>().anchoredPosition.y);
            star321Dropper[i].GetComponent<RectTransform>().anchoredPosition
                = new Vector2(leftStarPosX + starSpacing * i, star321Dropper[i].GetComponent<RectTransform>().anchoredPosition.y);
            star321LimitTextColor[i] = star321LimitText[i].color;
        }
    }

    private void InitStatus()
    {
        for (int i = 0; i < 3; i++)
        {
            star321LimitText[i].DOKill();
            star321LimitText[i].gameObject.SetActive(true);
            star321LimitText[i].color = star321LimitTextColor[i];
            star321Dropper[i].CancelSequence();
            star321Dropper[i].gameObject.SetActive(true);
            star321Dropper[i].transform.rotation = Quaternion.identity;
            star321Dropper[i].GetComponent<Image>().color = new Color(0, 0, 0, 1);
            star321Dropper[i].GetComponent<RectTransform>().anchoredPosition
                = new Vector2(leftStarPosX + starSpacing * i, starPosY);
            star321Dropper[i].Init();
        }
        star = 3;
        tileTouchScript.Init();
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
        isGaming = false;
        gameOverObj.SetActive(true);
        Logger.Log("GAMEOVER!~~");
    }

    public void GameClear()
    {
        if (stage == 1 && level == 1) // 튜토리얼
        {
            TutorialController.GetComponent<TutorialController>().TutorialClearEvent(star);
            return;
        }
        isGaming = false;
        gameClearObj.SetActive(true);
        PersistentDataManager.Instance.SetStageClearData(star);

        if (level == stageSO.numOfLevelOfStage[stage - 1] || level == -stageSO.numOfExtraLevelOfStage[stage - 1])
            goToNextLevelText.text = "다음 스테이지";
        else if (level < 0)
            goToNextLevelText.text = "다음 Ex단계";
        else
            goToNextLevelText.text = "다음 단계";
        goToExtraObj.SetActive(level == stageSO.numOfLevelOfStage[stage - 1]
            && PersistentDataManager.Instance.GetStageTotalStarData(stage) >= stageSO.numOfLevelOfStage[stage - 1] * 3);
        
        Logger.Log("Game Clear");
    }

#region Tutorial
    // private void TutorialClearEvent()
    // {
    //     if (tutorialLevel == 1)
    //     {
    //         isGaming = false;
    //         tutorialLevel++;
    //         PersistentDataManager.Instance.LoadTutorialLevel(2);
    //         GameObject tutorialCanvas = Resources.Load<GameObject>("Prefabs/TutorialCanvas2");
    //         Instantiate(tutorialCanvas);
    //         moveTutorialCanvasObj = GameObject.Find("MoveTutorialCanvas(Clone)");
    //         moveTutorialCanvasObj.SetActive(false);
    //         tutorialArrow.SetActive(false);
    //         Start();
    //         color12Border.enabled = false;
    //         isGaming = false;
    //     }
    //     else if (tutorialLevel == 2)
    //     {
    //         tutorialLevel++;
    //         PersistentDataManager.Instance.LoadTutorialLevel(3);
    //         Start();
    //         ThirdTutorialEvent();
    //     }
    //     else if (tutorialLevel == 3)
    //     {
    //         GameObject tutorialCanvas = Resources.Load<GameObject>("Prefabs/TutorialCanvas3");
    //         Instantiate(tutorialCanvas);
    //         GameObject moveTutorialCanvas = GameObject.Find("MoveTutorialCanvas(Clone)");
    //         Destroy(moveTutorialCanvas);
    //         Destroy(tutorialBorderTooltip);
    //         Destroy(tutorialAnswerButtonObj);
    //         isGaming = false;
    //         PersistentDataManager.Instance.SetStageClearData(star);
    //     }
    // }
    // private void FirstTutorialEvent()
    // {
    //     isGaming = false;
    //     tutorialLevel = 1;
    //     GameObject tutorialCanvas = Resources.Load<GameObject>("Prefabs/TutorialCanvas1");
    //     Instantiate(tutorialCanvas);
    // }
    // int firstTutorialArrowStatus = 0;
    // public void FirstTutorialImageCloseEvent()
    // {
    //     tutorialArrow = Instantiate(tutorialArrow);
    //     tutorialArrow.transform.position = new Vector2(-2, 0);
    //     PlayerController.Instance.MoveEvent += (pos) =>
    //     {
    //         // TODO
    //     };
    // }
    // public void SecondTutorialEvent()
    // {
    //     moveTutorialCanvasObj.SetActive(true);
    //     tutorialColor12BorderTempObj = Instantiate(tutorialColor12Border);
    //     tutorialColor12BorderTempObj.transform.position = Board.Instance.GetTilePos(2, 1);
    //     tutorialArrow = Instantiate(tutorialArrow);
    //     tutorialArrow.transform.position = Board.Instance.GetTilePos(2, 0);
    //     tutorialBorderTooltip = Instantiate(tutorialBorderTooltip);
    //     GameObject tutorialAnswerButton = Resources.Load<GameObject>("Prefabs/TutorialAnswerButton");
    //     tutorialAnswerButtonObj = Instantiate(tutorialAnswerButton);
    //     tutorialAnswerButtonObj.SetActive(false);
    //     tutorialIsOpenedAnswerButton = false;
    // }
    // private void ThirdTutorialEvent()
    // {
    //     Destroy(tutorialColor12BorderTempObj);
    //     Invoke("ThirdTutorialEventAfterSeconds", 0.1f);
    //     tutorialArrow.transform.position = Board.Instance.GetTilePos(1, -1);
    //     //TutorialAnswerButton tutorialAnswerButton = FindAnyObjectByType<TutorialAnswerButton>();
    //     tutorialAnswerButtonObj.SetActive(true);
    //     tutorialAnswerButtonObj.GetComponent<TutorialAnswerButton>().OnTutorial3();
    //     tutorialAnswerButtonObj.SetActive(false);
    //     tutorialIsOpenedAnswerButton = false;
    // }
    // private void ThirdTutorialEventAfterSeconds()
    // {  
    //     tutorialColor12BorderTempObj = Instantiate(tutorialColor12Border);
    //     tutorialColor12Border.transform.position = Board.Instance.GetTilePos(1, 0);
    // }
#endregion

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
        if (color12WarningBackground.gameObject.activeSelf)
            SetColor12Warning(false);
        if (gameOverObj.activeSelf) gameOverObj.SetActive(false);
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
        if (level == stageSO.numOfLevelOfStage[stage - 1] || level == -stageSO.numOfExtraLevelOfStage[stage - 1])
        {
            if (stageSO.numOfStage == stage || PersistentDataManager.Instance.totalStar < stageSO.numOfStarToUnlockStage[stage])
            {
                SelectLevel();
                return;
            }
        }

        int nextStage, nextLevel;
        if (level > 0)
        {
            nextStage = level == stageSO.numOfLevelOfStage[stage - 1] ? stage + 1 : stage;
            nextLevel = level == stageSO.numOfLevelOfStage[stage - 1] ? 1 : level + 1;
        }
        else
        {
            nextStage = level == -stageSO.numOfExtraLevelOfStage[stage - 1] ? stage + 1 : stage;
            nextLevel = level == -stageSO.numOfExtraLevelOfStage[stage - 1] ? 1 : level - 1;
        }
        if (PersistentDataManager.Instance.LoadStageAndLevel(nextStage, nextLevel))
        {
            if (level == stageSO.numOfLevelOfStage[stage - 1] || level == -stageSO.numOfExtraLevelOfStage[stage - 1])
                PlayerPrefs.SetInt("LastSelectedCard", stage);
            UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("InGame"));
        }
        else
        {
            Logger.Log($"Failed to go to Next Stage {stage} - {level}");
        }
    }

    public void GoToExtraStage()
    {
        if (PersistentDataManager.Instance.LoadStageAndLevel(stage, -1))
        {
            UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("InGame"));
        }
        else
        {
            Logger.Log($"Failed to go to Extra Stage {stage}-{1}");
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

    private void SetColor12Warning(bool isAppear)
    {
        if (isAppear)
        {
            GameObject color12WarningParent = color12WarningBackground.transform.parent.gameObject;
            if (stage == 1 && level == 1)
            {
                RectTransform rect = color12WarningParent.GetComponent<RectTransform>();

                // 가로: stretch (0~1)
                rect.anchorMin = new Vector2(0, 0.5f);  // 왼쪽
                rect.anchorMax = new Vector2(1, 0.5f);  // 오른쪽
                rect.pivot = new Vector2(0.5f, 0.5f);

                rect.anchoredPosition = new Vector2(0, 0);

                TutorialController.GetComponent<TutorialController>().ShowTutorialAnswerButton();
            }
            color12WarningParent.SetActive(true);
            color12WarningBackground.color = new Color(1, 1, 1, 0);
            color12WarningText.color = new Color(0, 0, 0, 0);
            color12WarningRestartButton.color = new Color(1, 1, 1, 0);
            color12WarningBackground.DOColor(new Color(1, 1, 1, 0.5f), 0.6f);
            color12WarningText.DOColor(Color.black, 0.6f);
            color12WarningRestartButton.DOColor(Color.white, 0.6f);
        }
        else
        {
            color12WarningBackground.transform.parent.gameObject.SetActive(false);
        }
    }

    public void Color12Warning()
    {
        if (color12WarningBackground.transform.parent.gameObject.activeSelf)
            return;

        SetColor12Warning(true);
        // RectTransform rect = color12Warning.GetComponent<RectTransform>();
        // rect.anchoredPosition = new Vector2(0, -310);
        // rect.DOAnchorPosY(115, rectDuration).SetEase(gogoEase);
    }
    // public float rectDuration = 0.7f;
    // public Ease gogoEase = Ease.OutQuad;
}
