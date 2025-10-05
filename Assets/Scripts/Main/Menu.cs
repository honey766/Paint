using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Menu : MonoBehaviour
{
    [SerializeField] bool isGaming;
    [SerializeField] float rotateDuration;

    public void Resume()
    {
        if (isGaming) GameManager.Instance.Resume();
        Destroy(gameObject);
    }

    public void Restart()
    {
        if (isGaming) GameManager.Instance.Restart();
        Destroy(gameObject);
    }

    public void SelectLevel()
    {
        if (isGaming) GameManager.Instance.SelectLevel();
    }

    public void OpenSetting()
    {
        UIManager.Instance.OpenSettings();
    }

    public void OpenShop()
    {

    }

    public void GoToMainMenu()
    {
        UIManager.Instance.ScreenTransition(() => SceneManager.LoadScene("Main"));
    }
}
