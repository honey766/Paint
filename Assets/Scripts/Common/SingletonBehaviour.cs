using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    //씬 전환 시 삭제할지 여부
    protected bool m_IsDestroyOnLoad = false;

    //이 클래스의 스태틱 인스턴스 변수
    protected static T m_Instance;

    public static T Instance
    {
        get { return m_Instance; }
    }

    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        if(m_Instance == null)
        {
            m_Instance = (T)this;

            if(!m_IsDestroyOnLoad)
            {
                DontDestroyOnLoad(this);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
