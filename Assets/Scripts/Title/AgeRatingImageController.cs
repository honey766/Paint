using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class AgeRatingImageController : MonoBehaviour
{
    void Awake()
    {
        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        Image image = GetComponentInChildren<Image>();
        yield return new WaitForSeconds(2.8f);
        image.DOFade(0, 0.5f);
    }
}
