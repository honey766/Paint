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
    [SerializeField] private TextMeshProUGUI sliderPercent, status;
    [SerializeField] private GameObject touchText, goMainButton;
    [SerializeField] private float fadeDuration;
    private bool loadingFinished;



    void Start()
    {
        loadingFinished = false;
        loadingTime = Random.Range(0.75f, 1.25f);
        InitAddressables();
    }

    // async void InitAddressables()
    // {
    //     status.text = "Addressables 초기화 중...";

    //     var init = Addressables.InitializeAsync();
    //     await init.Task;

    //     if (init.Status == AsyncOperationStatus.Succeeded)
    //     {
    //         status.text = "초기화 성공!";
    //         Debug.Log($"RuntimePath: {Addressables.RuntimePath}");
    //     }
    //     else
    //     {
    //         status.text = $"초기화 실패: {init.OperationException?.Message}";
    //         Debug.LogError($"Exception: {init.OperationException}");
    //     }

    //     loadingFinished = true;
    // }

    void InitAddressables()
    {
        status.text = "Addressables 초기화 중...";

        var init = Addressables.InitializeAsync();
        
        // Completed 이벤트에 핸들러 등록
        init.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                status.text = "초기화 성공!";
                Debug.Log($"RuntimePath: {Addressables.RuntimePath}");
            }
            else
            {
                status.text = $"초기화 실패: {op.OperationException?.Message}";
                Debug.LogError($"Exception: {op.OperationException}");
            }

            loadingFinished = true;
        };
    }

    void Update()
    {
        if (!isReady)
        {
            t += Time.deltaTime;
            float value = Mathf.Min(loadingTime, t) / loadingTime;
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
                    StartCoroutine(ReadyCoroutine());
                }
            }
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
            // float t = Mathf.PingPong(Time.time - startTime, fadeDuration) / fadeDuration;
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
            AudioManager.Instance.PlayBgmImmediatelyAsync(BgmType.Title, 0.5f);
            SceneManager.LoadScene("Main");
        }
    }
}
