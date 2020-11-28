using UnityEngine;
using System.Collections.Generic;


public class ResourcePoolingManager 
{
    Dictionary<int, CObjectPool<GameObject>> poolList = new Dictionary<int, CObjectPool<GameObject>>();
    Dictionary<int, GameObject> poolObject            = new Dictionary<int, GameObject>();

    GameObject parent = null;

    public ResourcePoolingManager() 
    {    
    }

    /// <summary>
    /// Add pooling to that type. Return value is true only when newly added.
    /// </summary>
    public int CreatePool(string sKey ,GameObject pPoolingTarget, GameObject _parent, int nInitCreateCount  = 1, int nExpandCreateCount = 1 )
    {
        parent = _parent;

        int key = CRC32.GetHashForAnsi(sKey);

        GameObject go = null;

        pPoolingTarget.transform.SetParent(parent.transform);
        pPoolingTarget.SetActive(false);

        if (!poolObject.TryGetValue(key,out go))
        {
            poolObject.Add(key, pPoolingTarget);
        }  
        else
        {
            poolObject[key] = pPoolingTarget;
        }

        CObjectPool<GameObject> objectPool = null;

        if (!poolList.TryGetValue(key, out objectPool))
        {
            objectPool = new CObjectPool<GameObject>()
            {
                InitializeCreateCount = nInitCreateCount,
                ExpandCreateCount     = nExpandCreateCount,

                OnObjectDestroy = (GameObject v) =>
                {
                    GameObject.Destroy(v);
                },
                OnTake = (GameObject v) =>
                {
                },
                OnRelease = (GameObject v) =>
                {
                    if (v)
                    {
                        v.transform.SetParent(parent.transform);
                        v.SetActive(false);
                    }
                },
                OnCreate = () =>
                {
                    var data = EAFrameUtil.AddChild(parent , poolObject[key] );
                    return data;
                }
            };

            poolList.Add(key, objectPool);
        }

        return key;
    }

    /// <summary>
    /// Check if it's already in the pool
    /// </summary>
    /// <param name="sKey"></param>
    /// <returns></returns>
    public bool Find(string sKey)
    {
        GameObject go = null;

        int key = CRC32.GetHashForAnsi(sKey);

        return poolObject.TryGetValue(key, out go);
    }  
   
    /// <summary>
    /// The return value is when the key registered in m_mpPoolingType is deleted.
    /// </summary>
    /// <param name="eType"></param>
    /// <returns></returns>
    public bool DeletePool(string sKey)
    {
        int key = CRC32.GetHashForAnsi(sKey);

        GameObject go = null;

        if(poolObject.TryGetValue(key, out go))
        {
            GameObject.Destroy(go);
        }   

        poolObject.Remove(key);

        CObjectPool<GameObject> objectPool = null;

        if (poolList.TryGetValue(key, out objectPool))
        {
            objectPool.Dispose();
            poolList.Remove(key);
        }

        return true;
    }
  
    /// <summary>
    /// 
    /// </summary>
    public void DeletePoolAll()
    {
        foreach (KeyValuePair<int, GameObject> v in poolObject)
        {
            if(v.Value != null)
            {
                GameObject.Destroy(v.Value);
            }  
        }

        foreach (KeyValuePair<int, CObjectPool<GameObject>> v in poolList)
        {
            v.Value.Dispose();
        }

        poolObject.Clear();
        poolList.Clear();
    }
    
        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="eType"></param>
    /// <returns></returns>
    public GameObject Spwan(string sKey)
    {
        CObjectPoolWrapper<GameObject> obj = null;

        CObjectPool<GameObject> objectPool = null;

        int key = CRC32.GetHashForAnsi(sKey);

        if (poolList.TryGetValue(key, out objectPool))
        {
            obj = objectPool.Take();

            PoolObject info = obj.Value.GetComponent<PoolObject>();

            if (info == null)
            {
                info = obj.Value.AddComponent<PoolObject>();
            }

            info.obj     = obj;
            info.hashKey = key;

            obj.Value.transform.parent = null;
            obj.Value.SetActive(true);

            return obj.Value;
        }

        return null;
    }

    public void Despawn(GameObject go)
    {
        if (go == null)
            return;

        PoolObject info = go.GetComponent<PoolObject>();

        if (info != null)
        {
            CObjectPool<GameObject> objectPool = null;
            int hashkey = info.hashKey;

            if (poolList.TryGetValue(hashkey, out objectPool))
            {
                objectPool.Release(info.obj);
            }
        }
        else
        {
            // Remove if not pool object
            GameObject.Destroy(go);
        } 
    }
}