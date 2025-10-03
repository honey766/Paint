using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Transition : MonoBehaviour
{
    [SerializeField] GameObject card;
    [SerializeField] GameObject main;

    public void OnPlayClick()
    {
        UIManager.Instance.ScreenTransition(() =>
        {
            main.SetActive(false);
            card.SetActive(true);
        });
    }
}
