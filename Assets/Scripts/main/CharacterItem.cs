using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;

public class CharacterItem : MonoBehaviour
{
    public int stage; // 1부터 시작
    public bool isExtra;

    [Header("UI References")]
    public Image frontUI; // 카드의 앞면 UI 그룹
    public Image backUI;  // 카드의 뒷면 UI 그룹
    public Sprite extraButtonSprite, extraButtonLockedSprite, buttonLockedSprite, backUISprite, starSprite;
    public Sprite[] frontSprites;

    [Header("Flip Settings")]
    public float flipDuration = 0.4f; // 뒤집히는 데 걸리는 시간

    private AsyncOperationHandle<Sprite> frontSpriteHandle;
    private bool isFlipped = false;
    public bool isSelected = false;
    private bool isAnimating = false;
    private bool isSetUped = false;

    void OnEnable()
    {
        isAnimating = false;
        transform.rotation = Quaternion.identity;
        frontUI.gameObject.SetActive(true);
        backUI.gameObject.SetActive(false);
        isFlipped = false;
    }

    // 초기 데이터 세팅 (이름, 이미지 등)

    public async void Setup(Character character, GameObject levelButtonPrefab, bool isExtra)
    {
        if (isSetUped) return;
        isSetUped = true;

        stage = character.Index;
        this.isExtra = isExtra;

        // 🔧 앞면 이미지 로딩 시작 (백그라운드에서 로드)
        string address = $"Assets/Sprites/Main/{character.PicName}.png";
        frontSpriteHandle = Addressables.LoadAssetAsync<Sprite>(address);

        // // 앞면 이미지 불러오기
        // Sprite spriteFront = Resources.Load<Sprite>("Images/" + character.PicName);
        // if (spriteFront != null)
        // {
        //     frontUI.sprite = spriteFront;
        //     Debug.Log($"<color=green>성공:</color> {character.PicName} 이미지를 성공적으로 로드했습니다.");
        // }
        // else
        // {
        //     Debug.LogError($"<color=red>실패:</color> 다음 경로에서 스프라이트를 찾을 수 없습니다:");
        // }

        backUI.sprite = backUISprite;

        // 스테이지 진입 가능
        if (!isExtra && PersistentDataManager.Instance.CanStageUnlock(stage, false))
        {
            SetButtonOfBackUI(levelButtonPrefab);
        }
        // 스테이지 진입 가능
        else if (isExtra && PersistentDataManager.Instance.CanStageUnlock(stage, true))
        {
            SetButtonOfBackUIExtra(levelButtonPrefab);
        }
        // 스테이지 진입 불가능
        else 
        {
            SetBackUILocked();
        }

        // 🎯 마지막에 앞면 이미지 로딩 완료 대기 및 적용
        await frontSpriteHandle.Task;

        if (frontSpriteHandle.Status == AsyncOperationStatus.Succeeded)
        {
            frontUI.sprite = frontSpriteHandle.Result;
            Logger.Log($"<color=green>성공:</color> {character.PicName} 이미지를 성공적으로 로드했습니다.");
        }
        else
        {
            Logger.LogError($"<color=red>실패:</color> {address} 경로에서 스프라이트를 찾을 수 없습니다:");
            if (frontSpriteHandle.OperationException != null)
                Logger.LogError($"Exception: {frontSpriteHandle.OperationException.Message}");
        }
    }
    public void SetBackUILocked(bool hideLevelButton = false)
    {
        GameObject obj = Resources.Load<GameObject>("Prefabs/BackUILocked");
        obj = Instantiate(obj, backUI.transform);
        obj.transform.GetChild(0).gameObject.SetActive(hideLevelButton);
        obj.transform.GetChild(1).gameObject.SetActive(!isExtra);
        if (!isExtra)
            obj.GetComponentInChildren<TextMeshProUGUI>().text = $"x {PersistentDataManager.Instance.stageSO.numOfStarToUnlockStage[stage - 1]}";
    }
    public void SetButtonOfBackUI(GameObject levelButtonPrefab)
    {
        int numOfLevel = PersistentDataManager.Instance.stageSO.numOfLevelOfStage[stage - 1];

        int x = -257, y = 575, diff = 256, i;
        int clearCnt = 0;
        for (i = 0; i < numOfLevel; i++)
        {
            if (PersistentDataManager.Instance.GetStageClearData(stage, i + 1) > 0)
                clearCnt++;
            InstantiateButton(levelButtonPrefab,
                              new Vector2(x + diff * (i % 3), y - diff * (i / 3)),
                              i + 1,
                              //true);
                              i == 0 || clearCnt > i / 3);
            //i == 0 || PersistentDataManager.Instance.GetStageClearData(stage, i) > 0);
        }
    }
    public void SetButtonOfBackUIExtra(GameObject levelButtonPrefab)
    {
        int numOfLevel = PersistentDataManager.Instance.stageSO.numOfLevelOfExtraStage[stage - 1];

        int x = -257, y = 575, diff = 256, i;
        for (i = 0; i < numOfLevel; i++)
        {
            InstantiateButton(levelButtonPrefab,
                              new Vector2(x + diff * (i % 3), y - diff * (i / 3)),
                              -i - 1,
                              true);
                              //i == 0 || clearCnt > i / 3);
                              //i == 0 || PersistentDataManager.Instance.GetStageClearData(stage, i) > 0);
        }
    }
    private void InstantiateButton(GameObject levelButtonPrefab, Vector2 anchoredPos, int level, bool canEnter)
    {
        GameObject button = Instantiate(levelButtonPrefab, backUI.transform);
        button.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
        
        // 레벨 진입 가능
        if (canEnter)
        {
            if (level < 0)
                button.GetComponent<Image>().sprite = extraButtonSprite;

            string levelStr = Mathf.Abs(level).ToString();
            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = levelStr;
            button.GetComponent<Button>().onClick.AddListener(() => OnStageButtonClick(level));
            // 별 표시
            int star;
            if (level > 0) star = PersistentDataManager.Instance.GetStageClearData(stage, level);
            else star = PersistentDataManager.Instance.GetExtraStageClearData(stage, level);
            for (int i = 0; i < 3; i++)
            {
                Transform starTr = button.transform.GetChild(i + 1);
                starTr.gameObject.SetActive(true);
                if (i < star)
                    starTr.GetComponent<Image>().sprite = starSprite;
            }
        }
        else
        {
            if (level < 0) button.GetComponent<Image>().sprite = extraButtonLockedSprite;
            else button.GetComponent<Image>().sprite = buttonLockedSprite;
        }
    }

    public void OnCardClick(bool playSfx)
    {
        OnCardClick(flipDuration / 2f, playSfx);
    }
    public void OnCardClick(float duration, bool playSfx)
    {
        float originalFlipDuration = flipDuration;
        flipDuration = duration;
        if (playSfx) OnCardClickWithSfx();
        else OnCardClickWithNoSfx();
        flipDuration = originalFlipDuration;
    }
    public void OnCardClickWithSfx()
    {
        if (!isSelected || isAnimating) return;
        AudioManager.Instance.PlaySfx(SfxType.FlipCard);
        OnCardClickWithNoSfx();
    }
    private void OnCardClickWithNoSfx()
    {
        if (!isSelected || isAnimating) return;

        isAnimating = true;

        isFlipped = !isFlipped;
        Sequence flipSequence = DOTween.Sequence();
        flipSequence.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2).SetEase(Ease.InQuad))
                    .AppendCallback(() =>
                    {
                        // 90도에서 앞/뒷면 교체
                        frontUI.gameObject.SetActive(!isFlipped);
                        backUI.gameObject.SetActive(isFlipped);

                        // 회전값을 즉시 270°(=-90°)로 세팅해서 반대편에서 시작
                        transform.rotation = Quaternion.Euler(0, 270, 0);
                    })
                    .Append(transform.DORotate(Vector3.zero, flipDuration / 2).SetEase(Ease.OutQuad))
                    .OnComplete(() => isAnimating = false);
    }

    // CharacterSwiper가 이 카드를 '중앙 카드'로 선택했을 때 호출
    public void SetSelected()
    {
        isSelected = true;
    }

    // CharacterSwiper가 이 카드를 '선택 해제'했을 때 호출
    public void SetUnselected()
    {
        // 만약 카드가 뒤집혀 있었다면, 즉시 앞면으로 되돌림
        if (isFlipped)
        {
            isSelected = true;
            isAnimating = false;
            transform.DOKill();
            OnCardClick(flipDuration / 2f, false);
        }
        isSelected = false;
    }

    public void OnStageButtonClick(int level)
    {
        if (UIManager.Instance.doingTransition) return;

        // stage, level 데이터 호출 후 PersistentDataManager에 저장
        if (PersistentDataManager.Instance.LoadStageAndLevel(stage, level))
        {
            Logger.Log($"Going To Stage {stage} - {level}");
            UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("InGame"));
            if (stage == 1 && level == 1) AudioManager.Instance.ChangeBgmWithTransition(BgmType.Tutorial);
            else AudioManager.Instance.ChangeBgmWithTransition(stage);
        }
        else
        {
            Logger.Log($"Failed to go to Stage {stage} - {level}");
        }
    }
}