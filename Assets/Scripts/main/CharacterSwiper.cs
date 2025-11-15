 using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

// JSON 데이터 구조
[Serializable]
public class Character
{
    public int Index;
    public string Name;
    public string CV;
    public string PicName;
}

[Serializable]
public class Characters
{
    public Character[] characters;
}


public class CharacterSwiper : MonoBehaviour, IBeginDragHandler
{
    [Header("Core Components")]
    private Canvas canvas;
    public ScrollRect scrollRect;
    public RectTransform parentContent;
    public RectTransform[] contents;
    public RectTransform extraBackground;
    public RectTransform viewport;
    public GameObject characterCardPrefab;
    public GameObject levelButtonPrefab;

    [Header("Card Effect Settings")]
    public float spacing = 350f;
    public float scaleFactor = 0.7f;
    public float rotationFactor = 25f;

    [Header("Snap Settings")]
    public float snapDuration = 0.3f;
    private bool isSnapping = false;
    private bool isHorizontal = false;
    private Tween snapTween;
    private int curHorIndex, curVerIndex;
    private float canvasScaleFactor;
    private float extraContentHeight;

    [Header("Other")]
    private const float ContentSpacing = 600;

    private List<RectTransform>[] cardRects = new List<RectTransform>[2];
    private List<CharacterItem>[] characterItems = new List<CharacterItem>[2];

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasScaleFactor = canvas.scaleFactor;
        extraContentHeight = canvas.GetComponent<RectTransform>().rect.height + ContentSpacing;
        contents[1].anchoredPosition = new Vector2(0, -extraContentHeight);
        extraBackground.offsetMin = new Vector2(-extraContentHeight, -extraContentHeight); // Bottom
        extraBackground.offsetMax = new Vector2(extraContentHeight, ContentSpacing);  // Top

        (int savedHorIndex, int savedVerIndex) = GetSavedIndex();
        if (savedVerIndex == 1)
            PersistentDataManager.HaveWeInformedExtraUnlock();
        LoadAndSetupCharacters();
        StartCoroutine(InitSnapToCard((savedHorIndex, savedVerIndex)));

        if (PersistentDataManager.DoWeNeedToInformExtraUnlock())
        {
            GameObject extraUnlockInformCanvas = Resources.Load<GameObject>("Prefabs/ExtraStageUnlockInformCanvas");
            Instantiate(extraUnlockInformCanvas);
            PersistentDataManager.WeInformedExtraUnlock();
        }
    }

    void Update()
    {
        UpdateCardTransforms();
    }
    private (int, int) GetSavedIndex()
    {
        // 저장된 인덱스 불러오기 (없으면 0으로 기본값)
        return (PlayerPrefs.GetInt("LastSelectedCardHorizontal", 0), PlayerPrefs.GetInt("LastSelectedCardVertical", 0));
    }
    private IEnumerator InitSnapToCard((int, int) index)
    {
        // 한 프레임 기다렸다가 레이아웃 계산이 끝난 뒤 실행
        yield return null;
        
        parentContent.sizeDelta = new Vector2(contents[0].sizeDelta.x, 0);
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentContent);

        yield return null;

        if (cardRects[0].Count > 0 && index.Item1 >= 0 && index.Item1 < cardRects[0].Count)
        {
            float centerX = viewport.position.x;
            float offset = (centerX - cardRects[0][index.Item1].position.x) / canvasScaleFactor;
            float positionY = index.Item2 * extraContentHeight;
            //Logger.Log($"centerX {centerX}, offset;{offset}, posY:{positionY}, anchorX:{parentContent.anchoredPosition.x}");
            //Logger.Log($"add:{parentContent.anchoredPosition.x + offset}");
            parentContent.anchoredPosition = new Vector2(parentContent.anchoredPosition.x + offset, positionY);
            //Logger.Log($"anchor:{parentContent.anchoredPosition}");

            UpdateCardTransforms();
        }
    }

    private void LoadAndSetupCharacters()
    {
        // JSON 파일 로드
        TextAsset characterJson = Resources.Load<TextAsset>("Character");
        if (characterJson == null)
        {
            Debug.LogError("Character.json not found in Resources folder.");
            return;
        }

        Characters characterData = JsonUtility.FromJson<Characters>(characterJson.text);

        for (int i = 0; i < 2; i++)
        {
            characterItems[i] = new List<CharacterItem>();
            cardRects[i] = new List<RectTransform>();
                
            // JSON 데이터 기반으로 카드 프리팹 생성
            foreach (var character in characterData.characters)
            {
                GameObject cardObject = Instantiate(characterCardPrefab, contents[i]);
                CharacterItem item = cardObject.GetComponent<CharacterItem>();
                item.Setup(character, levelButtonPrefab, i == 1);

                // 생성된 카드와 컴포넌트들을 리스트에 저장
                characterItems[i].Add(item);
                cardRects[i].Add(cardObject.GetComponent<RectTransform>());

                // 카드의 버튼에 OnCardClick 이벤트 연결
                cardObject.GetComponent<Button>().onClick.AddListener(item.OnCardClickWithSfx);
            }
        }
    }

    private void UpdateCardTransforms()
    {
        float centerX = viewport.position.x;

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < cardRects[j].Count; i++)
            {
                float cardCenterX = cardRects[j][i].position.x;
                float distance = Mathf.Abs(cardCenterX - centerX);

                // 거리에 따라 스케일과 Y축 회전값 계산
                float t = Mathf.Clamp01(distance / spacing);
                float t2 = 1 - Mathf.Cos(t * Mathf.PI / 2f);
                float scale = Mathf.Lerp(1f, scaleFactor, t2);//distance / spacing);
                float rotationY = Mathf.Lerp(0, rotationFactor, distance / spacing) * Mathf.Sign(cardCenterX - centerX);

                // 적용
                cardRects[j][i].localScale = Vector3.one * scale;
                // cardRects[i].localRotation = Quaternion.Euler(0, rotationY, 0);

                // 중앙에 가장 가까운 카드(스케일이 거의 1)를 '선택된' 상태로 만듦
                if (scale > 0.99f)
                {
                    characterItems[j][i].SetSelected();
                }
                else
                {
                    characterItems[j][i].SetUnselected();
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 엑스트라 스테이지 해금 조건
        if (PersistentDataManager.HaveWeInformedExtraUnlock())
        {
            isHorizontal = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);
            scrollRect.horizontal = isHorizontal;
            scrollRect.vertical = !isHorizontal;
        }
        // 해금이 안 됐다면 수평 스크롤만 허용
        else
        {
            isHorizontal = true;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
        }   

        (curHorIndex, curVerIndex) = GetNearestIndex();
        Logger.Log($"({curHorIndex}, {curVerIndex})");

        if (snapTween != null && snapTween.IsActive())
        {
            snapTween.Kill();
        }
    }

    // ScrollRect의 EventTrigger(EndDrag)에 연결할 함수
    public void OnEndDrag()
    {
        SnapToClosest();
    }

    private void SnapToClosest()
    {
        (int nearestHorIndex, int nearestVerIndex) = GetNearestIndex();
        float swipeThreshold = 250;

        if (isHorizontal && curHorIndex == nearestHorIndex)
        {
            if (scrollRect.velocity.x > swipeThreshold)
                nearestHorIndex = Mathf.Max(nearestHorIndex - 1, 0);
            else if (scrollRect.velocity.x < -swipeThreshold)
                nearestHorIndex = Mathf.Min(nearestHorIndex + 1, cardRects[0].Count - 1);
        }
        else if (!isHorizontal && curVerIndex == nearestVerIndex)
        {
            if (scrollRect.velocity.y < -swipeThreshold)
                nearestVerIndex = Mathf.Max(nearestVerIndex - 1, 0);
            else if (scrollRect.velocity.y > swipeThreshold)
                nearestVerIndex = Mathf.Min(nearestVerIndex + 1, 1);
        }

        AudioManager.Instance.PlaySfx(SfxType.SelectCard);
        scrollRect.StopMovement();
        scrollRect.velocity = Vector2.zero;

        // float offset = (centerX - cardRects[0][nearestHorIndex].position.x) / canvasScaleFactor;
        // Vector2 targetPos = new Vector2(parentContent.anchoredPosition.x + offset, nearestVerIndex * extraContentHeight);

        // isSnapping = true;
        // snapTween = parentContent.DOAnchorPos(targetPos, snapDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        // {
        //     isSnapping = false;
        //     PlayerPrefs.SetInt("LastSelectedCardHorizontal", nearestHorIndex);
        //     PlayerPrefs.SetInt("LastSelectedCardVertical", nearestVerIndex);
        //     PlayerPrefs.Save();
        // });

        isSnapping = false;
        DoSnapping(nearestHorIndex, nearestVerIndex);
    }

    private void DoSnapping(int hor, int ver)
    {
        if (isSnapping) return;

        float centerX = viewport.position.x;
        float offset = (centerX - cardRects[0][hor].position.x) / canvasScaleFactor;
        Vector2 targetPos = new Vector2(parentContent.anchoredPosition.x + offset, ver * extraContentHeight);

        PlayerPrefs.SetInt("LastSelectedCardHorizontal", hor);
        PlayerPrefs.SetInt("LastSelectedCardVertical", ver);
        PlayerPrefs.Save();

        isSnapping = true;
        snapTween = parentContent.DOAnchorPos(targetPos, snapDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            isSnapping = false;
        });
    }

    // public void DoSnappingButton()
    // {
    //     AudioManager.Instance.PlaySfx(SfxType.Click1);
    //     if (isSnapping) return;

    //     (int nearestHorIndex, int nearestVerIndex) = GetNearestIndex();
    //     nearestVerIndex = 1 - nearestVerIndex;
    //     DoSnapping(nearestHorIndex, nearestVerIndex);
    //     goUpDownButtonImage.DORotate(GetGoUpDownImageRotationVector(nearestVerIndex), 0.2f);
    // }

    public void DoSnapToExtra()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);

        (int nearestHorIndex, int nearestVerIndex) = GetNearestIndex();
        nearestVerIndex = 1;
        DoSnapping(nearestHorIndex, nearestVerIndex);
    }

    private Vector3 GetGoUpDownImageRotationVector(int verIndex)
    {
        return Vector3.forward * 90 * (1 - 2 * verIndex);
    }
    private Quaternion GetGoUpDownImageRotationQuaternion(int verIndex)
    {
        return Quaternion.Euler(Vector3.forward * 90 * (1 - 2 * verIndex));
    }

    private (int, int) GetNearestIndex()
    {
        float centerX = viewport.position.x;
        float minDistance = float.MaxValue;

        int nearestHorIndex = 0;
        int nearestVerIndex = cardRects[0][0].position.y / canvasScaleFactor < extraContentHeight ? 0 : 1;

        for (int i = 0; i < cardRects[0].Count; i++)
        {
            float distance = Mathf.Abs(cardRects[0][i].position.x - centerX); //Mathf.Abs(cardRects[i].anchoredPosition.x - centerX);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestHorIndex = i;
            }
        }
        return (nearestHorIndex, nearestVerIndex);
    }

    public void FlipCardImmediately()
    {
        characterItems[PlayerPrefs.GetInt("LastSelectedCardVertical", 0)][PlayerPrefs.GetInt("LastSelectedCardHorizontal", 0)].OnCardClick(0, true);
    }


    public float GetCanvasExtraYPosition()
    {
        return extraContentHeight;
    }
}