using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EditorDropdownController : MonoBehaviour
{
    public TMP_InputField inputField;

    private Transform[] objectsControlledByDropdown;
    [SerializeField] private Transform dropdownTileParent;
    [Header("Spray")]
    [SerializeField] private Toggle sprayInfiniteToggle;
    [Header("DirectedSpray")]
    [SerializeField] private Toggle directedSprayInfiniteToggle;
    [SerializeField] private Toggle directedSprayReverseToggle;
    private Transform directedSprayTriangle;
    private Image directedSprayTriangleImage;
    public Vector2Int directedSprayDirection { get; private set; }
    public bool directedSprayDoPaintReverse { get; private set; }

    private ToggleManager toggle;

    private void Start()
    {
        toggle = ToggleManager.Instance;

        objectsControlledByDropdown = transform.Cast<Transform>().ToArray();
        directedSprayTriangle = transform.GetChild(1).GetChild(0).GetChild(1).GetChild(0);
        directedSprayTriangleImage = directedSprayTriangle.GetComponent<Image>();

        DropdownObjectsInitAndSetActive(0);
    }

    public void DropdownOnValueChanged(int index)
    {
        switch (index)
        {
            case 0:
                toggle.ToggleGroup(TileEditingTool.AddSpray);
                inputField.gameObject.SetActive(true);
                break;
            case 1:
                toggle.ToggleGroup(TileEditingTool.AddDirectedSpray);
                inputField.gameObject.SetActive(true);
                break;
            case 2:
                toggle.ToggleGroup(TileEditingTool.Temp2);
                inputField.gameObject.SetActive(false);
                break;
        }
        DropdownObjectsInitAndSetActive(index);
    }

    public void DropdownObjectsInitAndSetActive(int index)
    {
        InitSpray();
        InitDirectedSpray();
        inputField.interactable = true;

        for (int i = 0; i < objectsControlledByDropdown.Length; i++)
        {
            if (index == i)
            {
                objectsControlledByDropdown[i].SetParent(dropdownTileParent);
                objectsControlledByDropdown[i].GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            }
            else
            {
                objectsControlledByDropdown[i].SetParent(transform);
            }
        }
    }

    private void InitSpray()
    {
        sprayInfiniteToggle.isOn = false;
    }
    private void InitDirectedSpray()
    {
        directedSprayInfiniteToggle.isOn = false;
        directedSprayReverseToggle.isOn = false;
        directedSprayTriangle.rotation = Quaternion.identity;
        directedSprayTriangleImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        directedSprayDirection = Vector2Int.up;
        directedSprayDoPaintReverse = false;
    }

    public void SprayInfiniteToggleOnValueChanged(bool isOn)
    {
        toggle.ToggleGroup(TileEditingTool.AddSpray);
        inputField.interactable = !isOn;
        if (isOn) inputField.text = "-1";
    }

    public void DirectedSprayInfiniteToggleOnValueChanged(bool isOn)
    {
        toggle.ToggleGroup(TileEditingTool.AddDirectedSpray);
        inputField.interactable = !isOn;
        if (isOn) inputField.text = "-1";
    }

    public void DirectedSprayRotation()
    {
        switch (directedSprayDirection)
        {
            case Vector2Int v when v == Vector2Int.up:
                directedSprayDirection = Vector2Int.right;
                break;
            case Vector2Int v when v == Vector2Int.right:
                directedSprayDirection = Vector2Int.down;
                break;
            case Vector2Int v when v == Vector2Int.down:
                directedSprayDirection = Vector2Int.left;
                break;
            case Vector2Int v when v == Vector2Int.left:
                directedSprayDirection = Vector2Int.up;
                break;
        }
        directedSprayTriangle.rotation = CustomTools.GetRotationByDirection(directedSprayDirection);
    }

    public void DirectedSprayReverseToggleOnValueChanged(bool isOn)
    {
        toggle.ToggleGroup(TileEditingTool.AddDirectedSpray);
        directedSprayDoPaintReverse = isOn;
        directedSprayTriangleImage.color = isOn ? new Color(0.85f, 0.63f, 0.85f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
    }
}
