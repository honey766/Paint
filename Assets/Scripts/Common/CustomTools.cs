using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomTools : Editor
{
#if UNITY_EDITOR
    [MenuItem("Inflearn/Add User Gem (+10)")]
    public static void AddUserGem()
    {
        var Gem = long.Parse(PlayerPrefs.GetString("Gem"));
        Gem += 10;

        PlayerPrefs.SetString("Gem", Gem.ToString());
        PlayerPrefs.Save();
    }

    [MenuItem("Inflearn/Add User Gold (+100)")]
    public static void AddUserGold()
    {
        var Gold = long.Parse(PlayerPrefs.GetString("Gold"));
        Gold += 100;

        PlayerPrefs.SetString("Gold", Gold.ToString());
        PlayerPrefs.Save();
    }
#endif

    public static Quaternion GetRotationByDirection(Vector2Int direction)
    {
        switch (direction)
        {
            case Vector2Int v when v == Vector2Int.up:
                return Quaternion.identity;
            case Vector2Int v when v == Vector2Int.right:
                return Quaternion.Euler(new Vector3(0, 0, -90));
            case Vector2Int v when v == Vector2Int.down:
                return Quaternion.Euler(new Vector3(0, 0, 180));
            case Vector2Int v when v == Vector2Int.left:
                return Quaternion.Euler(new Vector3(0, 0, 90));
        }
        return Quaternion.identity;
    }
}