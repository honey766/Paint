using System;
using System.Collections;
using UnityEngine;

public class ExtraStageUnlockInformCanvas : MonoBehaviour
{
    [SerializeField] private GameObject verticalCursorAnimPrefab;
    public Action onClick;

    private CharacterSwiper cs;

    public void OnClickButton()
    {
        cs = FindAnyObjectByType<CharacterSwiper>();
        cs.DoSnapToExtra();
        onClick?.Invoke();
        transform.GetChild(0).gameObject.SetActive(false);
        StartCoroutine(InstantiateCursor());
    }

    private IEnumerator InstantiateCursor()
    {
        yield return new WaitForSeconds(0.5f);
        StageUnlockInformAnimation unlockInform = FindAnyObjectByType<StageUnlockInformAnimation>();
        if (unlockInform != null)
        {
            while(unlockInform != null && unlockInform.isAnimating)
                yield return null;
        }

        GameObject verCursor = Instantiate(verticalCursorAnimPrefab);
        Transform cardCanvas = GameObject.Find("CardCanvas").transform;
        verCursor.transform.SetParent(cardCanvas, false);
        cs.verticalDragBeginEvent += (() =>
        {
            if (verCursor.activeSelf)
                verCursor.SetActive(false);
        });
        Destroy(gameObject);
    }
}