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

    Vector2 startPos = new Vector2(0, Screen.height);

    private void Start()
    {
        transitionRect.sizeDelta = new Vector2(Screen.width, Screen.height);
        transitionRect.anchoredPosition = startPos;
    }

    public void ScreenTransition(Action action)
    {
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
            .OnComplete(() => transitionRect.anchoredPosition = startPos);
    }
}