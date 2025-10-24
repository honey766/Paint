using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Menu : MonoBehaviour
{
    [SerializeField] bool isGaming;
    [SerializeField] float rotateDuration;

    public void Resume()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        if (isGaming) GameManager.Instance.Resume();
        Destroy(gameObject);
    }

    public void Restart()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        if (isGaming) GameManager.Instance.Restart();
        Destroy(gameObject);
    }

    public void SelectLevel()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        if (isGaming) GameManager.Instance.SelectLevel();
    }

    public void OpenSetting()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        UIManager.Instance.OpenSettings();
    }

    public void OpenShop()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
    }

    public void GoToMainMenu()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("Main"));
    }
}
