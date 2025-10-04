using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Transition : MonoBehaviour
{
    [SerializeField] GameObject card;
    [SerializeField] GameObject main;

    public void MainMenuToCard()
    {
        UIManager.Instance.ScreenTransition(() =>
        {
            main.SetActive(false);
            card.SetActive(true);
        });
    }

    public void CardToMainMenu()
    {
        UIManager.Instance.ScreenTransition(() =>
        {
            main.SetActive(true);
            card.SetActive(false);
        });
    }

    public void OpenMenu()
    {
        UIManager.Instance.OpenMenu(false);
    }

    public void OpenSettings()
    {
        UIManager.Instance.OpenSettings();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            OpenMenu();
    }

    public void OpenShop()
    {
        UIManager.Instance.OpenShop();
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
