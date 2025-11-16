using UnityEngine;
using UnityEngine.UI;

public class ScrollIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform scrollContent;
    [SerializeField] private CharacterSwiper characterSwiper;

    [Header("Settings")]
    [SerializeField] private bool isBottomIndicator;
    [SerializeField] private bool isExplainButton;
    [SerializeField] private float baseOffset = 60f;
    [SerializeField] private float fadeAnimationDuration = 0.2f;
    [SerializeField] private float bounceSpeed = 2.3f;
    [SerializeField] private float bounceAmplitude = 5f;
    [SerializeField] private float bouncePower = 0.85f;
    
    private RectTransform rectTransform;
    private Image image;
    private Color originalColor;
    public float currentAlpha;
    private float maxScrollPosition = UNINITIALIZED_VALUE;
    private bool recentIsFadeOut;
    
    private const float SCROLL_THRESHOLD = 0.1f;
    private const float UNINITIALIZED_VALUE = -100000f;

    private void Start()
    {
        if (!PersistentDataManager.HaveWeInformedExtraUnlock())
            gameObject.SetActive(false);
        InitializeComponents();
        currentAlpha = 1f;
        recentIsFadeOut = true;
    }

    private void Update()
    {
        InitializeMaxScrollPosition();
        UpdateIndicatorPosition();
        if (!isExplainButton)
            UpdateIndicatorVisibility();
    }

    private void InitializeComponents()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    private void InitializeMaxScrollPosition()
    {
        if (maxScrollPosition <= 0)
        {
            maxScrollPosition = characterSwiper.GetCanvasExtraYPosition();
        }
    }

    private void UpdateIndicatorPosition()
    {
        float yPosition = CalculateBaseYPosition();
        float bounceOffset = CalculateBounceOffset();
        
        rectTransform.anchoredPosition = new Vector2(
            rectTransform.anchoredPosition.x, 
            yPosition + bounceOffset
        );
    }

    private float CalculateBaseYPosition()
    {
        if (isBottomIndicator)
        {
            return baseOffset + scrollContent.anchoredPosition.y;
        }
        else
        {
            return -baseOffset + scrollContent.anchoredPosition.y - maxScrollPosition;
        }
    }

    private float CalculateBounceOffset()
    {
        if (isExplainButton)
        {
            return 0;
        }
        float sineWave = Mathf.Sin(Time.time * bounceSpeed);
        float poweredWave = Mathf.Sign(sineWave) * Mathf.Pow(Mathf.Abs(sineWave), bouncePower);
        return poweredWave * bounceAmplitude;
    }

    private void UpdateIndicatorVisibility()
    {
        bool shouldFadeOut = ShouldFadeOut();
        bool shouldFadeIn = ShouldFadeIn();

        if (shouldFadeOut && currentAlpha > 0)
        {
            recentIsFadeOut = true;
            FadeOut();
        }
        else if (shouldFadeIn && currentAlpha < 1)
        {
            recentIsFadeOut = false;
            FadeIn();
        }
    }

    private bool ShouldFadeOut()
    {
        if (isBottomIndicator)
        {
            return scrollContent.anchoredPosition.y > maxScrollPosition / 5f;
        }
        else
        {
            return scrollContent.anchoredPosition.y < maxScrollPosition * 4f / 5f;
        }
    }

    private bool ShouldFadeIn()
    {
        // 사라진 상태라면
        if (recentIsFadeOut)
        {
            if (isBottomIndicator)
            {
                return Mathf.Abs(scrollContent.anchoredPosition.y) < SCROLL_THRESHOLD;
            }
            else
            {
                return Mathf.Abs(scrollContent.anchoredPosition.y - maxScrollPosition) < SCROLL_THRESHOLD;
            }
        }
        // 나타나고 있는 상태라면
        else
        {
            if (isBottomIndicator)
            {
                return scrollContent.anchoredPosition.y < maxScrollPosition / 5f;
            }
            else
            {
                return scrollContent.anchoredPosition.y > maxScrollPosition * 4f / 5f;
            }
        }
    }

    private void FadeOut()
    {
        currentAlpha -= Time.deltaTime / fadeAnimationDuration;
        currentAlpha = Mathf.Max(currentAlpha, 0f);
        UpdateImageAlpha();
    }

    private void FadeIn()
    {
        currentAlpha += Time.deltaTime / fadeAnimationDuration;
        currentAlpha = Mathf.Min(currentAlpha, 1f);
        UpdateImageAlpha();
    }

    public void UpdateImageAlpha()
    {
        image.color = new Color(
            originalColor.r, 
            originalColor.g, 
            originalColor.b, 
            currentAlpha
        );
    }
}