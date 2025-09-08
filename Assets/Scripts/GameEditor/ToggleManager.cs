using UnityEngine;
using System.Collections.Generic;

public enum TileEditingTool
{
    None,
    GenerateTile,
    ChangeTileColor1, ChangeTileColor2, ChangeTileColor12,
    DeleteTile,
    SetStartPos,
    AddColor1Paint, AddColor2Paint
}

public class ToggleManager : SingletonBehaviour<ToggleManager>
{
    public TileEditingTool tool;
    private Dictionary<TileEditingTool, ToggleController> toggleMap = new Dictionary<TileEditingTool, ToggleController>();

    private void Awake()
    {
        m_IsDestroyOnLoad = true;
        Init();

        ToggleController[] toggles = GetComponentsInChildren<ToggleController>();
        foreach (var toggle in toggles)
            toggleMap[toggle.tool] = toggle;

        Setup();
    }

    private void Setup()
    {
        foreach (var toggle in toggleMap.Values)
        {
            toggle.SetupForManager(this);
            toggle.ToggleByManager(false);
        }
        toggleMap[TileEditingTool.GenerateTile].ToggleByManager(true);
        tool = TileEditingTool.GenerateTile;
    }

    public void ToggleGroup(TileEditingTool tool)
    {
        this.tool = tool;
        foreach (var toggle in toggleMap.Values)
        {
            if (toggle.tool == tool)
                toggle.ToggleByManager(true);
            else
                toggle.ToggleByManager(false);
        }
    }

    public bool isAllOff()
    {
        foreach (var toggleController in toggleMap.Values)
        {
            if (toggleController.toggle.isOn)
                return false;
        }
        return true;
    }
}