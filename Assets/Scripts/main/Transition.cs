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

    public void GoToChoiceLevel()
    {
        main.SetActive(false);
        card.SetActive(true);
        Invoke(nameof(FlipCard), 0.02f);
    }
    private void FlipCard()
    {
        card.transform.GetChild(0).GetComponent<CharacterSwiper>().FlipCardImmediately();
    }
}
