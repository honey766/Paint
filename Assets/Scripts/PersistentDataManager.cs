using UnityEngine;
using System.Collections.Generic;
using System;

public class PersistentDataManager : SingletonBehaviour<PersistentDataManager>
{
    public int stage { get; private set; }
    public int level { get; private set; } // 음수면 extra레벨
    public StageSO stageSO;
    public BoardSO boardSO { get; private set; }
    public ColorPaletteSO colorPaletteSO;
    public bool isTileTouch { get; private set; }
    public float moveLatencyRate { get; private set; }

    public int totalStar { get; private set; }
    private int[,] stageClearData; // [stage][level] : 0~3 (별 획득 개수)
    private int[,] extraStageClearData;
    private int[] stageTotalStarData; // 각 stage에서 획득한 총 별의 개수 (extra 미포함)
    private int[] extraStageTotalStarData;
    private List<int>[] stagePlayerPrefsData; // PlayerPrefs.GetInt 결과를 캐싱
    private List<int>[] extraStagePlayerPrefsData;

    private void Awake()
    {
        Init();
        LoadStageClearData();
        isTileTouch = PlayerPrefs.GetInt("isTileTouch", 1) == 1;
        moveLatencyRate = PlayerPrefs.GetInt("moveLatencyRate", 70) / 100f;
    }

    public void SaveSettings(bool isTileTouch, float moveLatencyRate)
    {
        this.isTileTouch = isTileTouch;
        this.moveLatencyRate = moveLatencyRate;
    }

    public void LoadTutorialLevel(int level)
    {
        boardSO = Resources.Load<BoardSO>($"ScriptableObjects/Board/Stage1/Stage1-1-{level}");
    }

    // stage, level이 존재하면 true
    public bool LoadStageAndLevel(int stage, int level)
    {
        string name;
        if (level > 0) name = $"Stage{stage}-{level}";
        else name = $"ExtraStage{stage}-{-level}";

        boardSO = Resources.Load<BoardSO>($"ScriptableObjects/Board/Stage{stage}/" + name);
        if (boardSO != null)
        {
            this.stage = stage;
            this.level = level;
        }
        return boardSO != null;
    }

    #region StageData
    // 별 개수 데이터는 PlayerPrefs에 "Stage{stage(1~n)}ClearData{1~m}"이름으로 int형식으로 저장된다.
    // 32비트이므로 2비트씩 끊어서 16레벨의 별 개수를 저장할 수 있다.
    // 2스테이지 15레벨의 별 개수는 "Stage2ClearData1의 29,30비트에 저장되며
    // 3스테이지 18레벨의 별 개수는 "Stage3ClearData2의 3,4비트에 저장된다. 
    // (1~16레벨 데이터는 Stage3ClearData1, 17~32레벨 데이터는 Stage3ClearData2에 저장)
    // Extra 데이터는 PlayerPrefs에 "ExtraStage{stage(1~n)}ClearData{1~m}"이름으로 int형식으로 저장된다.

    private void LoadStageClearData()
    {
        stageSO = Resources.Load<StageSO>("ScriptableObjects/Stage/Stage");
        int stageNum = stageSO.numOfStage;
        int maxLevelNum = -1;
        int maxExtraLevelNum = -1;
        for (int i = 0; i < stageNum; i++)
        {
            maxLevelNum = Mathf.Max(maxLevelNum, stageSO.numOfLevelOfStage[i]);
            maxExtraLevelNum = Mathf.Max(maxExtraLevelNum, stageSO.numOfExtraLevelOfStage[i]);
        }

        stageClearData = new int[stageNum, maxLevelNum];
        stageTotalStarData = new int[stageNum];
        stagePlayerPrefsData = new List<int>[stageNum];
        extraStageClearData = new int[stageNum, maxExtraLevelNum];
        extraStageTotalStarData = new int[stageNum];
        extraStagePlayerPrefsData = new List<int>[stageNum];
        totalStar = 0;

        for (int i = 0; i < stageNum; i++)
        {
            stagePlayerPrefsData[i] = new List<int>();
            extraStagePlayerPrefsData[i] = new List<int>();
            LoadCertainStageClearData(stagePlayerPrefsData[i], stageSO.numOfLevelOfStage[i], i, false);
            LoadCertainStageClearData(extraStagePlayerPrefsData[i], stageSO.numOfExtraLevelOfStage[i], i, true);
            totalStar += stageTotalStarData[i] + extraStageTotalStarData[i];
        }

        for (int i = 0; i < stageNum; i++)
            for (int j = 0; j < stageSO.numOfLevelOfStage[i]; j++)
                Logger.Log($"stage{i + 1}-{j + 1}:{stageClearData[i, j]}");
        for (int i = 0; i < stageNum; i++)
            for (int j = 0; j < stageSO.numOfExtraLevelOfStage[i]; j++)
                Logger.Log($"extraStage{i + 1}-{j + 1}:{extraStageClearData[i, j]}");
        for (int i = 0; i < stageNum; i++)
        {
            Logger.Log($"stage{i + 1}, star{stageTotalStarData[i]}, extraStar{extraStageTotalStarData[i]},\n" +
            $"data: {Convert.ToString(stagePlayerPrefsData[i][0], 2).PadLeft(32, '0')}");
        }
    }
    private void LoadCertainStageClearData(List<int> prefsData, int numOfLevel, int i, bool isExtra)
    {
        int curLevel = 0;
        int count = 0;
        while (curLevel < numOfLevel)
        {
            count++;
            int data = PlayerPrefs.GetInt((isExtra ? "Extra" : "") + $"Stage{i + 1}ClearData{count}", 0);
            prefsData.Add(data);
            for (int j = 0; j < 16 && curLevel < numOfLevel; j++)
            {
                if (isExtra)
                {
                    extraStageClearData[i, curLevel] = (data >> (2 * j)) & 3;
                    extraStageTotalStarData[i] += extraStageClearData[i, curLevel];
                }
                else
                {
                    stageClearData[i, curLevel] = (data >> (2 * j)) & 3;
                    stageTotalStarData[i] += stageClearData[i, curLevel];
                }
                curLevel++;
            }
        }
    }

    public int GetStageClearData(int stage, int level) => stageClearData[stage - 1, level - 1];
    public int GetExtraStageClearData(int stage, int level) => extraStageClearData[stage - 1, Mathf.Abs(level) - 1];
    public int GetStageTotalStarData(int stage) => stageTotalStarData[stage - 1];
    public int GetExtraStageTotalStarData(int stage) => extraStageTotalStarData[stage - 1];

    public void SetStageClearData(int star) => SetStageClearData(stage, level, star);
    public void SetStageClearData(int stage, int level, int star)
    {
        Logger.Log($"update star - stage:{stage},level{level},star{star}");
        bool isExtra = level < 0;
        level = Mathf.Abs(level);
        stage--; level--;

        if (isExtra && extraStageClearData[stage, level] < star)
        {
            Logger.Log($"extra, star : {extraStageClearData[stage, level]} -> {star}");
            extraStageTotalStarData[stage] += star - extraStageClearData[stage, level];
            totalStar += star - extraStageClearData[stage, level];
            extraStageClearData[stage, level] = star;
            int data = extraStagePlayerPrefsData[stage][level / 16];
            data = data & ~(3 << 2 * (level % 16)) | (star << 2 * (level % 16));
            extraStagePlayerPrefsData[stage][level / 16] = data;
            PlayerPrefs.SetInt($"ExtraStage{stage + 1}ClearData{1 + level / 16}", data);
        }
        else if (!isExtra && stageClearData[stage, level] < star)
        {
            Logger.Log($"not extra, star : {stageClearData[stage, level]} -> {star}");
            stageTotalStarData[stage] += star - stageClearData[stage, level];
            totalStar += star - stageClearData[stage, level];
            stageClearData[stage, level] = star;
            int data = stagePlayerPrefsData[stage][level / 16];
            data = data & ~(3 << 2 * (level % 16)) | (star << 2 * (level % 16));
            stagePlayerPrefsData[stage][level / 16] = data;
            PlayerPrefs.SetInt($"Stage{stage + 1}ClearData{1 + level / 16}", data);
        }
    }

    #endregion
}
