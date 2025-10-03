using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField] RectTransform transitionRect;
    [SerializeField] float transitionDuration;
    [SerializeField] float waitDuration;
    [SerializeField] Color red, blue, purple;

    private Vector2 startPos;
    [SerializeField] private bool doingTransition;

    private void Start()
    {
        startPos = new Vector2(0, Screen.height);
        transitionRect.sizeDelta = new Vector2(Screen.width, Screen.height);
        transitionRect.anchoredPosition = startPos;
        doingTransition = false;
    }

    public void ScreenTransition(Action action)
    {
        if (doingTransition) return;
        doingTransition = true;

        int random = UnityEngine.Random.Range(0, 3);
        Image image = transitionRect.GetComponent<Image>();
        if (random == 0) image.color = red;
        else if (random == 1) image.color = blue;
        else if (random == 2) image.color = purple;

        StartCoroutine(ScreenTransitionCoroutine(action));
    }

    private IEnumerator ScreenTransitionCoroutine(Action action)
    {
        transitionRect.DOAnchorPosY(0, transitionDuration);
        yield return new WaitForSeconds(transitionDuration);
        action();
        yield return new WaitForSeconds(waitDuration);
        transitionRect.DOAnchorPosY(-Screen.height, transitionDuration)
            .OnComplete(() => { transitionRect.anchoredPosition = startPos; doingTransition = false; });
    }

    public void GoToChoiceLevelWhenComeToMainScene()
    {
        Invoke(nameof(GoToChoiceLevel), 0.02f);
    }
    private void GoToChoiceLevel()
    {
        Transition transition = GameObject.Find("MainManager").GetComponent<Transition>();
        transition.GoToChoiceLevel();
    }

    public void OpenSettings()
    {
        GameObject settingsPrefab = Resources.Load<GameObject>("Prefabs/SettingsCanvas");

        if (settingsPrefab == null)
        {
            Debug.LogError("프리팹 로드 실패! 경로를 확인하세요: Prefabs/SettingsCanvas");
            return;
        }

        Instantiate(settingsPrefab);
    }
}