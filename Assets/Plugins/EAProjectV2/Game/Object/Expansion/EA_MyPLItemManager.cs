using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = EAFrameWork.Debug;


using ITEM_UNIT_INDEX = System.UInt32;

class EA_MyPLItemManager
{
    EA_Equipment   m_pEquipment   = new EA_Equipment();
   
    public EA_MyPLItemManager()
    {
    }

    /*! Delete player item data */
    public void MyItemSystemClear()
    {
        Debug.Log("EA_MyPLItemManager - MyItemSystemClear");
        m_pEquipment.RemoveAllItem();
    }

    /*! Get data in player item unit index. */
    EA_CItemUnit MyPCItme_FindItemUnit(ITEM_UNIT_INDEX UnitIdx)
    {
        return m_pEquipment.FindEquipItem(UnitIdx);
    }

    /*! Set current bullet count to gun */
    void SetCurrAmmoCount(uint count)
    {
    }

     public EA_Equipment GetEquip()
     {   return m_pEquipment;
     }

}
