using UnityEngine;
using System.Collections;

using Debug = EAFrameWork.Debug;
using EAObjID = System.UInt32;

public class EAItem : MonoBehaviour
{
    protected virtual void OnCreate() { }

    protected virtual void OnInit() { }

    protected virtual void OnUpdate() { }

    protected virtual void OnClose() { }

    protected virtual void OnDecay() { }

    public virtual bool Use()
    {
        return true;
    }

    private void Awake()
    {
        OnCreate();
    }

    private void OnDestroy()
    {
        OnDecay();
    }

    public void SpawnAction()
    {
        OnInit();
    }
        
    public  void DeSpawnAction()
    {
        OnClose();
    }

    public void SetItemBase(EA_CItem pDiaItemBase)
    {
        m_pDiaItemBase = pDiaItemBase;
    }

    public EA_CItem GetItemBase()
    {
        return m_pDiaItemBase;
    }
        
    public GameObject GetOwner()
    {
        if (GetItemBase() != null)
        {
            EAObjID _objID = GetItemBase().GetItemInfo().m_HavenUser;

            EA_CObjectBase pObjectBase = EACObjManager.instance.GetGameObject(_objID);

            if (pObjectBase != null)
            {
                return pObjectBase.GetLinkEntity();
            }
        }

        return null;
    }

    public EAActor GetOwnerActor()
    {
        if (GetItemBase() != null)
        {
            EAObjID _objID = GetItemBase().GetItemInfo().m_HavenUser;

            EA_CCharBPlayer pActor = (EA_CCharBPlayer)EACObjManager.instance.GetActor(_objID);

            if (pActor != null && pActor.GetLinkIActor() != null)
            {    
                return pActor.GetLinkIActor();
            }
        }

        return null;
    }

    public virtual void OnAction(params object[] parms)
    {
    }

    public GameObject GetObjectInItem(string strObjectName)
    {
        return GetObjectInItem(transform.gameObject, strObjectName);
    }

    protected GameObject GetObjectInItem(GameObject pObject, string strObjectName)
    {
        for (int nCnt = 0; nCnt < pObject.transform.childCount; ++nCnt)
        {
            Transform child = pObject.transform.GetChild(nCnt);

            if (child.gameObject != null && child.gameObject.name == strObjectName)
            {
                return child.gameObject;
            }

            GameObject pFindObject = GetObjectInItem(child.gameObject, strObjectName);

            if (pFindObject != null)
            {
                return pFindObject;
            }
        }

        return null;
    }

   
    /// <summary>
    /// update function
    /// </summary>
    void Update()
    {
        OnUpdate();    
    }

    protected EA_CItem m_pDiaItemBase = null;
}
