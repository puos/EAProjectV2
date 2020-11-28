using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Debug = EAFrameWork.Debug;
using ITEM_UNIT_INDEX = System.UInt32;
using EAObjID         = System.UInt32;

public class EA_CItemUnit
{
    public const int LOOTING_SLOTMAX = 10;
    public const int BANKSLOTMAX = 20;
    public const int SHOP_SLOTMAX = 30;
    
    EA_ItemBaseInfo          m_ItemBaseInfo       = new EA_ItemBaseInfo();
    EA_ItemAttackWeaponInfo  m_ItemAttackWeapon   = new EA_ItemAttackWeaponInfo();
    EA_ItemDefenseInfo       m_ItemDefenseInfo    =  new EA_ItemDefenseInfo();

    public EA_CItemUnit()
    {
    }

    public EA_ItemBaseInfo GetItemBaseInfo()
    {
        return m_ItemBaseInfo;
    }

    public EA_ItemAttackWeaponInfo GetAttackWeaponInfo()
    {
        return m_ItemAttackWeapon;
    }

    public EA_ItemDefenseInfo GetItemDefenseInfo()
    {
        return m_ItemDefenseInfo;
    }  

    public void RequestDelete()
    {
        EA_ItemManager.instance.DeleteItemUnit(GetItemId());
    }
        
    /*! 아이템 유닛 아이디 얻기 */
    public ITEM_UNIT_INDEX GetItemId() { return m_ItemBaseInfo.m_GDItemId; }

    public EAObjID GetObjId() { return m_ItemBaseInfo.m_GDObjId; }
        
    /*! 아이템 수량 얻기 */
    public uint GetCount() { return m_ItemBaseInfo.m_nCount; }

    public virtual bool SetItemInfo(EA_ItemBaseInfo itemInfo)
    {
        m_ItemBaseInfo.Copy(itemInfo);
        return true;
    }

    public virtual bool SetAttackWeaponInfo(EA_ItemAttackWeaponInfo attackWeaponInfo)
    {
        m_ItemAttackWeapon.Copy(attackWeaponInfo);
        return true;
    }

    public virtual bool SetDefenseInfo(EA_ItemDefenseInfo DefenseInfo)
    {
        m_ItemDefenseInfo.Copy(DefenseInfo);
        return true;
    }
}
