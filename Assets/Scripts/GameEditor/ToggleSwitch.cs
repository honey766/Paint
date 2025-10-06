#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [HideInInspector]
    public Toggle toggle;
    public TileEditingTool tool;
    private ToggleManager toggleManager;

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

        toggleManager.ToggleGroup(tool);
    }

    public void ToggleByManager(bool state)
    {
        if (gameObject.activeInHierarchy)
            toggle.isOn = state;
    }
}
#endif