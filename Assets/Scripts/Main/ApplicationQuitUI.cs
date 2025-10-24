using UnityEngine;

public class ApplicationQuitUI : MonoBehaviour
{
    public void ApplicationQuit()
    {
        Application.Quit();
    }
    
    public void DeleteThis()
    {
        Destroy(gameObject);
    }
}
