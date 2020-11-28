using UnityEngine;
using Debug = EAFrameWork.Debug;

static class EA_CEffectResourceLoader
{
    public static bool EffectSetting(EA_CEffectNode pEffectNode,EA_EffectBaseInfo effectinfo)
    {
        if (null == pEffectNode)
        {
            Debug.Log("pEffectNode is null : " + effectinfo.m_GDEffectId);
            return false;
        }


        if (eEffectState.ES_Load != effectinfo.m_eEffectState)
        {
            return false;
        }

        //	Object basic information
        string sPoolType = effectinfo.m_EffectTableIndex;

        GameObject pGameObject = null;

        switch (effectinfo.m_eAttachType)
        {
            case eEffectAttachType.eWorld:
                {
                    pGameObject = CEffectResourcePoolingManager.instance.Spwan(sPoolType);

                    if(pGameObject != null)
                    {
                        pGameObject.transform.position = Vector3.zero;
                    } 
                }
                break;

            case eEffectAttachType.eLinkOffset:
                {
                    EA_CObjectBase pObjectBase = EACObjManager.instance.GetGameObject(effectinfo.m_AttachObjectId);

                    if (pObjectBase != null && pObjectBase.GetLinkEntity() != null)
                    {
                        GameObject IEntity = pObjectBase.GetLinkEntity();
                        
                        pGameObject = CEffectResourcePoolingManager.instance.Spwan(sPoolType);

                        if(pGameObject != null)
                        {
                            pGameObject.transform.parent = IEntity.transform;

                            pGameObject.transform.localPosition = Vector3.zero;
                            pGameObject.transform.localRotation = Quaternion.identity;
                        }
                    }
                }
                break;
            case eEffectAttachType.eLinkBone:
                {
                    EA_CObjectBase pObjectBase = EACObjManager.instance.GetGameObject(effectinfo.m_AttachObjectId);

                    if (pObjectBase != null) //  [4/11/2014 puos]                        attach to the actor bone
                    {
                        GameObject pBone = pObjectBase.GetObjectInActor(effectinfo.m_AttachBoneName);

                        if (pBone != null)
                        {
                            pGameObject = CEffectResourcePoolingManager.instance.Spwan(sPoolType);

                            if (pGameObject != null)
                            {
                                pGameObject.transform.parent = pBone.transform;
                                pGameObject.transform.localPosition = Vector3.zero;
                                pGameObject.transform.localRotation = Quaternion.identity;
                            }
                        }
                    }
               }
               break;
        }

        if (pGameObject == null)
        {
            Debug.Log("EffectSetting gameobject is null : " + effectinfo.m_GDEffectId);
            return false;
        }

        if (pGameObject.GetComponent<EAEffectModule>() == null)
        {
            pGameObject.AddComponent<EAEffectModule>();
        }

        pEffectNode.SetLinkEffect(pGameObject.GetComponent<EAEffectModule>());

        return true;
    }

    public static bool EffectUnSetting(EA_CEffectNode pDelEffectNode)
    {
        if (pDelEffectNode != null)
        {
            if (pDelEffectNode.GetEffectBaseInfo().m_eEffectState == eEffectState.ES_Load)
            {
                Debug.Log("EffectUnSetting error : " + pDelEffectNode.GetEffectBaseInfo().m_GDEffectId);
                return false;
            }

            if (pDelEffectNode.GetLinkEffect() != null)
            {
                pDelEffectNode.GetLinkEffect().transform.parent = null;

               CEffectResourcePoolingManager.instance.Despawn(pDelEffectNode.GetLinkEffect().gameObject);
            }
            
            pDelEffectNode.SetLinkEffect(null);

        }
        return false;
    }
}
