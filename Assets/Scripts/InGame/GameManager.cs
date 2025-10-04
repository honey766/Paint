using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehaviour<GameManager>
{
    public bool isGaming;
    public GameObject gameClearObj;
    public CameraSizeController cameraSizeController;

    [Header("InGame씬에서 바로 실행하기 (밑에 bool변수 true)")]
    public bool startGameDirectlyAtInGameScene;
    public BoardSO boardSO;

    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private GameObject menuUI;


    private int stage, board;

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
            board = PersistentDataManager.Instance.board;
            boardSO = Resources.Load<BoardSO>($"ScriptableObjects/Board/Stage{stage}/Stage{stage}-{board}");
        }

        stageText.text = $"Stage{stage}-{board}";
        Board.Instance.InitBoard(boardSO);
        PlayerController.Instance.InitPlayer(boardSO);
        cameraSizeController.AdjustCameraSize(boardSO);

        isGaming = true;
    }

    public void GameClear()
    {
        isGaming = false;
        gameClearObj.SetActive(true);
        Debug.Log("Game Clear");
    }


    public void Pause()
    {
        isGaming = false;
        menuUI.SetActive(true);
    }

    public void Resume()
    {
        isGaming = true;
        menuUI.SetActive(false);
        gameClearObj.SetActive(false);
    }

    public void Restart()
    {
        Board.Instance.InitBoardWhenRestart(boardSO);
        PlayerController.Instance.InitPlayer(boardSO);
        Resume();
    }

    public void ChoiceLevel()
    {
        UIManager.Instance.ScreenTransition(() =>
        {
            SceneManager.LoadScene("Main");
            UIManager.Instance.GoToChoiceLevelWhenComeToMainScene();
        });
    }


    public void OpenSetting()
    {
        UIManager.Instance.OpenSettings();
    }

    public void OpenShop()
    {

    }

    public void GoToMainMenu()
    {
        UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("Main"));
    }

    public void GoToNextStage()
    {
        if (board == 7) PlayerPrefs.SetInt("LastSelectedCard", stage);
        PersistentDataManager.Instance.stage = board == 7 ? stage + 1: stage;
        PersistentDataManager.Instance.board = board == 7 ? 1 : board + 1;
        UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("InGame"));
    }
}
