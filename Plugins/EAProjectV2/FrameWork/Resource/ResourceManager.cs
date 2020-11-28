/*
 purpose:
- Integrated resource management tool for the purpose of replacing the current reshelper
- Delegate and execute Instanciate and destroy
- At this time, apply resource pool to manage memory efficiently.
- Memory pool is used internally by default.
- When using AssetBundles, loads under resouces and loads from assets are handled in the same function.
*/

using System.Collections.Generic;
using UnityEngine;
using Debug = EAFrameWork.Debug;


public class ResourceManager : EAGenericSingleton<ResourceManager> 
{
    private List<GameObject> removeList = new List<GameObject>();
    private Dictionary<int , Queue<GameObject>> objectPool = new Dictionary<int, Queue<GameObject>>();
    public  System.Func<UIManager.UISpawntype, string, GameObject> uiLoadevent = null;
    private static string UI_PATH = "ui/";

    #region Resource management and creation
    private Object Load(string path)
    {
        Object obj = null;

        obj = CoreLoad<Object>(path);

        if (obj != null)
            return obj;

        obj = Resources.Load<Object>(path);

        Debug.Assert(obj != null, "resource Load is null  path : " + path);

        return obj;
    }
       
    public T Load<T>(string path) where T : Object
    {
        T obj = null;

        obj = CoreLoad<T>(path);

        if (obj != null)
            return obj;


        obj = (T)Resources.Load<T>(path);

        Debug.Assert(obj != null, "resource Load is null  path : " + path);

        return obj;
    }

    private T CoreLoad<T>(string path) where T : Object
    {
        T obj = null;

        if (EAAssetBundleLoadModule.instance != null && 
            EAAssetBundleLoadModule.instance.GetUseBundle() == true)
        {
            obj = EAAssetBundleLoadModule.instance.Load<T>(path);

            if (obj != null)
                return obj;

            Debug.Assert(obj != null, "bundle resource Load is null  path : " + path);
        }

#if UNITY_EDITOR

        obj = EAAssetBundleLoadModule.LoadLocal<T>(path);

#endif
        return obj;
    }

    #endregion

    #region Custom object pooling

    public class CustomGameObject : MonoBehaviour
    {
        public int hashKey;
    }

    private GameObject CreateObject(string path)
    {
        GameObject obj = Load<GameObject>(path);

        if (obj != null)
        {
            return GameObject.Instantiate(obj);
        }

        return null;
    }

    public GameObject Create(string path)
    {
        var key = CRC32.GetHashForAnsi(path);

        if (objectPool.ContainsKey(key) == false)
            objectPool.Add(key, new Queue<GameObject>());

        GameObject obj = null;

        if (0 < objectPool[key].Count)
        {
            obj = objectPool[key].Dequeue();
        }

        if (obj == null)
        {
            obj = CreateObject(path);

            CustomGameObject customObject = obj.GetComponent<CustomGameObject>();

            if (customObject == null)
                customObject = obj.AddComponent<CustomGameObject>();

            customObject.hashKey = key;
        }
        
        obj.SetActive(true);

        return obj;
    }

    public GameObject CreateUI(UIManager.UISpawntype spawnType, string uiId)
    {
        if (uiLoadevent != null)
            return uiLoadevent(spawnType, uiId);

        string strPath = $"{UI_PATH}{uiId}";

        return Create(strPath);
    }

    public void Destroy(GameObject obj)
    {
        if (null == obj)
            return;

        removeList.Add(obj);
    }

    public void DestroyImmediate(GameObject obj)
    {
        if (null == obj)
            return;

        CustomGameObject customObject = obj.GetComponent<CustomGameObject>();

        if(customObject == null)
        {
            GameObject.Destroy(obj);
            return;
        }

        if(objectPool.ContainsKey(customObject.hashKey) == false)
        {
            GameObject.Destroy(obj);
            return;
        }

        obj.SetActive(false);

        objectPool[customObject.hashKey].Enqueue(obj);
    }

    public void Update()
    {
        if(removeList.Count > 0)
        {
            foreach(var obj in removeList)
            {
                if (obj != null)
                    DestroyImmediate(obj);
            }

            removeList.Clear();
        } 
    }

    public void Clear()
    {
        Update();
        
        Dictionary<int, Queue<GameObject>>.Enumerator to = objectPool.GetEnumerator();

        while(to.MoveNext())
        {
            var item = to.Current;

            foreach(var obj in item.Value)
            {
                if (obj != null)
                    GameObject.Destroy(obj);
            }

            item.Value.Clear();
        }

        objectPool.Clear();
    }

    public bool GetObjectCached(string path , out Queue<GameObject> objQuue) 
    {
        var key = CRC32.GetHashForAnsi(path);

        if(objectPool.TryGetValue(key, out objQuue))
        {
            return (objQuue.Count > 0);
        }

        return false;
    }
    

    #endregion
}
