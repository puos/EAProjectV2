using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ITEM_UNIT_INDEX  = System.UInt32;
using EAObjID          = System.UInt32;


// Equipment window slot location
public enum eEquipSlotSpot
{   
    eESS_MainWeapon1, 
    eESS_MainWeapon2,
    eESS_MainWeapon3,
    eESS_SkillWeapon,
    eESS_Max 
};

// Character Equipment Slot Value
public enum eEquipmentSlotType
{    eEST_BASE = 0, 
     eEST_Weapon1 = 100, 
     eEST_Weapon2,
     eEST_Weapon3,
     eEST_SkillWeapon,
     eEST_MAX,
};

// Weapon Accessories Slot Value
enum eWeaponAccessorySlotType
{   eWAST_BASE = 0, 
    eWAST_Silencer = 30, 
    eWAST_Scope, 
    eWAST_Barrel, 
    eWAST_GunBody, 
    eWAST_Option, 
    eWAST_Disguise, 
    eWAST_MAX 
};

// Suit(Armor) Part Slot Value
enum eSuitPartSlotType 
{ 
    eSPST_BASE = 0, 
    eSPST_Head = 50, 
    eSPST_Face, 
    eSPST_Arm, 
    eSPST_Breast,
    eSPST_Leg, 
    eSPST_Waist, 
    eSPST_MAX 
};

// Existing Item System Area
enum eItemSaveSpot
{ 
    eISS_MyInven = 200,
    eISS_MyEquip, 
    eISS_MyShop, 
    eISS_Looting,
    eISS_WareHouse,
    eISS_MAX
};


////////////////////////////////////////////////////////////////////////////////////
/* Item unit storage location */
enum eItemKeepSpot
{   eIKS_Base, 
    eIKS_MyInven, 
    eIKS_MyEquip, 
    eIKS_OtherEquip,
    eIKS_Store,
    eIKS_Looting, 
    eIKS_WareHouse,
    eIKS_MAX
};

/* Item Type  */
public enum eItemType 
{   eIT_Base, 
    eIT_Weapon,
    eIT_Defense, 
    eIT_Material,
    eIT_Potion,
    eIT_Accessory,
    eIT_Bullet,
    eIT_Max,
};

/* Attack weapon type */
public enum eWeaponType 
{
    eWT_Unarmed = 0,
    eWT_Sword,
    eWT_Bow,
    eWT_Rifle,
    eWT_Pistol,
    eWT_Turret,
    eWT_Breathing,
    eWT_Max,
};

/* ProjectileType */
public enum eProjectileType
{
    ePT_Custom_A,
    ePT_Custom_B,
    ePT_General,
    ePT_Guided,
    ePT_Ray,
};


/* Inventory slot count */
enum eInvenSlotSize
{
    eISS_Base = 24, 
    eISS_Twnlve = 12, 
    eISS_Fourty = 40,
    eISS_Max = eISS_Base 
};

/* Store sales types */
enum eStoreSellType 
{   eSST_First,
    eSST_Second, 
    eSST_Third,
    eSST_TypeMax 
};

/* Store view page and count settings */
enum sStoreSlot 
{   eSS_StartView = 1, 
    eSS_Endview = 2, 
    eSS_Prev = 3, 
    eSS_Next = 4, 
    eSS_OneView = 10, 
    eSS_MaxCount = 30,
};

enum eBankState 
{   eBS_Start = 1, 
    sBS_End = 2, 
    eBS_Prev = 3, 
    eBS_Next = 4, 
    eBS_Base = 35, 
    eBS_Fifty = 50, 
    eBS_Seventy = 70,
    eBS_Max = eBS_Seventy 
};


public class EA_ItemBaseInfo 
{
    
	public ITEM_UNIT_INDEX   m_GDItemId;
    public EAObjID           m_GDObjId;
    public string            m_ModelTypeIndex;
	public string		     m_szItemName;
    public eItemType         m_eItemType;
    public uint			     m_nLevel;
	public uint			     m_nDurability;
	public uint			     m_nPrice;
	public uint			     m_nWeight;
	public uint			     m_nCount;

    public Type m_objClassType;           // Item class type

    public EA_ItemBaseInfo() 
	{
         m_GDItemId       = CObjGlobal.InvalidItemID;
         m_GDObjId        = CObjGlobal.InvalidObjID;
         m_ModelTypeIndex = "";
         m_szItemName  = "";
         m_eItemType   = eItemType.eIT_Base;
         m_nLevel      =  0;
         m_nDurability =  0;
         m_nPrice      =  0;
         m_nWeight     =  0;
         m_nCount      =  0;
         m_objClassType = default(Type);
    }

    public void Copy(EA_ItemBaseInfo ib)
	{
        m_GDItemId = ib.m_GDItemId;
        m_GDObjId  = ib.m_GDObjId;
        m_ModelTypeIndex = ib.m_ModelTypeIndex;
        m_szItemName = ib.m_szItemName;
        m_eItemType = ib.m_eItemType;
        m_nLevel = ib.m_nLevel;
        m_nDurability = ib.m_nDurability;
        m_nPrice = ib.m_nPrice;
        m_nWeight = ib.m_nWeight;
        m_nCount = ib.m_nCount;
        m_objClassType = ib.m_objClassType;  
	}
}


public class EA_ItemAttackWeaponInfo
{
    public string       id;
    public eWeaponType  weaponType;
    public float        fKillDistance;       //  [4/7/2014 puos] Range
    public float        fFiringTime;         //  [4/7/2014 puos] Launch time
    public string       uProjectileModelType;
    public Type         m_objProjectileClassType;           // Item class type
    public float        fProjectileSpeed;
    public bool         bAutoMode;
    public eProjectileType projectileType;
    public float        m_fTargetDistance;  // Target offset distance
    public bool         bForceMode;       // Force call to shoot
    public EAObjID      m_TargetActorId;    // Target actor id
    public string       m_strTargetActorBoneName;
    public float        projectileRadius;
    public Action<uint> onExplosionEvent;

    public EA_ItemAttackWeaponInfo()
    {
        weaponType = eWeaponType.eWT_Unarmed;
        fKillDistance = 50.0f;
        fFiringTime = 0;
        uProjectileModelType = "";
        m_objProjectileClassType = default(Type);
        fProjectileSpeed = 0;
        bAutoMode = true;
        projectileType = eProjectileType.ePT_General;
        m_fTargetDistance = 0;
        bForceMode = false;
        m_TargetActorId          = CObjGlobal.InvalidObjID;
        m_strTargetActorBoneName = null;
        projectileRadius = 0.0f;
        onExplosionEvent = null;
    }

    public void Copy(EA_ItemAttackWeaponInfo obj)
    {
        id = obj.id;
        weaponType = obj.weaponType;
        fKillDistance = obj.fKillDistance;
        fFiringTime = obj.fFiringTime;
        uProjectileModelType = obj.uProjectileModelType;
        m_objProjectileClassType = obj.m_objProjectileClassType;
        fProjectileSpeed = obj.fProjectileSpeed;
        bAutoMode = obj.bAutoMode;
        projectileType = obj.projectileType;
        m_fTargetDistance = obj.m_fTargetDistance;
        bForceMode      = obj.bForceMode;
        m_TargetActorId = obj.m_TargetActorId;
        m_strTargetActorBoneName = obj.m_strTargetActorBoneName;
        projectileRadius = obj.projectileRadius;
        onExplosionEvent = obj.onExplosionEvent;
    }
}


public class EA_ItemDefenseInfo
{
    public int m_nIndex;
    public int m_nDefType;
    public int m_nDefense;
    public int m_nRace;
    public int m_nPartNum;

    public EA_ItemDefenseInfo()
    {
        m_nIndex   = 0;
        m_nDefType = 0;
        m_nDefense = 0;
        m_nRace    = 0;
        m_nPartNum = 0;
    }

    public void Copy(EA_ItemDefenseInfo obj)
    {
        m_nIndex = obj.m_nIndex;
        m_nDefType = obj.m_nDefType;
        m_nDefense = obj.m_nDefense;
        m_nRace = obj.m_nRace;
        m_nPartNum = obj.m_nPartNum;
    } 
}

