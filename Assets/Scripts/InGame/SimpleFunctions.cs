using UnityEngine;

public class SimpleFunctions : MonoBehaviour
{
    public void DestroyThis()
    {
        Destroy(gameObject);
    }

    public void PlayClickSfx()
    {
        AudioManager.Instance.PlaySfx(SfxType.Click1);
    }

    public void GamePlaying()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.isGaming = true;
    }
}
