using UnityEngine;
public class AppearOnDesktop : MonoBehaviour
{    
    #if !UNITY_STANDALONE  
    private void Awake()
    {
        Destroy(gameObject);
    }
    #endif
}