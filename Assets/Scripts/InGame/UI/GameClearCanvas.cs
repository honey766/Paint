using System.Collections;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class GameClearCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform[] stars;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    [SerializeField] private float firstWait = 0.2f, afterWait = 0.4f;
    private bool canTouch;

    public void Init(int star, string goToNextLevelText)
    {
        canTouch = false;
        if (nextLevelText != null)
            nextLevelText.text = goToNextLevelText;
        StartCoroutine(StarSequence(star));
    }

    public void GoToNextLevel()
    {
        if (!canTouch) return;
        GameManager.Instance.GoToNextLevel();
    }

    public void SelectLevel()
    {
        if (!canTouch) return;
        GameManager.Instance.GetComponent<Menu>().SelectLevel();
    }

    public void GoToMainMenu()
    {
        if (!canTouch) return;
        GameManager.Instance.GetComponent<Menu>().GoToMainMenu();
    }

    public void Restart()
    {
        if (!canTouch) return;
        GameManager.Instance.Restart();
        FadeAndDestroy();
    }

    private void FadeAndDestroy()
    {
        GetComponent<Animator>().enabled = false;
        GetComponent<CanvasGroup>().DOFade(0f, 0.5f);
        Invoke("DestroyThis", 0.5f);
    }
    private void DestroyThis()
    {
        Destroy(gameObject);
    }

    private IEnumerator StarSequence(int star)
    {
        foreach (var starRect in stars)
            starRect.gameObject.SetActive(false);
        yield return new WaitForSeconds(firstWait);
        for (int i = 0; i < star; i++)
        {
            yield return new WaitForSeconds(afterWait);
            stars[i].gameObject.SetActive(true);
            AudioManager.Instance.PlaySfx(SfxType.Click1);
            stars[i].GetComponentInChildren<ParticleSystem>().Emit(10);
        }
        canTouch = true;
    }
}