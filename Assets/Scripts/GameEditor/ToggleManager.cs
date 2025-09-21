using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public enum TileEditingTool
{
    None,
    GenerateTile,
    ChangeTileColor1, ChangeTileColor2, ChangeTileColor12, ChangeTileColorBlack,
    DeleteTile,
    SetStartPos,
    AddTarget,
    AddColor1Paint, AddColor2Paint, AddReversePaint,
    AddSpray,
    Temp1, Temp2
}

public class ToggleManager : SingletonBehaviour<ToggleManager>
{
    public TileEditingTool tool;
    public Transform[] objectsControlledByDropdown;
    public Transform dropdownTileStorage, dropdownTileParent;
    public TMP_InputField inputField;
    private Dictionary<TileEditingTool, ToggleController> toggleMap = new Dictionary<TileEditingTool, ToggleController>();

    private void Awake()
    {
        m_IsDestroyOnLoad = true;
        Init();

        ToggleController[] toggles = GetComponentsInChildren<ToggleController>(true);
        foreach (var toggle in toggles)
            toggleMap[toggle.tool] = toggle;

        DropdownObjectsSetActive(0);
    }

    private void Start()
    {
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
            if (toggleController.gameObject.activeInHierarchy && toggleController.toggle.isOn)
                return false;
        }
        return true;
    }

    public void DropdownOnValueChanged(int index)
    {
        switch (index)
        {
            case 0:
                ToggleGroup(TileEditingTool.AddSpray);
                inputField.gameObject.SetActive(true);
                break;
            case 1:
                ToggleGroup(TileEditingTool.Temp1);
                inputField.gameObject.SetActive(false);
                break;
            case 2:
                ToggleGroup(TileEditingTool.Temp2);
                inputField.gameObject.SetActive(true);
                break;
        }
        DropdownObjectsSetActive(index);
    }

    private void DropdownObjectsSetActive(int index)
    {
        for (int i = 0; i < objectsControlledByDropdown.Length; i++)
        {
            if (index == i)
            {
                objectsControlledByDropdown[i].SetParent(dropdownTileParent);
                objectsControlledByDropdown[i].GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            }
            else
            {
                objectsControlledByDropdown[i].SetParent(dropdownTileStorage);
            }
        }
    }

    public void SprayToggleOnValueChanged(bool isOn)
    {
        ToggleGroup(TileEditingTool.AddSpray);
        inputField.interactable = !isOn;
        if (isOn) inputField.text = "-1";
    }
}