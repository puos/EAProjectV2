using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using ITEM_UNIT_INDEX = System.UInt32;
using EAObjID = System.UInt32;
using Debug = EAFrameWork.Debug;

public class EA_Equipment
{
    public EA_Equipment()
    {
        InitEquipSlot();
    }

    void InitEquipSlot()
    {
        for (uint slot = 0; slot < (uint)eEquipSlotSpot.eESS_Max; ++slot)
        {
            m_EquipmentList.Add(slot, null);
	    }
    }

    /*! 아이템 유닛을  장비 슬롯 위치에 저장 */
    public bool InsertEquipItem(uint equip_slot, EA_CItemUnit pitem)
    {
        if ( (m_EquipmentList.ContainsKey(equip_slot) == false) || 
             (equip_slot >= (uint)eEquipSlotSpot.eESS_Max) )
        {   return false;
        }
        
       EA_CItemUnit pPrevItemUnit = m_EquipmentList[equip_slot];

       if (pPrevItemUnit != null)
       {   pPrevItemUnit.RequestDelete();
       } 

        m_EquipmentList[equip_slot] = pitem;
             
        return true;
    }

    uint GetEquipSlotByIndex(ITEM_UNIT_INDEX itemIndex)
    {
        foreach (KeyValuePair<uint, EA_CItemUnit> pItem in m_EquipmentList)
        {
            if (pItem.Value != null)
            {
                if (pItem.Value.GetItemId() == itemIndex)
                {     return pItem.Key;
                }
            }
        }

        return (uint)eEquipSlotSpot.eESS_Max;	
    }


    /*! 아이템 인덱스로 장비창에서 해제 */
    public bool RemoveEquipItem(ITEM_UNIT_INDEX itemIndex)
    {
        uint slot = GetEquipSlotByIndex(itemIndex);
        return RemoveEquipItembySlot(slot);
    }

    public bool RemoveAllItem()
    {
        try
        {
            for (uint slot = 0; slot < (uint)eEquipSlotSpot.eESS_Max; ++slot)
            {
                EA_CItemUnit pPrevItemUnit = m_EquipmentList[slot];

                if (pPrevItemUnit != null)
                {
                    pPrevItemUnit.RequestDelete();
                    m_EquipmentList[slot] = null;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(ex.Message);       
        }
        
       return true;
    }

    // 슬롯에 설치된 아이템 제거
    public bool RemoveEquipItembySlot(uint equip_slot)
    {
        if (IsEquipItem(equip_slot) == false)
        {
            return false;
        }

        EA_CItemUnit pPrevItemUnit = m_EquipmentList[equip_slot];

        if (pPrevItemUnit != null)
        { 
            pPrevItemUnit.RequestDelete();
            m_EquipmentList[equip_slot] = null;
        } 
               
        
        return true;
    }

    /*! 아이템 인덱스로 아이템 유닛 찾음. */
    public EA_CItemUnit FindEquipItem(ITEM_UNIT_INDEX itemIndex)
    {
        uint slot = GetEquipSlotByIndex(itemIndex);
        return FindEquipItembySlot(slot);
    }

    public EA_CItemUnit FindEquipItembySlot(uint equip_slot)
    {
	    if (m_EquipmentList.ContainsKey(equip_slot) == false)
        {    return null;
        }

        return m_EquipmentList[equip_slot];
    }

    bool IsEquipItem(uint equip_slot)
    {
        if (m_EquipmentList.ContainsKey(equip_slot) == false)
        {      return false;
        }

        EA_CItemUnit pItem = m_EquipmentList[equip_slot];

        if (pItem == null)
        {     return false;
        }
        
        return true;
    }

    public EA_CItemUnit GetCurrentItem()
    {
        return FindEquipItembySlot(m_nCurrEquipSlot);
    }

    public void EquipItem(EA_CCharBPlayer pUser,uint equip_slot)
    {
        EA_CItemUnit pItemUnit = FindEquipItembySlot(equip_slot);

        if (pItemUnit != null)
        {
            if (pItemUnit.GetItemBaseInfo().m_eItemType == eItemType.eIT_Weapon)
            {
                EAObjID objId = SetActorItem(pUser, pItemUnit);
                
                pItemUnit.GetItemBaseInfo().m_GDObjId = objId;
                m_nCurrEquipSlot = equip_slot;
                pUser.DoSwitchWeapons((uint)pItemUnit.GetAttackWeaponInfo().weaponType);
            }
        }
        else // 무기가 없을시에도 장착을 시도해야함
        {
            m_nCurrEquipSlot = equip_slot;
            pUser.DoSwitchWeapons(0);
        }
    }

    EAObjID SetActorItem(EA_CCharBPlayer pUser, EA_CItemUnit pItemUnit)
    {
        if (pUser == null || pItemUnit == null)
        {
            return CObjGlobal.InvalidObjID;
        }
                
        EAObjID objID = CObjGlobal.InvalidObjID;

        ObjectInfo ObjInfo = new ObjectInfo();

        ObjInfo.m_ModelTypeIndex = pItemUnit.GetItemBaseInfo().m_ModelTypeIndex;
        ObjInfo.m_objClassType = pItemUnit.GetItemBaseInfo().m_objClassType;

        ItemObjInfo itemInfo = new ItemObjInfo();

        itemInfo.m_HavenUser = pUser.GetObjID();
        itemInfo.m_iItemIndex = pItemUnit.GetItemId(); //  [4/7/2014 puos] The item ID of the item object

        if (pItemUnit.GetItemBaseInfo().m_eItemType == eItemType.eIT_Weapon)
        {
            itemInfo.m_eItemType = eItemObjType.IK_WEAPON;

            //  [3/21/2018 puos] If there is an item object created in the current slot, it is not created.
            //  Fixed bug that was created even with created item object

            EA_CItem pItem = EACObjManager.instance.GetItemObject(pItemUnit.GetItemBaseInfo().m_GDObjId);

            if(pItem == null)
            {
                pItem = EACObjManager.instance.CreateItem(ObjInfo, itemInfo);
            } 
            
            if (pItem != null)
            {
                pUser.SetWeaponAttachment(pItemUnit.GetAttackWeaponInfo().weaponType,pItem.GetLinkEntity());
                return pItem.GetObjID();
            }
        }
              

        return objID;
    }



    public uint m_nCurrEquipSlot = 0; // Current weapon slot number

    Dictionary<uint, EA_CItemUnit> m_EquipmentList = new Dictionary<uint, EA_CItemUnit>();

    //eEquipSlotSpot m_eEquipSlotSize;

}