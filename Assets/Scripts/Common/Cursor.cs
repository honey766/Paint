using UnityEngine;

public class Cursor : MonoBehaviour
{
    #if UNITY_STANDALONE
    Transform cursorUI;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        cursorUI = transform.GetChild(0);
    }
    private void LateUpdate()
    {
        cursorUI.position = Input.mousePosition;
    }
    #else
    private void Start()
    {
        Destroy(gameObject);
    }
    #endif
}