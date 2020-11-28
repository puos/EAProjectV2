using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Debug = EAFrameWork.Debug;

using EAObjID = System.UInt32;

public class EA_PCItemManager
{
    /*! Player_id로 해당 actor 장비 아이템 생성.*/
	public bool	InsertPCEquip(EAObjID _id, EA_Equipment pPLEquip)
    {
        m_PCEquipMap.Add(_id, pPLEquip);
        return true;
    }

	/*! Player_id로 해당 pc 장비 삭제.*/
	public bool RemovePCEquip(EAObjID _id)
    {
        if (m_PCEquipMap.ContainsKey(_id) == true)
        {
            EA_Equipment pEquipment = m_PCEquipMap[_id];

            if (pEquipment != null)
            {
                pEquipment.RemoveAllItem();
                pEquipment = null; 
            }
            
            m_PCEquipMap.Remove(_id);
            return true;
        }
        return false;
    }
	
	/*! Player_id로 해당 player 장비 찾기 .*/
	public EA_Equipment  Get_PCEquipItem(EAObjID _id)
    {
        if (m_PCEquipMap.ContainsKey(_id) == true)
        {
            return m_PCEquipMap[_id];
        }

        return null;
    }

	/*! 장비 저장하기*/
	public bool	EquipItem(EAObjID _id, uint slot, EA_CItemUnit pItem)
    {
        if (m_PCEquipMap.ContainsKey(_id) == true)
        {    return false;
        }

        EA_Equipment pEquipment = m_PCEquipMap[_id];

        pEquipment.InsertEquipItem(slot,pItem);

        return true;
    }

	/*! 장비 해제하기*/
    bool RemoveItem(EAObjID _id, uint slot)
    {
        if (m_PCEquipMap.ContainsKey(_id) == true)
        {   return false;
        }

        EA_Equipment pEquipment = m_PCEquipMap[_id];

        pEquipment.RemoveEquipItembySlot(slot);
        return true;
    }

    public void PCItemSystemClear()
    {
        foreach (KeyValuePair<EAObjID, EA_Equipment> pEquipData in m_PCEquipMap)
        {
            EA_Equipment pEquipment = m_PCEquipMap[pEquipData.Key];

            if (pEquipment != null)
            {
                pEquipment.RemoveAllItem();
            }
        }

        m_PCEquipMap.Clear();
    }


    Dictionary<EAObjID, EA_Equipment> m_PCEquipMap = new Dictionary<EAObjID, EA_Equipment>();
}