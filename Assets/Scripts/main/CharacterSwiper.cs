 using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System;
using System.Collections;

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


public class CharacterSwiper : MonoBehaviour
{
    [Header("Core Components")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public RectTransform viewport;
    public GameObject characterCardPrefab; 

    [Header("Card Effect Settings")]
    public float spacing = 350f;
    public float scaleFactor = 0.7f;
    public float rotationFactor = 25f;

    [Header("Snap Settings")]
    public float snapDuration = 0.3f;
    private bool isSnapping = false;
    private Tween snapTween;
    private int curIndex;

    private List<RectTransform> cardRects = new List<RectTransform>();
    private List<CharacterItem> characterItems = new List<CharacterItem>();

    void Start()
    {
        LoadAndSetupCharacters();
        StartCoroutine(InitSnapToCard(GetSavedIndex()));
    }

    void Update()
    {
        //if (isSnapping) return;
        UpdateCardTransforms();
    }
    private int GetSavedIndex()
    {
        // 저장된 인덱스 불러오기 (없으면 0으로 기본값)
        return PlayerPrefs.GetInt("LastSelectedCard", 0);
    }
    private IEnumerator InitSnapToCard(int index)
    {
        // 한 프레임 기다렸다가 레이아웃 계산이 끝난 뒤 실행
        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        if (cardRects.Count > 0 && index >= 0 && index < cardRects.Count)
        {
            float centerX = viewport.position.x;
            float offset = centerX - cardRects[index].position.x;
            content.anchoredPosition = new Vector2(content.anchoredPosition.x + offset, content.anchoredPosition.y);

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

        // JSON 데이터 기반으로 카드 프리팹 생성
        foreach (var character in characterData.characters)
        {
            GameObject cardObject = Instantiate(characterCardPrefab, content);
            CharacterItem item = cardObject.GetComponent<CharacterItem>();
            item.Setup(character);

            // 생성된 카드와 컴포넌트들을 리스트에 저장
            characterItems.Add(item);
            cardRects.Add(cardObject.GetComponent<RectTransform>());

            // 카드의 버튼에 OnCardClick 이벤트 연결
            cardObject.GetComponent<Button>().onClick.AddListener(item.OnCardClick);
        }
    }

    private void UpdateCardTransforms()
    {
        float centerX = viewport.position.x; //-content.anchoredPosition.x; // 뷰포트의 중심 X좌표

        for (int i = 0; i < cardRects.Count; i++)
        {
            float cardCenterX = cardRects[i].position.x; //cardRects[i].anchoredPosition.x + content.anchoredPosition.x;
            float distance = Mathf.Abs(cardCenterX - centerX);

            // 거리에 따라 스케일과 Y축 회전값 계산
            float t = Mathf.Clamp01(distance / spacing);
            float t2 = 1 - Mathf.Cos(t * Mathf.PI / 2f);
            float scale = Mathf.Lerp(1f, scaleFactor, t2);//distance / spacing);
            float rotationY = Mathf.Lerp(0, rotationFactor, distance / spacing) * Mathf.Sign(cardCenterX - centerX);
            
            // 적용
            cardRects[i].localScale = Vector3.one * scale;
            cardRects[i].localRotation = Quaternion.Euler(0, rotationY, 0);

            // 중앙에 가장 가까운 카드(스케일이 거의 1)를 '선택된' 상태로 만듦
            if (scale > 0.99f)
            {
                characterItems[i].SetSelected();
            }
            else
            {
                characterItems[i].SetUnselected();
            }
        }
    }

    public void OnBeginDrag()
    {
        curIndex = GetNearestIndex();

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
        float centerX = viewport.position.x; //-content.anchoredPosition.x;
        int nearestIndex = GetNearestIndex();

        Logger.Log($"Aa{scrollRect.velocity.x}, {nearestIndex}");
        if (curIndex == nearestIndex)
        {
            if (scrollRect.velocity.x > 500) 
                nearestIndex = Mathf.Max(nearestIndex - 1, 0);
            else if (scrollRect.velocity.x < -500) 
                nearestIndex = Mathf.Min(nearestIndex + 1, cardRects.Count - 1);
            Logger.Log($"Aa{scrollRect.velocity.x}, {nearestIndex}");
        }

        scrollRect.StopMovement();
        scrollRect.velocity = Vector2.zero;

        float offset = centerX - cardRects[nearestIndex].position.x;
        Vector2 targetPos = new Vector2(content.anchoredPosition.x + offset, content.anchoredPosition.y);

        isSnapping = true;
        snapTween = content.DOAnchorPos(targetPos, snapDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            isSnapping = false;
            PlayerPrefs.SetInt("LastSelectedCard", nearestIndex);
            PlayerPrefs.Save();
        });
    }

    private int GetNearestIndex()
    {
        float centerX = viewport.position.x; //-content.anchoredPosition.x;
        float minDistance = float.MaxValue;
        int nearestIndex = 0;

        for (int i = 0; i < cardRects.Count; i++)
        {
            float distance = Mathf.Abs(cardRects[i].position.x - centerX); //Mathf.Abs(cardRects[i].anchoredPosition.x - centerX);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }
}