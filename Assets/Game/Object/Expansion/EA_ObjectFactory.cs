using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = EAFrameWork.Debug;

using EAObjID = System.UInt32;
using EATblID = System.UInt32;

/// <summary>
/// Create game object and register in pool
/// </summary>
static class EA_ObjectFactory
{
    public static bool EntitySetting(EA_CObjectBase pSetObject,  ObjectInfo SetObjinfo )
	{
        if (null == pSetObject)
        {
            Debug.Log("EntitySetting pSetObject is null :" + SetObjinfo.m_strGameName);
            return false;
        }
        
        if (eObjectState.CS_SETENTITY != SetObjinfo.m_eObjState)
        {
            return false;
        }
		
	     string poolType = SetObjinfo.m_ModelTypeIndex;

        GameObject pGameObject = CObjResourcePoolingManager.instance.Spwan(poolType); 
        pSetObject.SetLinkEntity(pGameObject);

        if(pGameObject == null)
        {
            Debug.LogError("EntitySetting Game object is invalid. type : " + poolType + ", name : " + pSetObject.GetObjInfo().m_strGameName);
            return false;
        }

        // Modify class creation logic [3/30/2018 puos]
        if ( SetObjinfo.m_objClassType != default(Type))
        {
            if (pGameObject.GetComponent(SetObjinfo.m_objClassType) == null)
                 pGameObject.AddComponent(SetObjinfo.m_objClassType);
        }

        //	Create around object table
        switch (SetObjinfo.m_eObjType)
        {
            case eObjectType.CT_NPC:
            case eObjectType.CT_MONSTER:
            case eObjectType.CT_PLAYER:
            case eObjectType.CT_MYPLAYER:
                {
                    EAActor actor = pGameObject.GetComponent<EAActor>();

                    if (actor == null)
                    {
                        actor = pGameObject.AddComponent<EAActor>();
                        SetObjinfo.m_objClassType = typeof(EAActor);
                    }  
                    
                    ((EA_CCharBPlayer)pSetObject).SetLinkActor(actor);
                }
                break;

            case eObjectType.CT_MAPOBJECT:
                {
                    EAMapObject mapObject = pGameObject.GetComponent<EAMapObject>();

                    if (mapObject == null)
                    {
                        mapObject = pGameObject.AddComponent<EAMapObject>();
                        SetObjinfo.m_objClassType = typeof(EAMapObject);
                    }
                   
                    ((EA_CMapObject)pSetObject).SetLinkMapObject(mapObject);
                }
                break;
            case eObjectType.CT_ITEMOBJECT:
                {
                    EAItem itemObject = pGameObject.GetComponent<EAItem>();

                    if (itemObject == null)
                    {
                        itemObject = pGameObject.AddComponent<EAItem>();
                        SetObjinfo.m_objClassType = typeof(EAItem);
                    } 
                  
                    ((EA_CItem)pSetObject).SetLinkItem(itemObject);
                }
                break;
        }

        //Debug.Log("EntitySetting pSetObject :" + SetObjinfo.m_ModelTypeIndex);

        return true;
	}

    public static bool EntityUnSetting(EA_CObjectBase pDelObjBase)
	{
		if( pDelObjBase != null )
		{
            if (pDelObjBase.GetObjInfo().m_eObjState ==  eObjectState.CS_SETENTITY)
            {
                //  [3/6/2014 puos] entity unsetting error check
                Debug.Log("EntityUnSetting error : " + pDelObjBase.GetObjInfo().m_strGameName);
                return false;
            }

            GameObject pGameObject = pDelObjBase.GetLinkEntity();

            if(pGameObject == null)
            {
                Debug.Log("EntityUnSetting gameobject is null : " + pDelObjBase.GetObjInfo().m_strGameName);
                return false;
            }

            // puos 20141019 null parent set
            pGameObject.transform.parent = null;

            CObjResourcePoolingManager.instance.Despawn(pDelObjBase.GetLinkEntity());

            pDelObjBase.SetLinkEntity(null);

            //Debug.Log("EntityUnSetting pSetObject :" + pDelObjBase.GetObjInfo().m_strGameName);
        }
		return true;
	}
}