using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterItem : MonoBehaviour
{
    public int stage; // 1부터 시작

    [Header("UI References")]
    public Image frontUI; // 카드의 앞면 UI 그룹
    public Image backUI;  // 카드의 뒷면 UI 그룹
    public Sprite buttonLockedSprite, backUISprite, starSprite;


    [Header("Flip Settings")]
    public float flipDuration = 0.4f; // 뒤집히는 데 걸리는 시간

    private bool isFlipped = false;
    private bool isSelected = false;
    private bool isAnimating = false;

    void OnEnable()
    {
        isAnimating = false;
        transform.rotation = Quaternion.identity;
        frontUI.gameObject.SetActive(true);
        backUI.gameObject.SetActive(false);
        isFlipped = false;
    }

    // 초기 데이터 세팅 (이름, 이미지 등)

    public void Setup(Character character, GameObject levelButtonPrefab)
    {
        stage = character.Index;

        // 앞면 이미지 불러오기
        Sprite spriteFront = Resources.Load<Sprite>("Images/" + character.PicName);
        if (spriteFront != null)
        {
            frontUI.sprite = spriteFront;
            Debug.Log($"<color=green>성공:</color> {character.PicName} 이미지를 성공적으로 로드했습니다.");
        }
        else
        {
            Debug.LogError($"<color=red>실패:</color> 다음 경로에서 스프라이트를 찾을 수 없습니다:");
        }

        backUI.sprite = backUISprite;

        // 스테이지 진입 가능
        if (PersistentDataManager.Instance.totalStar >= PersistentDataManager.Instance.stageSO.numOfStarToUnlockStage[stage - 1])
        {
            SetButtonOfBackUI(levelButtonPrefab);
        }
        else // 스테이지 진입 불가능
        {
            GameObject obj = Resources.Load<GameObject>("Prefabs/BackUILocked");
            obj = Instantiate(obj, backUI.transform);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = $"x {PersistentDataManager.Instance.stageSO.numOfStarToUnlockStage[stage - 1]}";
        }
    }
    private void SetButtonOfBackUI(GameObject levelButtonPrefab)
    {
        int numOfLevel = PersistentDataManager.Instance.stageSO.numOfLevelOfStage[stage - 1];
        int numOfExtraLevel = PersistentDataManager.Instance.stageSO.numOfExtraLevelOfStage[stage - 1];

        int x = -257, y = 575, diff = 256;
        for (int i = 0; i < numOfLevel; i++)
        {
            InstantiateButton(levelButtonPrefab,
                              new Vector2(x + diff * (i % 3), y - diff * (i / 3)),
                              i + 1,
                              i == 0 || PersistentDataManager.Instance.GetStageClearData(stage, i) > 0);
        }

        y = 575 - 128 * 9;
        for (int i = 0; i < numOfExtraLevel; i++)
        {
            InstantiateButton(levelButtonPrefab,
                    new Vector2(x + diff * (i % 3), y - diff * (i / 3)),
                    -i - 1,
                    PersistentDataManager.Instance.GetStageTotalStarData(stage) >= numOfLevel * 3
                    && (i == 0 || PersistentDataManager.Instance.GetExtraStageClearData(stage, i) > 0));
        }
    }
    private void InstantiateButton(GameObject levelButtonPrefab, Vector2 anchoredPos, int level, bool canEnter)
    {
        GameObject button = Instantiate(levelButtonPrefab, backUI.transform);
        button.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
        // 레벨 진입 가능
        if (canEnter)
        {
            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Mathf.Abs(level).ToString();
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
            button.GetComponent<Image>().sprite = buttonLockedSprite;
        }
    }

    public void OnCardClick(float duration)
    {
        float originalFlipDuration = flipDuration;
        flipDuration = duration;
        OnCardClick();
        flipDuration = originalFlipDuration;
    }
    public void OnCardClick()
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
            OnCardClick(flipDuration / 2f);
        }
        isSelected = false;
    }

    public void OnStageButtonClick(int level)
    {
        if (PersistentDataManager.Instance.LoadStageAndLevel(stage, level))
        {
            Logger.Log($"Going To Stage {stage} - {level}");
            UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("InGame"));
        }
        else
        {
            Logger.Log($"Failed to go to Stage {stage} - {level}");
        }
    }
}