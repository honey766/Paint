using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [HideInInspector]
    public Toggle toggle;
    private ToggleManager toggleManager;
    public int toggleNum;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggle);
    }

    public void SetupForManager(ToggleManager toggleManager)
    {
        toggle = GetComponent<Toggle>();
        this.toggleManager = toggleManager;
    }

    public void OnToggle(bool isOn)
    {
        if (toggleManager == null)
            return;
        if (!isOn)
        {
            if (toggleManager.isAllOff())
                toggle.isOn = true;
            return;
        }

        toggleManager.ToggleGroup(toggleNum);
    }

    public void ToggleByManager(bool state)
    {
        toggle.isOn = state;
    }
}