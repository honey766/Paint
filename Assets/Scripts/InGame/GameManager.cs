using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using UnityEngine.Localization.Settings;

public class GameManager : SingletonBehaviour<GameManager>
{
    public bool isGaming;
    public CameraSizeController cameraSizeController;
    [SerializeField] private StageSO stageSO;

    [Header("GameClear, GameOver")]
    [SerializeField] private GameObject gameClearCanvasPrefab;
    private GameObject gameClearCanvasObj;

    [SerializeField] private GameObject gameOverCanvasPrefab;
    private GameObject gameOverCanvasObj;


    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialControllerPrefab;
    private TutorialController tutorialController;
    private bool isFirstTutorial;

    [Header("InGame씬에서 바로 실행하기 (밑에 bool변수 true)")]
    public bool startGameDirectlyAtInGameScene;
    public BoardSO boardSO;

    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Three Star UI")]
    [SerializeField] private TextMeshProUGUI[] star321LimitText;
    [SerializeField] private Image[] starImages;
    [SerializeField] private TextMeshProUGUI MoveCountText;
    [SerializeField] private Slider moveCountSlider;
    [SerializeField] private RectTransform starParticle;
    [SerializeField] private Vector2 starImageToParticleOffset;
    private ParticleSystem starParticleSystem;
    //[SerializeField] private float leftStarPosX = 0;
    //private const float Star3SliderValue = 0.522f, Star2SliderValue = 0.268f, Star1SliderValue = 0.018f, Star1SliderStopValue = 0.05f;
    //private const float Star3SliderValue = 0.582f, Star2SliderValue = 0.3f, Star1SliderValue = 0.018f, Star1SliderStopValue = 0.054f;
    //private float starSpacing, starPosY;

    [Header("Warning")]
    [SerializeField] private Image color12WarningBackground;
    private TextMeshProUGUI color12WarningText;
    private Image color12WarningRestartButton;
    private Color color12WarningTextColor = new Color(0.3803922f, 0.3921569f, 0.4f, 1f);

    [Header("Movement Mode")]
    [SerializeField] private TileClickEvent tileTouchScript;
    [SerializeField] private JoyStickInputController joyStickScript;

    [Header("Hint")]
    private GameObject hintObj;
    public List<BoardSO> boardSOs { get; private set; }
    public bool hintLoadTaskCompleted { get; private set; }

    private int stage, level, star;
    private float keyboardRestartNextFireTime;

    private void Awake()
    {
        m_IsDestroyOnLoad = true;
        Init();

        color12WarningText = color12WarningBackground.transform.parent.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        color12WarningRestartButton = color12WarningText.transform.parent.GetChild(1).GetChild(0).GetComponent<Image>();
        //starPosY = star321Dropper[0].GetComponent<RectTransform>().anchoredPosition.y;
        starParticleSystem = starParticle.transform.GetChild(0).GetComponent<ParticleSystem>();
        isFirstTutorial = true;
        LoadHintTasks();
    }

    public void Start()
    {
        isGaming = true;
        hintObj = null;
        
        #if !UNITY_EDITOR
        startGameDirectlyAtInGameScene = false;
        #endif

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
                tutorialController = Instantiate(tutorialControllerPrefab).GetComponent<TutorialController>();
                tutorialController.FirstTutorialEvent();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ControlEscapeKey();

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Time.time >= keyboardRestartNextFireTime)
            {
                keyboardRestartNextFireTime = Time.time + 0.1f;
                Restart();
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
            ShowHint();
    }

    private void ControlEscapeKey()
    {
        if (UIManager.Instance.doingTransition)
            return;

        // 메뉴 오픈하기
        if (isGaming)
        {
            Pause();
            return;
        }

        // 세팅 닫기
        Settings settings = FindAnyObjectByType<Settings>();
        if (settings != null)
        {
            settings.OnSettingExit();
            return;
        }

        // 상점 닫기
        GameObject shop = GameObject.Find("Shop(Clone)");
        if (shop != null)
        {
            AudioManager.Instance.PlaySfx(SfxType.Click1);
            Destroy(shop);
            return;
        }

        // 메뉴 닫기
        GameObject menu = GameObject.Find("GameMenuCanvas(Clone)");
        if (menu != null)
        {
            menu.GetComponent<Menu>().Resume();
            return;
        }

        // 힌트 닫기
        HintController hint = FindAnyObjectByType<HintController>();
        if (hint != null)
        {
            hint.OnCloseClick();
            return;
        }
    }

    private void OnDestroy()
    {
        UnloadHintTasks();
    }

    // 보라색 테두리 경고문구(주석처리됨), 별 슬라이더의 별 위치 조정
    private void InitializeUISetup()
    {
        // float canvasWidth = star321LimitText[0].GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
        // starSpacing = (canvasWidth / 2f - 100) / 2f;
        // Logger.Log($"canvasWidth : {canvasWidth}, starSpacing : {starSpacing}");

        // float aspect = (float)Screen.height / Screen.width;
        // float ratio = Mathf.Lerp(0.4f, 0.8f, Mathf.InverseLerp(0.8f, 2, aspect));
        // color12Warning.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta =
        //     new Vector2(canvasWidth * ratio, canvasWidth * ratio / 1976 * 852);

        star321LimitText[0].text = boardSO.limitStepForThreeStar.ToString();
        star321LimitText[1].text = boardSO.limitStepForTwoStar.ToString();
        if (boardSO.limitStepForOneStar > 0) star321LimitText[2].text = boardSO.limitStepForOneStar.ToString();
        else star321LimitText[2].text = "∞";

        // star321LimitTextColor = new Color[3];
        // for (int i = 0; i < 3; i++)
        // {
        //     star321LimitText[i].GetComponent<RectTransform>().anchoredPosition
        //         = new Vector2(leftStarPosX + starSpacing * i, star321LimitText[i].GetComponent<RectTransform>().anchoredPosition.y);
        //     star321Dropper[i].GetComponent<RectTransform>().anchoredPosition
        //         = new Vector2(leftStarPosX + starSpacing * i, star321Dropper[i].GetComponent<RectTransform>().anchoredPosition.y);
        //     star321LimitTextColor[i] = star321LimitText[i].color;
        // }
    }

    private void InitStatus()
    {
        for (int i = 0; i < 3; i++)
        {
            // star321LimitText[i].DOKill();
            // star321LimitText[i].gameObject.SetActive(true);
            // star321LimitText[i].color = star321LimitTextColor[i];
            // star321Dropper[i].CancelSequence();
            // star321Dropper[i].gameObject.SetActive(true);
            // star321Dropper[i].transform.rotation = Quaternion.identity;
            // star321Dropper[i].GetComponent<Image>().color = new Color(0, 0, 0, 1);
            // star321Dropper[i].GetComponent<RectTransform>().anchoredPosition
            //     = new Vector2(leftStarPosX + starSpacing * i, starPosY);
            // star321Dropper[i].Init();
            starImages[i].gameObject.SetActive(true);
        }
        star = 3;
        tileTouchScript.Init();
    }

    public void UpdateMoveCount(int moveCount)
    {
        MoveCountText.text = moveCount.ToString();
        // if (moveCount < 10) MoveCountText.GetComponent<RectTransform>().anchoredPosition = new Vector2(590, -2);
        // else MoveCountText.GetComponent<RectTransform>().anchoredPosition = new Vector2(630, -2);
        // float sliderTargetValue;

        if (moveCount <= boardSO.limitStepForThreeStar)
        {
            // sliderTargetValue = Mathf.Lerp(1, Star3SliderValue, (float)moveCount / boardSO.limitStepForThreeStar);
        }
        else if (moveCount <= boardSO.limitStepForTwoStar)
        {
            DropStar(3);
            // float ratio = (float)(moveCount - boardSO.limitStepForThreeStar) / (boardSO.limitStepForTwoStar - boardSO.limitStepForThreeStar);
            // sliderTargetValue = Mathf.Lerp(Star3SliderValue, Star2SliderValue, ratio);
        }
        else if (boardSO.limitStepForOneStar > 0)
        {
            DropStar(2);
            if (moveCount <= boardSO.limitStepForOneStar)
            {
                // float ratio = (float)(moveCount - boardSO.limitStepForTwoStar) / (boardSO.limitStepForOneStar - boardSO.limitStepForTwoStar);
                // sliderTargetValue = Mathf.Lerp(Star2SliderValue, Star1SliderValue, ratio);
            }
            else
            {
                DropStar(1);
                // sliderTargetValue = 0;
                GameOver();
            }
        }
        else // infinite
        {
            DropStar(2);
            // moveCount = Mathf.Min(moveCount, boardSO.limitStepForTwoStar + 150);
            // float ratio = (moveCount - boardSO.limitStepForTwoStar) / 150f;
            // ratio = Mathf.Sin(ratio * Mathf.PI / 2f);
            // sliderTargetValue = Mathf.Lerp(Star2SliderValue, Star1SliderStopValue, ratio);
        }

        // moveCountSlider.DOValue(sliderTargetValue, 0.3f).SetEase(Ease.OutQuad);
    }

    public void HideStar()
    {
        for (int i = 0; i < 3; i++)
            starImages[i].transform.parent.gameObject.SetActive(false);
    }
    public void ShowStarForTutorial()
    {
        Color color = starImages[0].color;
        for (int i = 0; i < 3; i++)
        {
            starImages[i].transform.parent.gameObject.SetActive(true);
            starImages[i].color = new Color(color.r, color.g, color.b, 0);
            starImages[i].DOColor(color, 0.8f);
        }
    }
    public void RedoStar(int moveCount)
    {
        Color color = starImages[0].color;
        int[] limitStep = new int[] { boardSO.limitStepForThreeStar, boardSO.limitStepForTwoStar, boardSO.limitStepForOneStar };
        for (int i = 2; i >= 0; i--)
        {
            if (limitStep[i] != -1 && moveCount > limitStep[i] || starImages[i].gameObject.activeSelf)
                continue;
            starImages[i].gameObject.SetActive(true);
            starImages[i].color = new Color(color.r, color.g, color.b, 0);
            starImages[i].DOColor(color, 0.8f);
            star = 3 - i;
        }
    }

    // 3/2/1별을 떨어트리면 n을 3/2/1을 입력
    private void DropStar(int n)
    {
        if (star < n) return;

        star = n - 1;
        n = 3 - n;
        // if (star321Dropper[n].DropStar())
        // {
        //     Color color = star321LimitTextColor[n];
        //     color.a = 0;
        //     star321LimitText[n].DOColor(color, 0.4f);
        // }
        starImages[n].gameObject.SetActive(false);
        //starParticle.anchoredPosition = starImages[n].transform.parent.GetComponent<RectTransform>().anchoredPosition + starImageToParticleOffset;
        //starParticleSystem.Emit(8);
        starImages[n].transform.parent.GetComponentInChildren<ParticleSystem>().Emit(9);
    }

    public void GameOver()
    {
        isGaming = false;
        gameOverCanvasObj = Instantiate(gameOverCanvasPrefab);
        gameOverCanvasObj.GetComponent<GameClearCanvas>().Init(0, "");
        Logger.Log("GAMEOVER!~~");
    }

    public async void GameClear()
    {
        if (stage == 1 && level == 1) // 튜토리얼
        {
            bool tutorialClear = await tutorialController.TutorialClearEvent(star);
            if (!tutorialClear)
                return;
        }
        isGaming = false;

        string key = "";
        if (level == stageSO.numOfLevelOfStage[stage - 1] || level == -stageSO.numOfLevelOfExtraStage[stage - 1])
            key = "NextStage";
        else if (level < 0)
            key = "NextEXLevel";
        else
            key = "NextLevel";
        string goToNextLevelText = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", key, LocalizationSettings.SelectedLocale);

        // 혹시 모를 중복을 방지
        GameClearCanvas curGameClearObj = FindAnyObjectByType<GameClearCanvas>();
        if (curGameClearObj == null)
        {
            gameClearCanvasObj = Instantiate(gameClearCanvasPrefab);
            gameClearCanvasObj.GetComponent<GameClearCanvas>().Init(star, goToNextLevelText);
        }

        PersistentDataManager.Instance.SetStageClearData(star);
        
        Logger.Log("Game Clear");
    }

    public void Pause()
    {
        isGaming = false;
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        UIManager.Instance.OpenMenu(true);
    }

    public void Resume()
    {
        isGaming = true;
        gameClearCanvasObj = null;
        gameOverCanvasObj = null;
    }

    public void Restart()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        Board.Instance.InitBoardWhenRestart(boardSO);
        PlayerController.Instance.InitPlayer(boardSO);
        InitStatus();
        if (color12WarningBackground.gameObject.activeSelf)
            SetColor12Warning(false);
        TutorialController t = FindAnyObjectByType<TutorialController>();
        if (t != null) t.RestartWhenFirstTutorial();
        Resume();
    }

    public void ShowHint()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        isGaming = false;

        if (stage == 1 && level == 1)
        {
            tutorialController.ShowHint();
            return;
        }
        if (hintObj == null)
        {
            hintObj = Resources.Load<GameObject>("Prefabs/Hint");
            hintObj = Instantiate(hintObj);
        }
        else if (!hintObj.activeSelf)
        {
            hintObj.SetActive(true);
        }
    }

    public void SelectLevel()
    {
        UIManager.Instance.ScreenTransition(() =>
        {
            SceneManager.LoadScene("Main");
            UIManager.Instance.GoToChoiceLevelWhenComeToMainScene();
        });
    }

    public async void GoToNextLevel()
    {
        if (level == stageSO.numOfLevelOfStage[stage - 1] || level == -stageSO.numOfLevelOfExtraStage[stage - 1])
        {
            if (stageSO.numOfStage == stage)// || PersistentDataManager.Instance.totalStar < stageSO.numOfStarToUnlockStage[stage])
            {
                AudioManager.Instance.ChangeBgmWithTransition(BgmType.Title);
                SelectLevel();
                return;
            }
        }

        int nextStage, nextLevel;
        // 다음 레벨 계산
        if (level > 0) // 일반 레벨일 때
        {
            nextStage = level == stageSO.numOfLevelOfStage[stage - 1] ? stage + 1 : stage;
            nextLevel = level == stageSO.numOfLevelOfStage[stage - 1] ? 1 : level + 1;
        }
        else // 엑스트라 레벨일 때
        {
            nextStage = level == -stageSO.numOfLevelOfExtraStage[stage - 1] ? stage + 1 : stage;
            nextLevel = level == -stageSO.numOfLevelOfExtraStage[stage - 1] ? 1 : level - 1;
        }



        // bool success = await PersistentDataManager.Instance.LoadStageAndLevelAsync(nextStage, nextLevel);
        // if (success)
        // {
        //     // 1-1 => 1-2도 bgm 바뀜
        //     AudioManager.Instance.ChangeBgmWithTransition(nextStage);
        //     if (level == stageSO.numOfLevelOfStage[stage - 1] || level == -stageSO.numOfLevelOfExtraStage[stage - 1])
        //     {
        //         PlayerPrefs.SetInt("LastSelectedCardHorizontal", stage);
        //         PlayerPrefs.SetInt("LastSelectedCardVertical", 0);
        //     }
        //     UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("InGame"));
        // }
        // else
        // {
        //     Logger.Log($"Failed to go to Next Stage {stage} - {level}");
        // }

        Logger.Log($"AAAAAAAA{nextStage}, {nextLevel}");
        Task<bool> loadingTask = PersistentDataManager.Instance.LoadStageAndLevelAsync(nextStage, nextLevel);
        Task[] tasksToWait = new Task[] { loadingTask };

        Action conditionalSceneLoadAction = async () =>
        {
            // 로딩 Task의 최종 결과(bool)를 비동기적으로 기다림
            bool success = await loadingTask;

            if (success)
            {
                // 성공: InGame 씬으로 이동
                Logger.Log($"Going To Stage {nextStage} - {nextLevel}");
                AudioManager.Instance.ChangeBgmWithTransition(nextStage);
                if (level == stageSO.numOfLevelOfStage[stage - 1] || level == -stageSO.numOfLevelOfExtraStage[stage - 1])
                {
                    PlayerPrefs.SetInt("LastSelectedCardHorizontal", stage);
                    PlayerPrefs.SetInt("LastSelectedCardVertical", 0);
                }
                SceneManager.LoadScene("InGame");
            }
            else
            {
                // 실패: Title 씬으로 이동
                Logger.LogError("에셋 로드 실패! 타이틀 화면으로 복귀합니다.");
                SceneManager.LoadScene("Title");
            }
        };

        // 4. UIManager 코루틴 시작
        // action 인자에 조건부 로직을 담은 함수를, tasks 인자에 로딩 Task를 넘깁니다.
        UIManager.Instance.ScreenTransition(
            conditionalSceneLoadAction, 
            tasksToWait
        );
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
            color12WarningParent.SetActive(true);
            color12WarningBackground.color = new Color(1, 1, 1, 0);
            color12WarningTextColor.a = 0;
            color12WarningText.color = color12WarningTextColor;
            color12WarningRestartButton.color = new Color(1, 1, 1, 0);
            color12WarningBackground.DOColor(new Color(1, 1, 1, 0.5f), 0.6f);
            color12WarningTextColor.a = 1;
            color12WarningText.DOColor(color12WarningTextColor, 0.6f);
            color12WarningRestartButton.DOColor(new Color(1, 1, 1, 0.92f), 0.6f);
        }
        else
        {
            color12WarningBackground.DOColor(new Color(1, 1, 1, 0), 0.3f);
            color12WarningTextColor.a = 0;
            color12WarningText.DOColor(color12WarningTextColor, 0.3f);
            color12WarningRestartButton.DOColor(new Color(1, 1, 1, 0), 0.3f).OnComplete(
                () => color12WarningBackground.transform.parent.gameObject.SetActive(false));
        }
    }

    public void Color12Warning(bool isActive)
    {
        if (isActive == color12WarningBackground.transform.parent.gameObject.activeSelf)
            return;
        SetColor12Warning(isActive);
    }

    private async void LoadHintTasks()
    {
        hintLoadTaskCompleted = false;

        bool isExtra = PersistentDataManager.Instance.level < 0;
        int stage = PersistentDataManager.Instance.stage;
        int level = Mathf.Abs(PersistentDataManager.Instance.level);
        boardSOs = new();
        int num = 1;

        while (true)
        {
            string boardName = isExtra ? "Extra" : "";
            boardName += $"Hint{stage}-{level}" + (num == 1 ? "" : $"-{num}");
            string address = $"Assets/ScriptableObjects/Hint/Stage{stage}/{boardName}.asset";

            var locations = Addressables.LoadResourceLocationsAsync(address);
            await locations.Task;

            // 키가 없으면 종료
            if (locations.Status != AsyncOperationStatus.Succeeded || locations.Result.Count == 0)
            {
                Logger.Log($"✅ 힌트 로드 완료. (총 {boardSOs.Count}개, 마지막 확인 키: {address})");
                Addressables.Release(locations);
                break;
            }

            Addressables.Release(locations);

            try
            {
                AsyncOperationHandle<BoardSO> newHandle = Addressables.LoadAssetAsync<BoardSO>(address);
                await newHandle.Task; // 로드가 완료될 때까지 대기

                // 비동기 작업이 성공했는지 확인
                if (newHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    BoardSO boardSO = newHandle.Result;

                    if (boardSO == null)
                    {
                        Debug.LogWarning($"에셋은 찾았으나 내용(BoardSO)이 null입니다: {address}");
                        Addressables.Release(newHandle);
                        break; 
                    }

                    boardSOs.Add(boardSO);
                    num++;
                    // 다음 반복을 위해 핸들을 해제하지 않음 (로드가 성공했으므로 계속 참조)
                }
                else
                {
                    // Status가 Failed지만 InvalidKeyException으로 잡히지 않은 다른 종류의 실패 (예: 파일 손상 등)
                    Logger.Log($"로드 상태 실패 (Status: {newHandle.Status}): {address}");
                    Addressables.Release(newHandle);
                    break;
                }
            }
            catch (UnityEngine.AddressableAssets.InvalidKeyException)
            {
                // 🎯 이전에 찾던 에셋(키)이 Addressables Catalog에 없음을 확인
                Logger.Log($"✅ 힌트 로드 완료. (마지막 키 없음: {address})");
                break; // 찾으려는 연속된 에셋이 없으므로 루프 종료
            }
            catch (System.Exception ex)
            {
                // InvalidKeyException 외의 다른 예상치 못한 오류 처리
                Logger.LogError($"에셋 로드 중 예상치 못한 오류 발생 ({address}): {ex.Message}");
                break;
            }
        }

        hintLoadTaskCompleted = true;
    }

    private void UnloadHintTasks()
    {
        if (boardSOs == null) return;

        foreach (BoardSO boardSO in boardSOs)
            Addressables.Release(boardSO);
        boardSOs.Clear();
    }
}