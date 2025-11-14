using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleLoading : MonoBehaviour
{
    private float t = 0;
    private bool isReady = false;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI sliderPercent;
    [SerializeField] private GameObject touchText, goMainButton;
    [SerializeField] private float fadeDuration;

    void Update()
    {
        if (!isReady)
        {
            t += Time.deltaTime;
            float value = Mathf.Min(2f, t / 2f);
            sliderPercent.text = $"{(int)(value * 100 + 0.01f)}%";
            slider.value = value;
            if (t > 2f)
            {
                isReady = true;
                StartCoroutine(ReadyCoroutine());
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
            AudioManager.Instance.PlayBgmImmediately(BgmType.Title, 0.5f);
            SceneManager.LoadScene("Main");
        }
    }
}
