using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameClearCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform[] stars;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    [SerializeField] private float firstWait = 0.2f, afterWait = 0.4f;
    [SerializeField] private GameObject[] visibleObjs;
    [SerializeField] private Image eyeButtonImg;
    [SerializeField] private Sprite eyeButtonVisibleSpr, eyeButtonInVisibleSpr;
    
    private bool canTouch;
    private bool isVisible; // UI가 보이는 상태인지

    public void Init(int star, string goToNextLevelText)
    {
        canTouch = false;
        isVisible = true;

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

    public void ToggleVisible()
    {
        if (!canTouch) return;

        isVisible = !isVisible;
        foreach (GameObject obj in visibleObjs)
            obj.SetActive(isVisible);

        eyeButtonImg.sprite = isVisible ? eyeButtonVisibleSpr : eyeButtonInVisibleSpr;
        eyeButtonImg.color = isVisible ? Color.white : new Color(0.7509433f, 0.7904252f, 0.8126814f, 1f);
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
        GameObject hint = GameObject.Find("Hint(Clone)");
        if (hint != null) Destroy(hint);
        canTouch = true;
    }
}