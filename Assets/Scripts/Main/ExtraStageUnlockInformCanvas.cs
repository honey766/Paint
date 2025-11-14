using UnityEngine;

public class ExtraStageUnlockInformCanvas : MonoBehaviour
{
    public void OnClickButton()
    {
        CharacterSwiper cs = FindAnyObjectByType<CharacterSwiper>();
        cs.DoSnapToExtra();
    }
}
