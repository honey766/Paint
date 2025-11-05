using UnityEngine;
using UnityEngine.UI;

public class BackgroundImageLoader : MonoBehaviour
{
    void Start()
    {
        Sprite backgroundImg = Resources.Load<Sprite>($"Images/ingame_back_img_{PersistentDataManager.Instance.stage}");
        if (backgroundImg != null)
            GetComponent<Image>().sprite = backgroundImg;
        else
            Logger.LogWarning($"Images/ingame_back_img_{PersistentDataManager.Instance.stage} Do not exists");
    }
}
