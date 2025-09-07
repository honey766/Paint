using UnityEngine;

public class ToggleManager : SingletonBehaviour<ToggleManager>
{
    public ToggleController[] toggles;
    public int toggleNum;

    private void Awake()
    {
        m_IsDestroyOnLoad = true;
        Init();
        
        toggles = GetComponentsInChildren<ToggleController>();
        Setup();
    }

    private void Setup()
    {
        foreach (var toggle in toggles)
        {
            toggle.SetupForManager(this);
            toggle.ToggleByManager(false);
        }
        toggles[0].ToggleByManager(true);
        toggleNum = 0;
    }

    public void ToggleGroup(int toggleNum)
    {
        this.toggleNum = toggleNum;
        foreach (var toggle in toggles)
        {
            if (toggle.toggleNum == toggleNum)
                toggle.ToggleByManager(true);
            else
                toggle.ToggleByManager(false);
        }
    }

    public bool isAllOff()
    {  
        foreach (var toggleController in toggles)
        {
            if (toggleController.toggle.isOn)
                return false;
        }
        return true;
    }
}