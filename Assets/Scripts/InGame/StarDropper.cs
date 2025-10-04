using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StarDropper : MonoBehaviour
{
    [SerializeField] private float rotateTime = 0.1f;
    private Image starImage;
    [SerializeField] private bool hasDropped;

    private void Awake()
    {
        hasDropped = false;
        starImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            DropStar();
    }

    public void Init()
    {
        hasDropped = false;
    }

    public bool DropStar()
    {
        if (hasDropped) return false;
        hasDropped = true;

        RectTransform rect = starImage.GetComponent<RectTransform>();
        Vector2 startPos = rect.anchoredPosition;

        Sequence seq = DOTween.Sequence();

        int n = Random.Range(1, 3);
        for (int i = 0; i < n; i++)
        {
            seq.Append(rect.DORotate(Vector3.forward * Random.Range(3f, 7f), rotateTime * Random.Range(0.8f, 1.2f)));
            seq.Append(rect.DORotate(Vector3.back * Random.Range(3f, 7f), rotateTime * Random.Range(0.8f, 1.2f)));
        }

        Vector3 targetRotation = Vector3.forward * Random.Range(-30f, 30f);
        // 1. 살짝 위로 튀어오르기
        seq.Append(rect.DOAnchorPos(startPos + new Vector2(0, 50), 0.25f)
            .SetEase(Ease.OutQuad));
        seq.Join(rect.DORotate(targetRotation / 1.05f * 0.25f, 0.25f)).SetEase(Ease.Linear);

        // 2. 흔들리며 아래로 떨어지기
        seq.Append(rect.DOAnchorPos(startPos + new Vector2(Random.Range(-70, 70), -300), 0.8f)
            .SetEase(Ease.InCubic));

        // 동시에 페이드아웃
        seq.Join(starImage.DOFade(0f, 0.8f));
        seq.Join(rect.DORotate(targetRotation, 0.8f)).SetEase(Ease.Linear);

        // 끝나면 비활성화
        seq.OnComplete(() =>
        {
            starImage.gameObject.SetActive(false);
        });

        return true;
    }
}