using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public bool isGaming;
    public GameObject gameClearText;
    public CameraSizeController cameraSizeController;

    [Header("InGame씬에서 바로 실행하기 (밑에 bool변수 true)")]
    public bool startGameDirectlyAtInGameScene;
    public BoardSO boardSO;

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
            boardSO = Resources.Load<BoardSO>($"ScriptableObjects/Board/Stage{stage}/Stage{stage}Board{board}");
        }

        Board.Instance.InitBoard(boardSO);
        PlayerController.Instance.InitPlayer(boardSO);
        cameraSizeController.AdjustCameraSize(boardSO);

        isGaming = true;
    }

    public void GameClear()
    {
        isGaming = false;
        gameClearText.SetActive(true);
        Debug.Log("Game Clear");
    }
}
