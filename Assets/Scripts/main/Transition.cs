using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;

public class Transition : MonoBehaviour
{
    [SerializeField] GameObject card;
    [SerializeField] GameObject main;
    [SerializeField] TextMeshProUGUI starCountText;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    public void MainMenuToCard()
    {
        UIManager.Instance.ScreenTransition(() =>
        {
            main.SetActive(false);
            card.SetActive(true);
            SetStarCountText();
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
        SetStarCountText();
        Invoke(nameof(FlipCard), 0.02f);
    }
    private void FlipCard()
    {
        card.transform.GetChild(0).GetComponent<CharacterSwiper>().FlipCardImmediately();
    }
    private void SetStarCountText()
    {
        starCountText.text = $"x {PersistentDataManager.Instance.totalStar}";
    }
}
