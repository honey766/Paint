using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;

public class MainManager : MonoBehaviour
{
    [SerializeField] GameObject card;
    [SerializeField] GameObject main;
    [SerializeField] TextMeshProUGUI starCountText;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ControlEscapeKey();
        // if (Input.GetKeyDown(KeyCode.A))
        //     PlayerPrefs.DeleteAll();
    }

    private void ControlEscapeKey()
    {
        if (UIManager.Instance.doingTransition)
            return;

        // 세팅 이동 설명 닫기
        GameObject moveExplain = GameObject.Find("MoveExplainCanvas(Clone)");
        if (moveExplain != null && moveExplain.activeSelf)
        {
            moveExplain.SetActive(false);
            return;
        }
            
        // 세팅 닫기
        Settings settings = FindAnyObjectByType<Settings>();
        if (settings != null)
        {
            settings.OnSettingExit();
            return;
        }

        // 상점 닫기
        GameObject shop = GameObject.Find("Shop(Clone)");
        if (shop != null)
        {
            AudioManager.Instance.PlaySfx(SfxType.Click1);
            Destroy(shop);
            return;
        }

        // 메뉴 닫기
        GameObject menu = GameObject.Find("MainMenuCanvas(Clone)");
        if (menu != null)
        {
            menu.GetComponent<Menu>().Resume();
            return;
        }
            
        if (card.activeSelf)
        {
            // 메인 메뉴로 돌아가기
            // CardToMainMenu();
            OpenMenu();
        }
        else
        {
            // 게임 종료 팝업 열고 닫기
            #if !UNITY_WEBGL
            UIManager.Instance.ControlExitGamePopUp();
            #endif
        }
    }

    public void MainMenuToCard()
    {
        UIManager.Instance.ScreenTransition(() =>
        {
            main.SetActive(false);
            card.SetActive(true);
            SetStarCountText();
        }, 
        () => !CardImageLoader.Instance.imageLoadingCompleted);
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
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        UIManager.Instance.OpenMenu(false);
    }

    public void OpenSettings()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        UIManager.Instance.OpenSettings();
    }

    public void OpenShop()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        UIManager.Instance.OpenShop();
    }

    public void GoToChoiceLevel()
    {
        main.SetActive(false);
        card.SetActive(true);
        SetStarCountText();
        Invoke(nameof(FlipCard), 0.05f);
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
