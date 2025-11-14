#if UNITY_WEBGL
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;

// https://techchat-apps-in-toss.toss.im/t/topic/1169
public class WebGLBGMPlayer : SingletonBehaviour<WebGLBGMPlayer>
{
    [DllImport("__Internal")]
    private static extern void StartObserveVisibility(string goName, string methodName);
        
    private void Start()
    {
        // StartObserveVisibility(GetHierarchyPath(transform), nameof(OnVisibleStatusChanged));
        StartObserveVisibility(gameObject.name, nameof(OnVisibleStatusChanged));
    }

    public string GetHierarchyPath(Transform transform)
    {
        List<string> path = new List<string>();
        Transform current = transform;

        while (current != null)
        {
            path.Add(current.name);
            current = current.parent;
        }

        path.Reverse();
        return string.Join("/", path);
    }
        
    private void OnVisibleStatusChanged(int pauseStatus)
    {
        if (pauseStatus == 1)
        {
            AudioListener.pause = true;
            Time.timeScale = 0f;
            Debug.Log("앱 백그라운드 - 오디오 일시정지");
        }
        else
        {
            AudioListener.pause = false;
            Time.timeScale = 1f;
            Debug.Log("앱 포그라운드 - 오디오 재개");
        }
    }
}
#endif