using UnityEngine;

public class SimpleFunctions : MonoBehaviour
{
    [SerializeField] private GameObject instantiatePrefab;

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

    public void InstantiatePrefab()
    {
        if (instantiatePrefab != null)
            Instantiate(instantiatePrefab);
    }
}
