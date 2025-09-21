using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterItem : MonoBehaviour
{
    [Header("UI References")]
    public Image frontUI; // 카드의 앞면 UI 그룹
    public Image backUI;  // 카드의 뒷면 UI 그룹


    [Header("Flip Settings")]
    public float flipDuration = 0.4f; // 뒤집히는 데 걸리는 시간

    private bool isFlipped = false;
    private bool isSelected = false;
    private bool isAnimating = false;

    // 초기 데이터 세팅 (이름, 이미지 등)

    public void Setup(Character character)
    {
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

        // 뒷면 이미지 불러오기 (없으면 앞면 재사용)
        //Sprite spriteBack = Resources.Load<Sprite>("Image/" + character.PicName);
        //if (spriteBack != null)
        //backImage.sprite = spriteBack;
        // else
        // backImage.sprite = spriteFront;
    }

    // 카드를 터치했을 때 호출될 함수
    public void OnCardClick()
    {
        // 중앙에 선택된 카드가 아니거나, 이미 애니메이션 중이면 무시
        if (!isSelected || isAnimating) return;

        isAnimating = true;

        float targetY = isFlipped ? 0f : 180f;

        Sequence flipSequence = DOTween.Sequence();
        flipSequence.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2).SetEase(Ease.InQuad))
                    .AppendCallback(() =>
                    {
                        // 90도 회전했을 때 앞/뒷면 교체
                        isFlipped = !isFlipped;
                        frontUI.gameObject.SetActive(!isFlipped);
                        backUI.gameObject.SetActive(isFlipped);
                    })
                    .Append(transform.DORotate(new Vector3(0, targetY, 0), flipDuration / 2).SetEase(Ease.OutQuad))
                    .OnComplete(() =>
                    {
                        isAnimating = false;
                    });
    }

    // CharacterSwiper가 이 카드를 '중앙 카드'로 선택했을 때 호출
    public void SetSelected()
    {
        isSelected = true;
    }

    // CharacterSwiper가 이 카드를 '선택 해제'했을 때 호출
    public void SetUnselected()
    {
        isSelected = false;

        // 만약 카드가 뒤집혀 있었다면, 즉시 앞면으로 되돌림
        if (isFlipped)
        {
            transform.DOKill(); // 진행중인 모든 애니메이션 정지
            isFlipped = false;
            frontUI.gameObject.SetActive(true);
            backUI.gameObject.SetActive(false);
            transform.rotation = Quaternion.identity; // 회전값 즉시 초기화
        }
    }
}