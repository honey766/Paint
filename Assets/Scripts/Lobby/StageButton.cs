using UnityEngine;

public class StageButton : MonoBehaviour
{
    public int stage, board;

    public void OnClick()
    {
        PersistentDataManager.Instance.stage = stage;
        PersistentDataManager.Instance.board = board;
        SceneLoader.Instance.LoadScene(SceneType.InGame);
    }
}
