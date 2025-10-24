using UnityEngine;

public class PlayerPrefsManager : SingletonBehaviour<PlayerPrefsManager>
{
    public int[,] stageClearData; // [stage][level] : 0~3 (별 획득 개수)

    private void Awake()
    {
        Init();
        LoadStageClearData();
    }

    private void LoadStageClearData()
    {
        StageSO stageSO = Resources.Load<StageSO>("ScriptableObjects/Stage/Stage");
        int stageNum = stageSO.numOfLevelOfStage.Length;
        int maxLevelNum = -1;
        for (int i = 0; i < stageNum; i++)
            maxLevelNum = Mathf.Max(maxLevelNum, stageSO.numOfLevelOfStage[i]);
        stageClearData = new int[stageNum, maxLevelNum];
    }
}