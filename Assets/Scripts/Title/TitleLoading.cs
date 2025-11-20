using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleLoading : MonoBehaviour
{
    private float loadingTime;
    private float t = 0;
    private bool isReady = false;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sliderPercent;
    [SerializeField] private GameObject touchText, goMainButton;
    [SerializeField] private float fadeDuration;
    private bool loadingFinished;
    private float prevValue = 0f;
    private AsyncOperationHandle<AudioClip> pendingLoadHandle = default;

    void Start()
    {
        loadingFinished = false;
        loadingTime = Random.Range(3.1f, 3.6f);
        InitAddressables();
    }

    void InitAddressables()
    {
        var init = Addressables.InitializeAsync();
        
        // Completed 이벤트에 핸들러 등록
        init.Completed += (op) =>
        {
            loadingFinished = true;
            pendingLoadHandle = AudioManager.Instance.PreloadBgmAsync(BgmType.Title);
        };
    }

    void Update()
    {
        if (!isReady)
        {
            t += Time.deltaTime;
            float value = Mathf.Min(loadingTime, t) / loadingTime;
            float noise = Mathf.PerlinNoise(Time.time * 3f, 0f) * 0.02f - 0.04f;
            noise += Random.Range(-0.5f, 0.5f) * Time.deltaTime;
            value = Mathf.Clamp01(value + noise);
            value = Mathf.Max(prevValue, value);
            sliderPercent.text = $"{(int)(value * 100 + 0.01f)}%";
            slider.value = value;
            if (t > loadingTime)
            {
                if (!loadingFinished)
                {
                    sliderPercent.text = $"99%";
                    slider.value = 0.99f;
                }
                else
                {
                    isReady = true;
                    sliderPercent.text = $"100%";
                    slider.value = 1f;
                    StartCoroutine(ReadyCoroutine());
                }
            }
            prevValue = value;
        }
    }

    private IEnumerator ReadyCoroutine()
    {
        touchText.SetActive(true);
        goMainButton.SetActive(true);
        TextMeshProUGUI txt = touchText.GetComponent<TextMeshProUGUI>();

        float startTime = Time.time - fadeDuration / 11f;
        float alpha;
        while (true)
        {
            float t = ((Time.time - startTime) % fadeDuration) / fadeDuration;
            alpha = Mathf.Max(0, Mathf.Sin((1.1f * t - 0.1f) * Mathf.PI));
            txt.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    public void GoMainButton()
    {
        if (isReady)
        {
            AudioManager.Instance.PlayBgmImmediatelyAsync(BgmType.Title, 0.5f, pendingLoadHandle);
            SceneManager.LoadScene("Main");
        }
    }
}