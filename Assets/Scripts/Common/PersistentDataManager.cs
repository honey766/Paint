using UnityEngine;
using System.Collections.Generic;
using System;

public class PersistentDataManager : SingletonBehaviour<PersistentDataManager>
{
    public int numOfStage { get; private set; }

    [Header("Ingame Data")]
    public int stage { get; private set; }
    public int level { get; private set; } // 음수면 extra레벨
    public StageSO stageSO;
    public BoardSO boardSO { get; private set; }
    public ColorPaletteSO colorPaletteSO;

    [Header("Stage Data")]
    public int totalStar { get; private set; }
    private int[,] stageClearData; // [stage][level] : 0~3 (별 획득 개수)
    private int[,] extraStageClearData;
    private int[] stageTotalStarData; // 각 stage에서 획득한 총 별의 개수 (extra 미포함)
    private int[] extraStageTotalStarData;
    private List<int>[] stagePlayerPrefsData; // PlayerPrefs.GetInt 결과를 캐싱
    private List<int>[] extraStagePlayerPrefsData;

    [Header("Settings Data")]
    public bool isTileTouch { get; private set; }
    public float moveLatencyRate { get; private set; }

    [Header("Unlock Data")]
    private bool[] stageUnlockInformData;
    private bool[] extraStageUnlockInformData;
    private List<int> stageUnlockInformPrefsData;
    private List<int> extraStageUnlockInformPrefsData;

    private void Awake()
    {
        Init();
        LoadStageClearData();
        LoadUnlockData();
        isTileTouch = LoadIsTileTouch();
        moveLatencyRate = LoadMoveLatencyRate() / 100f;
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
        numOfStage = stageSO.numOfStage;
        int maxLevelNum = -1;
        int maxExtraLevelNum = -1;
        for (int i = 0; i < numOfStage; i++)
        {
            maxLevelNum = Mathf.Max(maxLevelNum, stageSO.numOfLevelOfStage[i]);
            maxExtraLevelNum = Mathf.Max(maxExtraLevelNum, stageSO.numOfLevelOfExtraStage[i]);
        }

        stageClearData = new int[numOfStage, maxLevelNum];
        stageTotalStarData = new int[numOfStage];
        stagePlayerPrefsData = new List<int>[numOfStage];
        extraStageClearData = new int[numOfStage, maxExtraLevelNum];
        extraStageTotalStarData = new int[numOfStage];
        extraStagePlayerPrefsData = new List<int>[numOfStage];
        totalStar = 0;

        for (int i = 0; i < numOfStage; i++)
        {
            stagePlayerPrefsData[i] = new List<int>();
            extraStagePlayerPrefsData[i] = new List<int>();
            LoadCertainStageClearData(stagePlayerPrefsData[i], stageSO.numOfLevelOfStage[i], i, false);
            LoadCertainStageClearData(extraStagePlayerPrefsData[i], stageSO.numOfLevelOfExtraStage[i], i, true);
            totalStar += stageTotalStarData[i] + extraStageTotalStarData[i];
        }

        for (int i = 0; i < numOfStage; i++)
            for (int j = 0; j < stageSO.numOfLevelOfStage[i]; j++)
                Logger.Log($"stage{i + 1}-{j + 1}:{stageClearData[i, j]}");
        for (int i = 0; i < numOfStage; i++)
            for (int j = 0; j < stageSO.numOfLevelOfExtraStage[i]; j++)
                Logger.Log($"extraStage{i + 1}-{j + 1}:{extraStageClearData[i, j]}");
        for (int i = 0; i < numOfStage; i++)
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

        var clearData = isExtra ? extraStageClearData : stageClearData;
        var totalStarData = isExtra ? extraStageTotalStarData : stageTotalStarData;
        var prefsData = isExtra ? extraStagePlayerPrefsData : stagePlayerPrefsData;
        string keyPrefix = isExtra ? "ExtraStage" : "Stage";

        // 스테이지 2 이상의 1레벨을 최초로 클리어했으면, 엑스트라 스테이지가 해금됨 (스테이지2를 건너뛸 가능성 고려)
        if (stage >= 1 && level == 0 && !isExtra && clearData[stage, level] == 0 && star > 0 && !HaveWeInformedExtraUnlock())
        {
            Logger.Log("엑스트라 스테이지 해금!"); WeInformedExtraUnlock(); NowWeNeedToInformExtraUnlock();
        }
        if (clearData[stage, level] >= star) return;

        Logger.Log($"{(isExtra ? "extra" : "normal")}, star : {clearData[stage, level]} -> {star}");

        int delta = star - clearData[stage, level];
        totalStarData[stage] += delta;
        totalStar += delta;
        clearData[stage, level] = star;

        int index = level / 16;
        int shift = 2 * (level % 16);
        int data = prefsData[stage][index];
        data = (data & ~(3 << shift)) | (star << shift);
        prefsData[stage][index] = data;

        PlayerPrefs.SetInt($"{keyPrefix}{stage + 1}ClearData{index + 1}", data);
    }

    #endregion

    #region Settings
    public static int LoadBGM() => PlayerPrefs.GetInt("bgm", 100);
    public static int LoadSFX() => PlayerPrefs.GetInt("sfx", 100);
    public static int LoadMoveLatencyRate() => PlayerPrefs.GetInt("moveLatencyRate", 70);
    public static bool LoadIsTileTouch() => PlayerPrefs.GetInt("isTileTouch", 1) == 1;
    public static bool LoadIsNoticeEnabled() => PlayerPrefs.GetInt("notice", 1) == 1;

    public static void SaveBgm(int bgm) => PlayerPrefs.SetInt("bgm", bgm);
    public static void SaveSfx(int sfx) => PlayerPrefs.SetInt("sfx", sfx);
    public static void SaveMoveLatencyRate(int moveLatencyRate) => PlayerPrefs.SetInt("moveLatencyRate", moveLatencyRate);
    public static void SaveIsTileTouch(bool isTileTouch) => PlayerPrefs.SetInt("isTileTouch", isTileTouch ? 1 : 0);
    public static void SaveIsNoticeEnabled(bool notice) => PlayerPrefs.SetInt("notice", notice ? 1 : 0);
    #endregion

    #region UnlockInform
    // 스테이지 선택 화면에서 엑스트라 스테이지 해금에 대한 안내 메시지를 띄워야 하는지
    public static bool DoWeNeedToInformExtraUnlock() => PlayerPrefs.GetInt("haveToInformExtraUnlock", 0) == 1;
    // 다음에 스테이지 선택 화면에 들어올 때 엑스트라 스테이지 해금에 대한 안내 메시지를 띄우도록 설정
    public static void NowWeNeedToInformExtraUnlock() => PlayerPrefs.SetInt("haveToInformExtraUnlock", 1);
    // 엑스트라 스테이지 해금에 대한 안내 메시지를 띄운 뒤, 다음에 다시 띄우지 않도록 해당 함수를 호출
    public static void WeInformedExtraUnlock()
    {
        PlayerPrefs.SetInt("haveToInformExtraUnlock", 0);
        PlayerPrefs.SetInt("haveInformedExtraUnlock", 1);
    }
    public static bool HaveWeInformedExtraUnlock() => PlayerPrefs.GetInt("haveInformedExtraUnlock", 0) == 1;
    public static void SetHaveWeInformedExtraUnlock() => PlayerPrefs.SetInt("haveInformedExtraUnlock", 1);

    private void LoadUnlockData()
    {
        int stageNum = stageSO.numOfStage;
        stageUnlockInformData = new bool[stageNum];
        extraStageUnlockInformData = new bool[stageNum];

        stageUnlockInformPrefsData = new List<int>();
        extraStageUnlockInformPrefsData = new List<int>();
        LoadCertainUnlockData(stageUnlockInformPrefsData, false);
        LoadCertainUnlockData(extraStageUnlockInformPrefsData, true);
    }

    private void LoadCertainUnlockData(List<int> prefsData, bool isExtra)
    {
        int curStage = 1;
        int count = 0;
        while (curStage < numOfStage)
        {
            count++;
            int data = PlayerPrefs.GetInt((isExtra ? "Extra" : "") + $"StageUnlockData{count}", 0);
            prefsData.Add(data);
            for (int j = 0; j < 32 && curStage <= numOfStage; j++)
            {
                if (isExtra)
                {
                    extraStageUnlockInformData[curStage - 1] = (data & (1 << j)) != 0;
                }
                else
                {
                    stageUnlockInformData[curStage - 1] = (data & (1 << j)) != 0;
                }
                curStage++;
            }
        }
        if (!stageUnlockInformData[0]) InformedCertainStageUnlock(1, false);
    }

    public void InformedCertainStageUnlock(int stageNum, bool isExtra)
    {
        int savedData;
        stageNum--;
        if (isExtra)
        {
            extraStageUnlockInformData[stageNum] = true;
            extraStageUnlockInformPrefsData[stageNum / 32] |= 1 << (stageNum % 32);
            savedData = extraStageUnlockInformPrefsData[stageNum / 32];
        }
        else
        {
            stageUnlockInformData[stageNum] = true;
            stageUnlockInformPrefsData[stageNum / 32] |= 1 << (stageNum % 32);
            savedData = stageUnlockInformPrefsData[stageNum / 32];
        }
        PlayerPrefs.SetInt((isExtra ? "Extra" : "") + $"StageUnlockData{1 + stageNum / 32}", savedData);
    }

    public bool CanStageUnlock(int stageNum, bool isExtra)
    {
        if (isExtra)
            return GetStageTotalStarData(stageNum) >= 3 * stageSO.numOfLevelOfStage[stageNum - 1];
        else
            return totalStar >= stageSO.numOfStarToUnlockStage[stageNum - 1];
    }

    public bool HaveInformedStageUnlock(int stageNum, bool isExtra)
    {
        if (isExtra)
            return extraStageUnlockInformData[stageNum - 1];
        else
            return stageUnlockInformData[stageNum - 1];
    }
    #endregion
}