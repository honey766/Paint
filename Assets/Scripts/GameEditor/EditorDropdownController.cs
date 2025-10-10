#if UNITY_EDITOR
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;

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

    [Header("Mirror")]
    [SerializeField] private Transform mirrorImage;
    private bool isMirrorBottomLeftToTopRight;

    [Header("Brush")]
    [SerializeField] private Image brushImage;
    [SerializeField] private Toggle brushWhiteToggle;
    [SerializeField] private Toggle brushColor1Toggle, brushColor2Toggle;
    private Toggle[] brushToggle;

    private ToggleManager toggle;

    private void Start()
    {
        toggle = ToggleManager.Instance;

        objectsControlledByDropdown = transform.Cast<Transform>().ToArray();
        directedSprayTriangle = transform.GetChild(1).GetChild(0).GetChild(1).GetChild(0);
        directedSprayTriangleImage = directedSprayTriangle.GetComponent<Image>();
        brushToggle = new Toggle[] { brushWhiteToggle, brushColor1Toggle, brushColor2Toggle };

        DropdownObjectsInitAndSetActive(0);
    }

    public void DropdownOnValueChanged(int index)
    {
        inputField.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 80);
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
                toggle.ToggleGroup(TileEditingTool.AddIce);
                inputField.gameObject.SetActive(false);
                break;
            case 3:
                toggle.ToggleGroup(TileEditingTool.AddMirror);
                inputField.gameObject.SetActive(true);
                inputField.text = "1";
                inputField.interactable = false;
                break;
            case 4:
                toggle.ToggleGroup(TileEditingTool.AddBrush);
                inputField.gameObject.SetActive(true);
                inputField.interactable = false;
                OnBrushToggleClicked(0);
                inputField.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                break;
            case 5:
                toggle.ToggleGroup(TileEditingTool.AddJustBlock);
                inputField.gameObject.SetActive(false);
                break;
        }
        DropdownObjectsInitAndSetActive(index);
    }

    public void DropdownObjectsInitAndSetActive(int index)
    {
        if (index != 3)
            inputField.interactable = true;
        InitSpray();
        InitDirectedSpray();
        InitMirror(); 

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
    private void InitMirror()
    {
        isMirrorBottomLeftToTopRight = true;
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

    public void MirrorButton()
    {
        toggle.ToggleGroup(TileEditingTool.AddMirror);
        isMirrorBottomLeftToTopRight = !isMirrorBottomLeftToTopRight;
        inputField.text = isMirrorBottomLeftToTopRight ? "1" : "0";
        mirrorImage.rotation = isMirrorBottomLeftToTopRight ?
                               Quaternion.identity : Quaternion.Euler(new Vector3(0, 0, 90));
    }

    public void OnBrushToggleClicked(int idx)
    {
        toggle.ToggleGroup(TileEditingTool.AddBrush);
        if (!brushToggle[idx].isOn) return;
        for (int i = 0; i < 3; i++)
            if (i != idx) 
                brushToggle[i].isOn = false;

        inputField.text = idx.ToString();
        if (idx == 0)
            brushImage.color = Color.white;
        else if (idx == 1)
            brushImage.color = new Color(1, 0.45f, 0.41f, 1);
        else if (idx == 2)
            brushImage.color = new Color(0.24f, 0.57f, 1, 1);
    }
}
#endif