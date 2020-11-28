using UnityEngine;

public class CObjResourcePoolingManager : Singleton<CObjResourcePoolingManager>
{
    protected ResourcePoolingManager mResourcePoolingManager = new ResourcePoolingManager();

    public override GameObject GetSingletonParent()
    {
        return EAMainFrame.instance.gameObject;
    }

    protected override void Awake()
    {
        base.Awake();

        EAMainFrame.instance.OnMainFrameFacilityCreated(MainFrameAddFlags.ObjectPoolManager);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool Destroy()
    {
        mResourcePoolingManager.DeletePoolAll();
        return true;
    }


    /// <summary>
    /// createPool
    /// </summary>
    /// <param name="sKey"></param>
    /// <param name="szPath"></param>
    /// <param name="initCreateCount"></param>
    public void CreatePool(string sKey , string szPath , int initCreateCount)
    {
        if(mResourcePoolingManager.Find(sKey))
        {
            return;
        } 

        GameObject pObject = ResourceManager.instance.Load<GameObject>(szPath);

        if (pObject != null)
        {
            GameObject tObj = GameObject.Instantiate(pObject);
            mResourcePoolingManager.CreatePool(sKey , tObj , gameObject, initCreateCount);
        }
    }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="sKey"></param>
    public void DeletePool(string sKey)
    {
        mResourcePoolingManager.DeletePool(sKey);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    public void Despawn(GameObject go)
    {
        mResourcePoolingManager.Despawn(go);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKey"></param>
    /// <returns></returns>
    public GameObject Spwan(string sKey)
    {
        return mResourcePoolingManager.Spwan(sKey);
    }
}