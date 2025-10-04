using UnityEngine;

public class PersistentDataManager : SingletonBehaviour<PersistentDataManager>
{
    public int stage { get; private set; }
    public int level { get; private set; }
    public BoardSO boardSO { get; private set; }
    public ColorPaletteSO colorPaletteSO;

    // stage, level이 존재하면 true
    public bool LoadStageAndLevel(int stage, int level)
    {
        boardSO = Resources.Load<BoardSO>($"ScriptableObjects/Board/Stage{stage}/Stage{stage}-{level}");
        if (boardSO != null)
        {
            this.stage = stage;
            this.level = level;
        }
        return boardSO != null;
    }
}
