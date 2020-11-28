using UnityEngine;
using Debug = EAFrameWork.Debug;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    protected static T m_instance;

    /// <summary>
    ///  DontDestroyOnLoad is not used if there is a parent object.
    /// </summary>
    public virtual GameObject GetSingletonParent()
    {
        return null;
    }

    public static T instance
    {
        get
        {
            if ( m_instance == null)
            {
                CreateInstance();
            }

            return m_instance;
        }
    }

    static public void CreateInstance()
    {
        if (m_instance)
        {
            Debug.Log(typeof(T).Name + " has already created.");
            return;
        }

        GameObject go = new GameObject(typeof(T).Name, typeof(T));
        m_instance = go.GetComponent<T>();

        GameObject parentGo = m_instance.GetSingletonParent();

        if (parentGo != null)
            go.transform.parent = parentGo.transform;
        else
            DontDestroyOnLoad(go);
    }

    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (null == m_instance)
        {
            m_instance = this as T;
        }
    }

    virtual protected void Start()
    {
        
    }

    virtual protected void OnEnable()
    {
        
    }

    virtual protected void OnDisable()
    {
       
    }
}
