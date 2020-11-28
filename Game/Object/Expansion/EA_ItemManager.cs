using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = EAFrameWork.Debug;


using ITEM_UNIT_INDEX = System.UInt32;
using EAObjID         = System.UInt32;

public class EA_ItemManager : EAGenericSingleton<EA_ItemManager>
{
    Dictionary<ITEM_UNIT_INDEX, EA_CItemUnit> m_mapItemUnitList = new Dictionary<ITEM_UNIT_INDEX, EA_CItemUnit>();

    EA_MyPLItemManager m_myPlItemManager = new EA_MyPLItemManager(); //  [4/7/2014 puos] My Item Management
    EA_PCItemManager   m_PCItemManager   = new EA_PCItemManager();   //  [4/7/2014 puos] npc and mob item management

    //--------------------------------------------------------------------------
    //	Information for constructors and destructors
    EAIDGenerator m_pIDGenerator = new EAIDGenerator(10000);
    //--------------------------------------------------------------------------

    public EA_ItemManager()
    {
       
    }

	public void Init()
    {
        Debug.Log("EA_ItemManager Init");
    }

    bool SetItemUnitCount(ITEM_UNIT_INDEX UnitIdx, uint count)
    {
        return true;
    }

    /*! Mount item in equipment slot */
    public bool EquipmentItem(EAObjID _id, uint equip_slot)
    {
        EA_CCharBPlayer pUser = EACObjManager.instance.GetActor(_id);

         EA_Equipment pEquipment = null;

        if (pUser.GetObjInfo().m_eObjType == eObjectType.CT_MYPLAYER)
        {
            pEquipment = m_myPlItemManager.GetEquip();
        }
        else
        {
            pEquipment = m_PCItemManager.Get_PCEquipItem(_id);
            
        }

        pEquipment.EquipItem(pUser,equip_slot);
      
        return true;
    }

    public bool DeleteItemUnit(ITEM_UNIT_INDEX _id)
    {
        EA_CItemUnit pItemUnit = FindItemUnit(_id);

        if (pItemUnit != null)
        {
            //GDDebug.Log("EA_ItemManager - DeleteItemUnit - ITEM_UNIT_INDEX :" + _id);

            //  [4/10/2014 puos] Delete when an item object is created in an item unit
            if (pItemUnit.GetItemBaseInfo().m_GDObjId != CObjGlobal.InvalidObjID)
            {
                EACObjManager.instance.RemoveItem(pItemUnit.GetItemBaseInfo().m_GDObjId);
            } 

            m_mapItemUnitList.Remove((uint)_id);
            m_pIDGenerator.FreeID(_id);
        }

        return true;
    }

    public EA_CItemUnit FindItemUnit(ITEM_UNIT_INDEX _id)
    {
        EA_CItemUnit pItemUnit = null;

        m_mapItemUnitList.TryGetValue(_id, out pItemUnit);

        return pItemUnit;
    }
    
    public bool Equip_InsertEquipItem(EAObjID _id, uint slot, EA_CItemUnit pitem)
    {
        EA_CCharBPlayer pActor = EACObjManager.instance.GetActor(_id);

        if(pActor == null)
        { return false;
        }

        if (pActor.GetObjInfo().m_eObjType == eObjectType.CT_MYPLAYER)
        {
            EA_Equipment  pEquipment = m_myPlItemManager.GetEquip();

            if (pEquipment != null)
            {
                pEquipment.InsertEquipItem(slot, pitem);
            }
        }
        else
        {
            EA_Equipment pEquipment = m_PCItemManager.Get_PCEquipItem(_id);

            if (pEquipment == null)
            {
                pEquipment = new EA_Equipment();
                m_PCItemManager.InsertPCEquip(_id, pEquipment);
            }

            //  [4/10/2014 puos] Equipped with items
            pEquipment.InsertEquipItem(slot, pitem);
        }

        return true;
    }

    bool Equip_DeleteItem(EAObjID _id, uint slot)
    {
        return true;
    }


    public EA_Equipment Equip_FindEqipment(EAObjID _id)
    {
        EA_CCharBPlayer pActor = EACObjManager.instance.GetActor(_id);

        EA_Equipment pEquipment = null;

        if (pActor != null)
        {
            if (pActor.GetObjInfo().m_eObjType == eObjectType.CT_MYPLAYER)
            {
                pEquipment = m_myPlItemManager.GetEquip();
            }
            else
            {
                pEquipment = m_PCItemManager.Get_PCEquipItem(_id);
            }
        } 
        
        return pEquipment;
    } 

    public bool Equip_RemoveEquip(EAObjID _id)
    {
        EA_CCharBPlayer pActor = EACObjManager.instance.GetActor(_id);

        if (pActor != null)
        {
            if (pActor.GetObjInfo().m_eObjType != eObjectType.CT_MYPLAYER)
            {
                m_PCItemManager.RemovePCEquip(_id);
            }
            
            return true;
        }
        
        return false;
    }

    EA_CItemUnit CreateItemUnit(EA_ItemBaseInfo info)
    {
        EA_CItemUnit pItemUnit = null;
        
        if (CObjGlobal.InvalidItemID == info.m_GDItemId)
        {
            info.m_GDItemId = (EAObjID)m_pIDGenerator.GenerateID();

            pItemUnit = new EA_CItemUnit();
            pItemUnit.SetItemInfo(info);

            m_mapItemUnitList.Add(info.m_GDItemId, pItemUnit);
        }
        
        return pItemUnit;
    }

    //  [4/7/2014 puos] Attack weapon generation
    public EA_CItemUnit CreateWeaponItem(EA_ItemBaseInfo info,EA_ItemAttackWeaponInfo weaponinfo)
    {
        info.m_eItemType = eItemType.eIT_Weapon;
        EA_CItemUnit pItemUnit = CreateItemUnit(info);

        if (pItemUnit != null)
        {
            pItemUnit.SetAttackWeaponInfo(weaponinfo);
        }
        
        return pItemUnit;
    }

    //  [4/7/2014 puos] Create defensive weapons
    EA_CItemUnit CreateDefenseItem(EA_ItemBaseInfo info, EA_ItemDefenseInfo defenseInfo)
    {
        info.m_eItemType = eItemType.eIT_Defense;
        EA_CItemUnit pItemUnit = CreateItemUnit(info);
       
        if (pItemUnit != null)
        {
            pItemUnit.SetDefenseInfo(defenseInfo);
        }

        return pItemUnit;
    }

    private void Destroy()
    {
        m_myPlItemManager.MyItemSystemClear();
        m_PCItemManager.PCItemSystemClear();

        m_pIDGenerator.ReGenerate();
    }

    public void StartChangeMap()
    {
        Destroy();

        Debug.Log("StartChangeMap - EA_ItemManager");
    }

      public void EndChangeMap()
    {
        Destroy();

        Debug.Log("EndChangeMap - EA_ItemManager");
    }
}