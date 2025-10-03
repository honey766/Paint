using UnityEngine;
using UnityEngine.UI;

public class PaintMainScreenController : MonoBehaviour
{
    private static readonly int DashLengthID = Shader.PropertyToID("_DashLength");
    private static readonly int ScreenXID = Shader.PropertyToID("_ScreenX");
    private static readonly int ScreenYID = Shader.PropertyToID("_ScreenY");

    private void Awake()
    {
        Logger.Log($"{(float)Screen.height / Screen.width}\n{Screen.width},{Screen.height}");


        float ratio = (float)Screen.height / Screen.width;
        float buttonWidth = 160f;
        if (ratio > 2f)
            buttonWidth = Mathf.Lerp(160, 120, ratio - 2f);

        float dashLength = 1f / 60f * Mathf.Max(Screen.width, Screen.height);

        Image[] horizons = transform.Find("HorizontalLine").GetComponentsInChildren<Image>();
        for (int i = 0; i < horizons.Length; i++)
        {
            horizons[i].material.SetFloat(ScreenXID, Screen.width);
            horizons[i].material.SetFloat(DashLengthID, dashLength);
            horizons[i].GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width * 2, 6);
            horizons[i].GetComponent<RectTransform>().anchoredPosition =
                new Vector2(0, buttonWidth * (i - 0.5f * (horizons.Length - 1)));
        }

        Image[] verticals = transform.Find("VerticalLine").GetComponentsInChildren<Image>();
        for (int i = 0; i < verticals.Length; i++)
        {
            verticals[i].material.SetFloat(ScreenYID, Screen.height);
            verticals[i].material.SetFloat(DashLengthID, dashLength);
            verticals[i].GetComponent<RectTransform>().sizeDelta = new Vector2(6, Screen.height * 2);
            verticals[i].GetComponent<RectTransform>().anchoredPosition =
                new Vector2(buttonWidth * (i - 0.5f * (verticals.Length - 1)), 0);
        }

        RectTransform[] buttons = transform.Find("Button").GetComponentsInChildren<RectTransform>();
        for (int i = 1; i < buttons.Length; i++)
        {
            buttons[i].anchoredPosition = new Vector2(2 * buttonWidth * (i - 1 - 0.5f * (buttons.Length - 2)), 0);
            buttons[i].sizeDelta = Vector2.one * (buttonWidth + 6);
        }

        RectTransform[] texts = transform.Find("Paint").GetComponentsInChildren<RectTransform>();
        for (int i = 1; i < texts.Length; i++)
        {
            texts[i].anchoredPosition = new Vector2(buttonWidth * (i - 1 - 0.5f * (texts.Length - 2)), 370);
        }
    }
}