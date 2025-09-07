using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public BoardSO boardSO;
    public bool isGaming;
    public GameObject gameClearText;

    private void Start()
    {
        Board.Instance.InitBoard(boardSO);
        PlayerController.Instance.InitPlayer(boardSO);

        // 임시
        isGaming = true;
    }

    public void GameClear()
    {
        isGaming = false;
        gameClearText.SetActive(true);
        Debug.Log("Game Clear");
    }
}
