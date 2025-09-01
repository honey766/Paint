using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class CustomTools : Editor
{
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
}
#endif