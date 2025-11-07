using UnityEngine;

public class ApplicationQuitUI : MonoBehaviour
{
    public void ApplicationQuit()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        Application.Quit();
    }
    
    public void Disable()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
        gameObject.SetActive(false);
    }
}
