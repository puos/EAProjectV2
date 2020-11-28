#region Using Statements
using UnityEngine;
#endregion


public class CEffectResourcePoolingManager : Singleton<CEffectResourcePoolingManager>
{
    protected ResourcePoolingManager mResourcePoolingManager = new ResourcePoolingManager();

    public override GameObject GetSingletonParent()
    {
        return EAMainFrame.instance.gameObject;
    }

    protected override void Awake()
    {
        base.Awake();

        EAMainFrame.instance.OnMainFrameFacilityCreated(MainFrameAddFlags.SfxPoolManager);
    }
    
    public bool Destroy()
    {
        mResourcePoolingManager.DeletePoolAll();

        return true;
    }

    /// <summary>
    ///  create pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="szPath"></param>
    /// <param name="ePoolType"></param>
    public void CreatePool(string sKey, string szPath, int initCreateCount)
    {
        if (mResourcePoolingManager.Find(sKey))
        {
            return;
        }

        GameObject pObject = ResourceManager.instance.Load<GameObject>(szPath);

        if (pObject != null)
        {
            GameObject tObj = GameObject.Instantiate(pObject);
            mResourcePoolingManager.CreatePool(sKey, tObj, gameObject, initCreateCount);
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