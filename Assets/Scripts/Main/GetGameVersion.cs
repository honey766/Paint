using UnityEngine;
using TMPro;

public class GetGameVersion : MonoBehaviour
{
    void Start()
    {
        string gameVersion = Application.version;
        GetComponent<TextMeshProUGUI>().text = gameVersion;
    }
}