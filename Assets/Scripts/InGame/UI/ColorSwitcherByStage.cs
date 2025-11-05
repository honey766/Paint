using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorSwitcherByStage : MonoBehaviour
{
    Color white = Color.white;
    Color gray = new Color(0.3764706f, 0.3921569f, 0.4f, 1f);

    private void Start()
    {
        Color myColor;
        if (PersistentDataManager.Instance.stage == 6)
            myColor = white;
        else
            myColor = gray;

        if (TryGetComponent(out Image image)) image.color = myColor;
        if (TryGetComponent(out TextMeshProUGUI txt)) txt.color = myColor;
    }
}
