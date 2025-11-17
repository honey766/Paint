using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HintController : MonoBehaviour
{
    [SerializeField] private GameObject notExists, backgroundImageParent, loadingText;
    private HintDrawer hintDrawer;

    private void Awake()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        backgroundImageParent.transform.GetChild(0).GetComponent<Image>().sprite
            = FindAnyObjectByType<BackgroundImageLoader>().GetComponent<Image>().sprite;
        loadingText.SetActive(true);

        while (!GameManager.Instance.hintLoadTaskCompleted)
            yield return null;
        List<BoardSO> boardSOs = GameManager.Instance.boardSOs;

        loadingText.SetActive(false);
        notExists.SetActive(boardSOs.Count == 0);
        backgroundImageParent.gameObject.SetActive(boardSOs.Count != 0);
        if (boardSOs.Count == 0)
            yield break;

        hintDrawer = GetComponentInChildren<HintDrawer>();
        hintDrawer.Draw(boardSOs.ToArray());
    }
    
    public void OnCloseClick()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        GameManager.Instance.isGaming = true;
        gameObject.SetActive(false);
    }
}
