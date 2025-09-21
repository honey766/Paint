using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "ScriptableObjects/ColorPalette", order = 1)]
public class ColorPaletteSO : ScriptableObject
{
    public Color color1 = new Color(0, 0, 0, 1);
    public Color color2 = new Color(0, 0, 0, 1);
    public Color color12 = new Color(0, 0, 0, 1); // color1 + color2
}