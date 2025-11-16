using System;
using UnityEngine;

public class ExtraStageUnlockInformCanvas : MonoBehaviour
{
    public Action onClick;
    public void OnClickButton()
    {
        CharacterSwiper cs = FindAnyObjectByType<CharacterSwiper>();
        cs.DoSnapToExtra();
        onClick?.Invoke();  
    }
}
