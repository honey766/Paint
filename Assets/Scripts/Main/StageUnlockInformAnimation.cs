using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class StageUnlockInformAnimation : MonoBehaviour
{
    [SerializeField] private float informDuration = 1.2f;
    [SerializeField] private float flipDuration = 1f;
    [SerializeField] private float unlockDuration = 1f;
    [SerializeField] private float snapDurationRatio = 2.5f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private GameObject blockingUI;

    public bool isAnimating;
    private int numOfStage;
    private List<int> toInformStage = new(); // 음수면 extra
    private PersistentDataManager pdm;
    private CharacterSwiper cs;
    private int savedHorIndex, savedVerIndex;
    private ScrollIndicator[] scrollIndicators;
    private GameObject levelButtonPrefab;
    private bool doWeNeedToInformExtraUnlock;

    private void OnEnable()
    {
        isAnimating = true;
        doWeNeedToInformExtraUnlock = PersistentDataManager.DoWeNeedToInformExtraUnlock();
        cs = GetComponentInChildren<CharacterSwiper>();
        
        (savedHorIndex, savedVerIndex) = cs.GetSavedIndex();

        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        float startTime = Time.time;
        for (int i = 0; i < 3; i++)
            yield return null;
        
        pdm = PersistentDataManager.Instance;
        levelButtonPrefab = cs.levelButtonPrefab;
        scrollIndicators = GetComponentsInChildren<ScrollIndicator>();

        numOfStage = pdm.numOfStage;
        GetToInformStageData();
        blockingUI.SetActive(false);
        float time = Time.time - startTime;

        if (toInformStage.Count > 0)
            foreach (var entry in scrollIndicators) 
                entry.gameObject.SetActive(false);
        else
            isAnimating = false;

        if (time < 0.5f)
            yield return new WaitForSeconds(0.5f - time);

        if (toInformStage.Count > 0)
        {
            if (doWeNeedToInformExtraUnlock)
                StartCoroutine(SubscribeUnlockEvent());
            else
                InformStageUnlock();
        }
    }

    private IEnumerator SubscribeUnlockEvent()
    {
        yield return null;
        savedVerIndex = 1;
        FindAnyObjectByType<ExtraStageUnlockInformCanvas>().onClick += InformStageUnlockAfterSeconds;
    }

    private void GetToInformStageData()
    {
        int maxStage = numOfStage; // 엑스트라 해금 전
        if (PersistentDataManager.HaveWeInformedExtraUnlock() || PersistentDataManager.DoWeNeedToInformExtraUnlock())
            maxStage = 2 * numOfStage;
        
        for (int i = 0; i < maxStage; i++)
        {
            bool isExtra = i >= numOfStage;
            int curStage = (i % numOfStage) + 1;
            int sign = isExtra ? -1 : 1;

            if (pdm.CanStageUnlock(curStage, isExtra) && !pdm.HaveInformedStageUnlock(curStage, isExtra))
            {
                // 해당 스테이지 플레이 기록이 있다면
                if (!isExtra && pdm.GetStageClearData(curStage, 1) > 0 ||
                    isExtra && pdm.GetExtraStageClearData(curStage, 1) > 0)
                {
                    pdm.InformedCertainStageUnlock(curStage, isExtra);
                }
                else
                {
                    toInformStage.Add(sign * curStage);
                }
            }
        }
        foreach (int stageNum in toInformStage)
            Logger.Log($"이거 해금 알림 애니메이션 해야돼 {stageNum}");
    }

    private void InformStageUnlockAfterSeconds()
    {
        blockingUI.SetActive(true);
        Invoke(nameof(InformStageUnlock), 1f);
    }
    private void InformStageUnlock() => StartCoroutine(InformStageUnlockCoroutine());
    private IEnumerator InformStageUnlockCoroutine()
    {
        blockingUI.SetActive(true);
        bool isAnimated = false;
        CharacterSwiper characterSwiper = FindAnyObjectByType<CharacterSwiper>();
        characterSwiper.canGetKeyboardInput = false;

        foreach (int entryStage in toInformStage)
        {
            bool isExtra = entryStage < 0;
            int verIndex = isExtra ? 1 : 0;
            int stageIdx = Mathf.Abs(entryStage) - 1;
            pdm.InformedCertainStageUnlock(stageIdx + 1, isExtra);

            if (!isExtra && pdm.GetStageClearData(stageIdx + 1, 1) > 0) continue;
            if (isExtra && pdm.GetExtraStageClearData(stageIdx + 1, 1) > 0) continue;

            isAnimated = true;
            CharacterItem ci = cs.GetCharacterItem(stageIdx, verIndex);
            ci.SetBackUILocked(true);   
            cs.DoSnapping(stageIdx, verIndex, snapDurationRatio);
            yield return new WaitForSeconds(informDuration);

            cs.FlipCard(stageIdx, verIndex);
            yield return new WaitForSeconds(flipDuration);

            Transform backUI = ci.transform.GetChild(1);
            Transform backUnlockUI = backUI.GetChild(backUI.childCount - 1);
            UnlockAndFadeOut(backUnlockUI);
            AudioManager.Instance.PlaySfx(SfxType.Unlock);
            yield return new WaitForSeconds(unlockDuration);

            backUnlockUI.gameObject.SetActive(false);
        }
        PlayerPrefs.Save();

        cs.DoSnapping(savedHorIndex, savedVerIndex, snapDurationRatio); // 원래 자리로 돌아옴
        if (isAnimated && PersistentDataManager.HaveWeInformedExtraUnlock())
        {
            yield return new WaitForSeconds(cs.snapDuration * snapDurationRatio + 0.1f);
            // 스크롤 화살표 보이도록 설정
            foreach (var entry in scrollIndicators) 
            {
                entry.gameObject.SetActive(true);
                entry.currentAlpha = 0f;
                entry.UpdateImageAlpha();
                if (entry.isExplainButton) entry.ShowExplainButton();
            }
        }
        yield return new WaitForSeconds(0.3f); 

        characterSwiper.canGetKeyboardInput = true; 
        blockingUI.SetActive(false); // 스크롤 가능
        isAnimating = false;
    }

    private void UnlockAndFadeOut(Transform backUnlockUI)
    {
        for (int i = 0; i < 4; i++)
        {
            Transform child = backUnlockUI.GetChild(i);
            Image img = child.GetComponent<Image>();
            if (img != null) img.DOFade(0, fadeDuration);
            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.DOFade(0, fadeDuration);
        }
        Transform particle = backUnlockUI.GetChild(4);
        particle.gameObject.SetActive(true);
        particle.GetChild(0).GetComponent<ParticleSystem>().Emit(18);
    }
}
