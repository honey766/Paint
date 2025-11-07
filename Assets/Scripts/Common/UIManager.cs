using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField] float transitionDuration;
    [SerializeField] float waitDuration;
    [SerializeField] Color red, blue, purple;
    [SerializeField] private bool doingTransition;
    [SerializeField] private Transform transitionRectsParent;

    private GameObject exitGameObj;
    private RectTransform[] transitionRects;
    private Image[] transitionImages;
    private float rectWidth;

    private void Start()
    {
        doingTransition = false;

        List<RectTransform> rectList = new List<RectTransform>();

        foreach (Transform child in transitionRectsParent)
        {
            rectList.AddRange(child.GetComponentsInChildren<RectTransform>());
        }

        transitionRects = rectList.ToArray();
        transitionImages = new Image[transitionRects.Length];
        rectWidth = (float)Screen.width / transitionRects.Length;
        for (int i = 0; i < transitionRects.Length; i++)
        {
            transitionRects[i].sizeDelta = new Vector2(rectWidth, Screen.height);
            transitionRects[i].anchoredPosition = new Vector2(-Screen.width / 2f + rectWidth * (0.5f + i), Screen.height);
            transitionImages[i] = transitionRects[i].GetComponent<Image>();
        }
    }


    #region Transition
    public void ScreenTransition(Action action)
    {
        if (doingTransition) return;
        doingTransition = true;

        // 트랜지션 색 랜덤 결정
        Color color;
        int random = UnityEngine.Random.Range(0, 3);
        if (random == 0) color = red;
        else if (random == 1) color = blue;
        else color = purple;

        for (int i = 0; i < transitionRects.Length; i++)
            transitionImages[i].color = color;

        StartCoroutine(ScreenTransitionCoroutine(action));
    }

    private IEnumerator ScreenTransitionCoroutine(Action action)
    {
        AudioManager.Instance.PlaySfx(SfxType.Transition, 0.6f);
        for (int i = 0; i < transitionRects.Length; i++)
        {
            transitionRects[i].sizeDelta = new Vector2(rectWidth, Screen.height * Random.Range(1f, 1.3f));
            transitionRects[i].anchoredPosition = new Vector2(transitionRects[i].anchoredPosition.x, Screen.height * Random.Range(1.3f, 2f));
            transitionRects[i].DOAnchorPosY(0, transitionDuration - Random.Range(0f, 0.25f));
        }
        yield return new WaitForSeconds(transitionDuration);
        action();
        yield return new WaitForSeconds(waitDuration);
        for (int i = 0; i < transitionRects.Length; i++)
            transitionRects[i].DOAnchorPosY(-Screen.height * Random.Range(1.3f, 2f), transitionDuration - Random.Range(0f, 0.25f));
        yield return new WaitForSeconds(transitionDuration);
        doingTransition = false;
    }
    #endregion

    public void GoToChoiceLevelWhenComeToMainScene()
    {
        Invoke(nameof(GoToChoiceLevel), 0.02f);
    }
    private void GoToChoiceLevel()
    {
        MainManager transition = GameObject.Find("MainManager").GetComponent<MainManager>();
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

    public void OpenMenu(bool isGaming)
    {
        string path;
        if (isGaming) path = "Prefabs/GameMenuCanvas";
        else path = "Prefabs/MainMenuCanvas";

        GameObject menuPrefab = Resources.Load<GameObject>(path);

        if (menuPrefab == null)
        {
            Debug.LogError($"프리팹 로드 실패! 경로를 확인하세요: {path}");
            return;
        }

        Instantiate(menuPrefab);
    }

    public void OpenShop()
    {

    }

    public void ControlExitGamePopUp()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        
        if (exitGameObj == null)
        {
            exitGameObj = Resources.Load<GameObject>("Prefabs/ApplicationQuitCanvas");

            if (exitGameObj == null)
            {
                Debug.LogError($"프리팹 로드 실패! 경로를 확인하세요: {"Prefabs/ApplicationQuitCanvas"}");
                return;
            }

            exitGameObj = Instantiate(exitGameObj);
        }
        else
        {
            exitGameObj.SetActive(!exitGameObj.activeSelf);
        }
    }
}