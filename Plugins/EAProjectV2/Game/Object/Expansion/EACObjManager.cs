using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = EAFrameWork.Debug;

using EAObjID = System.UInt32;
using EATblID = System.UInt32;

public class EACObjManager : EAGenericSingleton<EACObjManager>
{
  
    //--------------------------------------------------------------------------
	Dictionary<EAObjID,EA_CCharUser>	 m_mapPlayerList   = new Dictionary<EAObjID,EA_CCharUser>();
	Dictionary<EAObjID,EA_CCharMob>	     m_mapMonsterList  = new Dictionary<EAObjID,EA_CCharMob>();
	Dictionary<EAObjID,EA_CCharNPC>	     m_mapNPCList      = new Dictionary<EAObjID,EA_CCharNPC>();
    Dictionary<EAObjID, EA_CItem>        m_mapItemList     = new Dictionary<EAObjID, EA_CItem>();
    Dictionary<EAObjID, EA_CMapObject>   m_mapObjectList   = new Dictionary<EAObjID, EA_CMapObject>();

    
    List<EA_CObjectBase>  m_entityList   = new List<EA_CObjectBase>();

    EA_CCharUser  m_pMainPlayer = null;     //	My character's information.

    //--------------------------------------------------------------------------
    //	Information for constructors and destructors
    EAIDGenerator m_pIDGenerator = new EAIDGenerator(50000);
	//--------------------------------------------------------------------------

    public EACObjManager()
    {
        Init();
    }

	public void Init()
    {
        //Debug.Log("EACObjManager Init");

        //  [3/6/2014 puos] Load template


        if ( m_pMainPlayer == null)
	    {
		    m_pMainPlayer = new EA_CCharUser();

		    ObjectInfo emptyObjInfo = new ObjectInfo();
            emptyObjInfo.m_eObjState = eObjectState.CS_MYENTITY;
            emptyObjInfo.m_eObjType = eObjectType.CT_MYPLAYER;
            emptyObjInfo.m_GDObjId = CObjGlobal.MyPlayerID; 
		    
            m_pMainPlayer.SetObjInfo(emptyObjInfo);

		    CharInfo emptyCharInfo = new CharInfo();
		    m_pMainPlayer.SetCharInfo(emptyCharInfo);
	    }
    }
    
	public void Destroy()
    {
        // player
        {
            foreach (KeyValuePair<uint, EA_CCharUser> pair in m_mapPlayerList)
            {
                pair.Value.ResetInfo(eObjectState.CS_UNENTITY);
            }

            m_mapPlayerList.Clear();
        }

        // monster
        {
            foreach (KeyValuePair<uint, EA_CCharMob> pair in m_mapMonsterList)
            {
                pair.Value.ResetInfo(eObjectState.CS_UNENTITY);
            }

            m_mapMonsterList.Clear();
        }

        // npc
        {
            foreach (KeyValuePair<uint, EA_CCharNPC> pair in m_mapNPCList)
            {
                pair.Value.ResetInfo(eObjectState.CS_UNENTITY);
            }

            m_mapNPCList.Clear();
        }

        // item
        {
            foreach (KeyValuePair<uint, EA_CItem> pair in m_mapItemList)
            {
                pair.Value.ResetInfo(eObjectState.CS_UNENTITY);
            }

            m_mapItemList.Clear();
        }

        // object
        {
            foreach (KeyValuePair<uint, EA_CMapObject> pair in m_mapObjectList)
            {
                pair.Value.ResetInfo(eObjectState.CS_UNENTITY);
            }

            m_mapObjectList.Clear();
        }

        m_entityList.Clear();

        m_pIDGenerator.ReGenerate();
    }

    public void ResourceLoad(string szPath , string sKey , int initCreateCount = 1)
    {
        CObjResourcePoolingManager.instance.CreatePool(sKey , szPath , initCreateCount);
    }

    public void ResourceUnLoad(string sKey)
    {
        CObjResourcePoolingManager.instance.DeletePool(sKey);
    }

	public void StartChangeMap()
    {
        Debug.Log("StartChangeMap - EACObjManager begin frameCount : " + Time.frameCount );

        Destroy();

        Debug.Log("EndChangeMap - EACObjManager end frameCount : " + Time.frameCount );
    }

  
    public void EndChangeMap()
    {
        Debug.Log("EndChangeMap - EACObjManager begin frameCount : " + Time.frameCount);

        Destroy();

        CObjResourcePoolingManager.instance.Destroy();

        Debug.Log("EndChangeMap - EACObjManager end");
    }

    /// <summary>
    /// The game object from the server is applied first and the Cry Entity is created by FirstUpdate ().
    /// </summary>
    /// <param name="GameObjInfo"></param>
    /// <returns></returns>
    public EA_CObjectBase CreateGameObject(ObjectInfo GameObjInfo)
    {
        EA_CObjectBase pReturnObject = null;

         GameObjInfo.m_eObjState = eObjectState.CS_SETENTITY;

        //	Temporarily specify ObjId (sometimes temporary use by external system)
        bool bCreateTempId = false;
        if (CObjGlobal.InvalidObjID == GameObjInfo.m_GDObjId)
        {
            GameObjInfo.m_GDObjId = (EAObjID)m_pIDGenerator.GenerateID();
            bCreateTempId = true;
        }

        switch (GameObjInfo.m_eObjType)
        {
            case eObjectType.CT_MYPLAYER:
                {
                    if (GetMainPlayer() != null)
                    {
                        //	My character is created at startup so I don't need to create it
                        GetMainPlayer().SetObjInfo(GameObjInfo);

                        pReturnObject = GetMainPlayer();
                    }
                  
                }
                break;
            case eObjectType.CT_PLAYER:
                {
                    EA_CCharUser pCharPlayer = null;

                    //	First check if it exists
                    if (bCreateTempId == false)
                    {
                        pCharPlayer = GetPlayer(GameObjInfo.m_GDObjId);
                    }

                    //	If not, create a new one
                    if (pCharPlayer == null)
                    {
                        pCharPlayer = new EA_CCharUser();

                        if (pCharPlayer != null)
                        {
                            m_mapPlayerList.Add(GameObjInfo.m_GDObjId, pCharPlayer);
                        }
                    }

                    //	Enter the information of the created object and apply it
                    if (pCharPlayer != null)
                    {
                        pCharPlayer.SetObjInfo(GameObjInfo);
                    }

                    pReturnObject = pCharPlayer;

                }
                break;
            case eObjectType.CT_NPC:
                {
                    EA_CCharNPC pCharNPC = null;

                    //	First check if it exists
                    if (bCreateTempId == false)
                    {
                        pCharNPC = GetNPC(GameObjInfo.m_GDObjId);
                    }

                    //	If not, create a new one
                    if (pCharNPC == null)
                    {
                        pCharNPC = new EA_CCharNPC();

                        if (pCharNPC != null)
                        {
                            m_mapNPCList.Add(GameObjInfo.m_GDObjId, pCharNPC);
                        }
                    }

                    //	Apply the information to the generated number
                    if (pCharNPC != null)
                    {
                        pCharNPC.SetObjInfo(GameObjInfo);
                    }

                    pReturnObject = pCharNPC;
                }
                break;
            case eObjectType.CT_MONSTER:
                {
                    EA_CCharMob pCharPlayer = null;

                    //	First check if it exists
                    if (bCreateTempId == false)
                    {
                        pCharPlayer = GetMob(GameObjInfo.m_GDObjId);
                    }

                    //	If not, create a new one
                    if (pCharPlayer == null)
                    {
                        pCharPlayer = new EA_CCharMob();

                        if (pCharPlayer != null)
                        {
                            m_mapMonsterList.Add(GameObjInfo.m_GDObjId, pCharPlayer);
                        }
                    }

                    //	Apply the information to the generated number
                    //			assert( pCharInfo && "No Character Info" );
                    if (pCharPlayer != null)
                    {
                        pCharPlayer.SetObjInfo(GameObjInfo);
                    }

                     pReturnObject = pCharPlayer;
                }
                break;
          case eObjectType.CT_ITEMOBJECT:
                {
                    EA_CItem pItem = null;

                    //	First check if it exists
                    if (bCreateTempId == false)
                    {
                        pItem = GetItemObject(GameObjInfo.m_GDObjId);
                    }

                    //	If not, create a new one
                    if (pItem == null)
                    {
                        pItem = new EA_CItem();

                        if (pItem != null)
                        {
                            m_mapItemList.Add(GameObjInfo.m_GDObjId, pItem);
                        }
                    }

                    //	Apply the information to the generated number
                    if (pItem != null)
                    {
                        pItem.SetObjInfo(GameObjInfo);
                    }

                    pReturnObject = pItem;
                }
                break;

          case eObjectType.CT_MAPOBJECT:
                {
                    EA_CMapObject pObject = null;

                    //	First check if it exists
                    if (bCreateTempId == false)
                    {
                        pObject = GetMapObject(GameObjInfo.m_GDObjId);
                    }

                    //	If not, create a new one
                    if (pObject == null)
                    {
                        pObject = new EA_CMapObject();

                        if (pObject != null)
                        {
                            m_mapObjectList.Add(GameObjInfo.m_GDObjId, pObject);
                        }
                    }

                    //	Apply the information to the generated number
                    if (pObject != null)
                    {
                        pObject.SetObjInfo(GameObjInfo);
                    }

                    pReturnObject = pObject;
                }
                break;

                /*
                  case CT_QUEST:
                    {
                        //	First check if it exists
                        if( false == bCreateTempId )
                            pReturnObject = GetQuestItem(GameObjInfo.m_DiaObjId);

                        //	If not, create a new one
                        if( NULL == pReturnObject )
                        {
                            pReturnObject = new IfDia_CObjectBase;
                            assert( pReturnObject && "Not Create ItemObject" );
                            if( pReturnObject )
                                m_mapQuestList.insert( TQuestListMap::value_type(GameObjInfo.m_DiaObjId,pReturnObject) );
                        }

                         //	Apply the information to the generated number
                        if( pReturnObject )
                        {
                            pReturnObject->SetObjInfo(GameObjInfo);
                        }
                    }
                    break;
                 /**/
        }

        //assert( pReturnObject );

        //	Apply the information to the generated number
        if (pReturnObject != null)
        {
            m_entityList.Add(pReturnObject);
        }

        return pReturnObject;
    }


    public bool DeleteGameObject(eObjectType DelObjectType, EAObjID _id)
    {
        if (CObjGlobal.InvalidObjID != _id)
        {
            EA_CObjectBase obj = null;

            switch (DelObjectType)
            {
                default:
                case eObjectType.CT_MYPLAYER:
                    {
                        break;
                    }
                case eObjectType.CT_PLAYER:
                    {
                        obj = RemovePlayer(_id);
                        break;
                    }
                case eObjectType.CT_NPC:
                    {
                        obj = RemoveNPC(_id);
                        break;
                    }
                case eObjectType.CT_MONSTER:
                    {
                        obj = RemoveMob(_id);
                        break;
                    }
                case eObjectType.CT_ITEMOBJECT:
                    {
                        obj = RemoveItem(_id);
                        break;
                    }

                case eObjectType.CT_MAPOBJECT:
                    {
                        obj = RemoveMapObject(_id);
                    }
                    break;
             }

            RemoveEntity(obj);
        }

        return false;
    }
    
    public EA_CCharUser GetMainPlayer()
    {
        return m_pMainPlayer;
    }

	public EA_CCharNPC	GetNpcFromIndex( EATblID _index )
    {
        if (m_mapNPCList.Count > 0)
        {
            EA_CCharNPC nPC = null;

            m_mapNPCList.TryGetValue(_index, out nPC);
        }
                
        return null;
    }

    public EA_CCharBPlayer GetActor( EAObjID _id )
    {
        EA_CCharBPlayer pActor = null;

        pActor = (pActor == null) ? GetPlayer(_id) : pActor;
        pActor = (pActor == null) ? GetMob(_id) : pActor;
        pActor = (pActor == null) ? GetNPC(_id) : pActor;

        return pActor;
    }

    EA_CCharUser GetPlayer( EAObjID _id )
    {
        EA_CCharUser pUser = null;
        
        m_mapPlayerList.TryGetValue(_id, out pUser);
        
        if (pUser == null && m_pMainPlayer.GetObjID() == _id)
        {
            return m_pMainPlayer;
        }
        
        return pUser;
    }

    public EA_CCharMob GetMob(EAObjID _id)
    {
        EA_CCharMob pMob = null;

        m_mapMonsterList.TryGetValue(_id, out pMob);

        return pMob;
    }

    public EA_CCharNPC GetNPC(EAObjID _id)
    {
        EA_CCharNPC pNPC = null;

        m_mapNPCList.TryGetValue(_id, out pNPC);

        return pNPC;
    }

    public EA_CItem GetItemObject(EAObjID _id)
    {
        EA_CItem pItem = null;

        m_mapItemList.TryGetValue(_id, out pItem);

        return pItem;
    }

    //  [4/3/2014 puos] Bring a map object
    public EA_CMapObject GetMapObject(EAObjID _id)
    {
        EA_CMapObject pMapObject = null;

        m_mapObjectList.TryGetValue(_id, out pMapObject);

        return pMapObject;
    }

    public EA_CObjectBase RemovePlayer(EAObjID _id)
    {
        EA_CCharUser pUser = GetPlayer(_id);

        if (pUser != null)
        {
            pUser.ResetInfo(eObjectState.CS_DEAD);
            m_mapPlayerList.Remove((uint)_id);
            m_pIDGenerator.FreeID(_id);
        }

        return pUser;
    }

    public EA_CObjectBase RemoveMob(EAObjID _id)
    {
        EA_CCharMob pMob = GetMob(_id);

        if (pMob != null)
        {
            pMob.ResetInfo(eObjectState.CS_DEAD);
            m_mapMonsterList.Remove((uint)_id);
            m_pIDGenerator.FreeID(_id);
        }

        return pMob;
    }

    public EA_CObjectBase RemoveNPC(EAObjID _id)
    {
        EA_CCharNPC pNPC = GetNPC( _id );

        if (pNPC != null)
        {
            pNPC.ResetInfo(eObjectState.CS_DEAD);
            m_mapNPCList.Remove((uint)_id);
            m_pIDGenerator.FreeID(_id);
        }

        return pNPC;
     }

    public EA_CObjectBase RemoveItem(EAObjID _id)
    {
        EA_CItem pItem = GetItemObject(_id);

        if (pItem != null)
        {
            pItem.ResetInfo(eObjectState.CS_DEAD);
            m_mapItemList.Remove((uint)_id);
            m_pIDGenerator.FreeID(_id);
        }

        return pItem;
    }

    public EA_CObjectBase RemoveMapObject(EAObjID _id)
    {
        EA_CMapObject pMapObject = GetMapObject(_id);

        if (pMapObject != null)
        {
            pMapObject.ResetInfo(eObjectState.CS_DEAD);
            m_mapObjectList.Remove((uint)_id);
            m_pIDGenerator.FreeID(_id);
        }

        return pMapObject;
    }
      
    public void RemoveEntity(EA_CObjectBase gameEntity)
    {
        if(gameEntity != null)
        {
            m_entityList.Remove(gameEntity);
        }  
    }

    public EA_CObjectBase GetGameObject(EAObjID _id)
    {
        EA_CObjectBase obj = null;

        obj = (obj == null) ? GetActor(_id) : obj;
        obj = (obj == null) ? GetItemObject(_id) : obj;
        obj = (obj == null) ? GetMapObject(_id) : obj;

        return obj;
    }

    public void For(System.Func<EA_CObjectBase,bool> action)
    {
        if (action == null)
        {
            return;
        }

        for(int i = 0; i < m_entityList.Count; ++i)
        {
            if (action(m_entityList[i]))
            {
                break;
            }
        }

        action(m_pMainPlayer);
    }

    public EA_CCharUser	CreateUser( ObjectInfo GameObjInfo, CharInfo charInfo )
    {
        GameObjInfo.m_eObjType = eObjectType.CT_PLAYER;
        EA_CCharUser pUser = (EA_CCharUser)CreateGameObject(GameObjInfo);

        if(pUser != null)
        {
            pUser.SetCharInfo(charInfo);
            pUser.PosInit();
        }

        return pUser;
    }

    public EA_CCharNPC CreateNPC( ObjectInfo GameObjInfo , NPCInfo npcInfo )
    {
        GameObjInfo.m_eObjType = eObjectType.CT_NPC;
        EA_CCharNPC pNpc = (EA_CCharNPC)CreateGameObject(GameObjInfo);

        if (pNpc != null)
        {
            pNpc.SetNPCInfo(npcInfo);
            pNpc.PosInit();
        }

        return pNpc;
    }

    public EA_CCharMob CreateMob(ObjectInfo GameObjInfo , MobInfo mobInfo )
    {
        GameObjInfo.m_eObjType = eObjectType.CT_MONSTER;
        EA_CCharMob pMob = (EA_CCharMob)CreateGameObject(GameObjInfo);

        if (pMob != null)
        {
            pMob.SetMobInfo(mobInfo);
            pMob.PosInit();
        }

        return pMob;
    }

    public EA_CItem CreateItem(ObjectInfo GameObjInfo, ItemObjInfo itemInfo)
    {
        GameObjInfo.m_eObjType = eObjectType.CT_ITEMOBJECT;
        EA_CItem pItem = (EA_CItem)CreateGameObject(GameObjInfo);

        if (pItem != null)
        {
            pItem.SetItemInfo(itemInfo);
        }

        return pItem;
    }

    public EA_CMapObject CreateMapObject(ObjectInfo GameObjInfo , MapObjInfo mapInfo)
    {
        GameObjInfo.m_eObjType = eObjectType.CT_MAPOBJECT;
        EA_CMapObject pObject = (EA_CMapObject)CreateGameObject(GameObjInfo);

        if (mapInfo != null)
        {
            pObject.SetMapInfo(mapInfo);
        } 

        return pObject;
    }
      
    //IFDia_CVehicle*		CreateGameObject( ObjectInfo& GameObjInfo, VehicleInfo& vehicleInfo );
}
