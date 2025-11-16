using System.Collections;
using UnityEngine;

public class SettingsMovementExplain : MonoBehaviour
{
    [SerializeField] private GameObject touchExplain, swipeExplain;
    [SerializeField] private GameObject leftButton, rightButton;

    public void Init(bool isTileTouch)
    {
        touchExplain.SetActive(isTileTouch);
        swipeExplain.SetActive(!isTileTouch);
        leftButton.SetActive(!isTileTouch);
        rightButton.SetActive(isTileTouch);
        StartCoroutine(SetPageDots(isTileTouch ? 0 : 1));
    }

    private IEnumerator SetPageDots(int idx)
    {
        yield return null;
        GetComponent<SwipeInputController>().SetPageDotsImmediately(idx);
    }
}
