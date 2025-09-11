using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public BoardSO boardSO;
    public bool isGaming;
    public GameObject gameClearText;
    public CameraSizeController cameraSizeController;

    private int stage, board;

    private void Awake()
    {
        m_IsDestroyOnLoad = true;
        Init();
    }

    private void Start()
    {
        stage = PersistentDataManager.Instance.stage;
        board = PersistentDataManager.Instance.board;
        boardSO = Resources.Load<BoardSO>($"ScriptableObjects/Board/Stage{stage}/Stage{stage}Board{board}");

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
