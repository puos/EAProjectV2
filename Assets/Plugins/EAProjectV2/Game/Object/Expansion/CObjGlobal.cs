using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EAObjID = System.UInt32;
using EATblID = System.UInt32;
using ITEM_UNIT_INDEX = System.UInt32;
using EAEffectID = System.UInt32;
using UnityEngine;


public enum eObjectState
{
    CS_READY,
    CS_SETENTITY,
    CS_UNENTITY,
    CS_DEAD,
    CS_MYENTITY,
    CS_HIDE,
    CS_SHOW,
    CS_MAXNUM
};

//	Object classification
public enum eObjectType
{
    CT_NPC = 1,
    CT_MONSTER,
    CT_MAPOBJECT,
    CT_ITEMOBJECT,
    CT_PLAYER,
    CT_MYPLAYER,
    CT_Vehicle,
    CT_MAXNUM
};

//	Object type
public enum eObjectKind
{
    CK_ACTOR,
    CK_Vehicle,
    CK_ITEM,
    CK_OBJECT,
    CK_MAP,
    CK_MAXNUM
};

//	Part division (character + vehicle)
public enum eCharParts
{
    CP_HEAD,
    CP_UPBODY,
    CP_DOWNBODY,
    CP_ARM,
    CP_LEG,
    CP_HANDS,
    CP_FEET,
    CP_MAX
};

// Item type 
public enum eItemObjType
{
    IK_DEFENSE,
    IK_WEAPON,
    IK_Projectile,
    IK_COIN,
    IK_ETC,
}

public enum eMapObjectType
{
    MOT_THREATEN_WRAP = 1,
    MOT_THREATEN_STONE = 2,
    MOT_MAP,
    MOT_MAX,
}

public static class CObjGlobal
{
    public static EAObjID InvalidObjID = 0;
    public static EATblID InvalidTblID = 0;
    public static ITEM_UNIT_INDEX InvalidItemID = 0;
    public static EAObjID MyPlayerID = 100000;
    public static EAEffectID InvalidEffectID = 0;
    public static float      fInvalidAngle = 0;
    public static float      fInvalidPos   = 0;
}

public class ObjectInfo
{
    public EAObjID m_GDObjId;		//	Unique number of the object specified by the server

    public eObjectType m_eObjType;		//	Object type (character, mob, npc, vehicle, etc.)
    public eObjectState m_eObjState;	//	The state of the object

    public string  m_ModelTypeIndex;	  // Model table index
    public Type m_objClassType;           // Object class type

    public string m_strGameName ="";	//	Name to be used in the game
    ///
    public float[] spawnPos = new float[] { CObjGlobal.fInvalidPos, CObjGlobal.fInvalidPos, CObjGlobal.fInvalidPos };	
    public float[] spawnAngle = new float[] { CObjGlobal.fInvalidAngle, CObjGlobal.fInvalidAngle, CObjGlobal.fInvalidAngle };	

    ///
    public ObjectInfo()
    {
        m_GDObjId = CObjGlobal.InvalidObjID;
        m_eObjType = eObjectType.CT_MAXNUM;
        m_eObjState = eObjectState.CS_READY;
        m_ModelTypeIndex = "";
        m_objClassType = default(Type);
    }

    public void SetObjName(string szNewName)
    {
        m_strGameName = szNewName;
    }

    public void Copy(ObjectInfo obj)
    {
        m_GDObjId = obj.m_GDObjId;
        m_eObjType = obj.m_eObjType;
        m_eObjState = obj.m_eObjState;
        m_ModelTypeIndex = obj.m_ModelTypeIndex;
        m_objClassType = obj.m_objClassType;

        spawnPos[0] = obj.spawnPos[0];
        spawnPos[1] = obj.spawnPos[1];
        spawnPos[2] = obj.spawnPos[2];

        spawnAngle[0] = obj.spawnAngle[0];
        spawnAngle[1] = obj.spawnAngle[1];
        spawnAngle[2] = obj.spawnAngle[2];

        SetObjName(obj.m_strGameName);
    }

}


/// <summary>
/// ObjectInfo and other character information
/// </summary>
public class CharInfo
{
    public uint m_ClassType = 0;		    //	Character occupation
    public string[] m_PartTblId = new string[(int)eCharParts.CP_MAX];	 //	Index number of the part you are currently wearing (obtained from Item Manager)
   
    public float Hp = 0;
    public float Atk =0;

    public float maxHp = 0;
    public float maxAtk = 0;
              
    public CharInfo()
    {       
    }

    public virtual void Copy(CharInfo charInfo)
    {
        Hp = charInfo.Hp;
        Atk = charInfo.Atk;
               
        maxHp = charInfo.maxHp;
        maxAtk = charInfo.maxAtk;
        
        m_ClassType = charInfo.m_ClassType;
        
        for (int i = 0; i < (int)eCharParts.CP_MAX; ++i)
        {
            m_PartTblId[i] = charInfo.m_PartTblId[i];
        }
    }

    public T GetClassType<T>()
    {
        return (T)Enum.ToObject(typeof(T), m_ClassType);
    }
};


//	ObjectInfo and other mob information
public class MobInfo : CharInfo
{
    public int level = 0;

    public int nPoint = 0;

    public float collisionRadius = 0;

    public MobInfo()
    {
    }
    
    public virtual void Copy(MobInfo mobInfo)
    {
        base.Copy(mobInfo);

        level  = mobInfo.level;
       
        nPoint = mobInfo.nPoint;

        collisionRadius = mobInfo.collisionRadius;
    }
};

/// <summary>
/// ObjectInfo and NPC Information
/// </summary>
public class NPCInfo : CharInfo
{
    public EATblID m_Npcindex = CObjGlobal.InvalidTblID;	//	Feature table index on Npc
    public uint m_nOwner_id = 0;
    
    public NPCInfo()
    {
       
    }

    public virtual void Copy(NPCInfo npcInfo)
    {
        base.Copy(npcInfo);

        m_Npcindex = npcInfo.m_Npcindex;
        m_nOwner_id  = npcInfo.m_nOwner_id;
    }
};


public class ItemObjInfo
{
    public EAObjID m_HavenUser;	//	Feature table index on Npc

    public ITEM_UNIT_INDEX m_iItemIndex;
    public int m_iItemCount;
    public uint m_itemRemainTime;
    public eItemObjType m_eItemType;
    public uint m_Score;

    public ItemObjInfo()
    {
        m_HavenUser = CObjGlobal.InvalidObjID;
        m_iItemIndex = CObjGlobal.InvalidItemID;
        m_iItemCount = 0;
        m_itemRemainTime = 0;
        m_eItemType = eItemObjType.IK_ETC;
        m_Score = 0;
    }

    public void Copy(ItemObjInfo Iteminfo)
    {
        m_HavenUser = Iteminfo.m_HavenUser;
        m_iItemIndex = Iteminfo.m_iItemIndex;
        m_iItemCount = Iteminfo.m_iItemCount;
        m_eItemType = Iteminfo.m_eItemType;
        m_itemRemainTime = Iteminfo.m_itemRemainTime;
        m_Score = Iteminfo.m_Score;
    }
};


public class MapObjInfo
{
    public eMapObjectType m_eMapObjectType;
    public int m_nDurability;
    public int m_nCrashDamage;
    public uint m_Score;

    public MapObjInfo()
    {
        m_eMapObjectType = eMapObjectType.MOT_MAX;
        m_nDurability = 0;
        m_nCrashDamage = 0;
        m_Score = 0;
    }

    public void Copy(MapObjInfo MapInfo)
    {
        m_eMapObjectType = MapInfo.m_eMapObjectType;
        m_nDurability = MapInfo.m_nDurability;
        m_nCrashDamage = MapInfo.m_nCrashDamage;
        m_Score = MapInfo.m_Score;
    }
};

public class HitHelper
{
    static Ray projectileRay;

    public static List<Collider> HitTestProjectile(Collider collider , Vector3 vPos , Vector3 vDir , float distance , bool includeProjectile , bool includeWeapon ,uint[] excludeIds)
    {
        List<Collider> hits = null;

        projectileRay.origin = vPos;
        projectileRay.direction = vDir;

        EACObjManager.instance.For(x =>
        {
            for(int i = 0; i < excludeIds.Length; ++i)
            {
                if(x.GetObjID() == excludeIds[i])
                {
                    return false;
                }
            }  

            if(includeProjectile == false)
            {
                if (x.GetObjInfo().m_eObjType == eObjectType.CT_ITEMOBJECT)
                {
                    EA_CItem item = x as EA_CItem;

                    if (item != null && item.GetItemInfo().m_eItemType == eItemObjType.IK_Projectile)
                    {
                        return false;
                    }
                }
            }

            if(includeWeapon == false)
            {
                if (x.GetObjInfo().m_eObjType == eObjectType.CT_ITEMOBJECT)
                {
                    EA_CItem item = x as EA_CItem;

                    if (item != null && item.GetItemInfo().m_eItemType == eItemObjType.IK_WEAPON)
                    {
                        return false;
                    }
                }
            }
            
            Collider c = x.collider;

            if (c != null && collider && (collider.GetHashCode() != c.GetHashCode()))
            {
                if (c.bounds.Intersects(collider.bounds))
                {
                    if (hits == null)
                    {
                        hits = new List<Collider>();
                    }

                    hits.Add(c);
                    return true;
                }

                if (MathUtil.IntersectRayWithPoint(projectileRay , c.bounds.center , distance , c.bounds.extents.magnitude + collider.bounds.extents.magnitude))
                {

                    if (hits == null)
                    {
                        hits = new List<Collider>();
                    }

                    hits.Add(c);
                    return true;
                }
            }

            return false;
        });

        return hits;
    }

    static Ray curRay_raycast;

    public static Collider Raycast(Vector3 vPos, Vector3 vDir, float distance, eObjectType[] inclueObjs)
    {
        Collider hit = null;

        curRay_raycast.origin = vPos;
        curRay_raycast.direction = vDir;

        float dist = float.MaxValue;

        EACObjManager.instance.For(x =>
        {
            for (int i = 0; i < inclueObjs.Length; ++i)
            {
                if (x.GetObjInfo().m_eObjType != inclueObjs[i])
                {
                    return false;
                }
            }

            Collider c = x.collider;

            if (c != null)
            {
                RaycastHit hitInfo;

                if (c.Raycast(curRay_raycast, out hitInfo, distance))
                {
                    if(dist > hitInfo.distance)
                    {
                        hit = c;
                        dist = hitInfo.distance;
                    } 
                   
                    return false;
                }
            }

            return false;
        });

        return hit;
    }
} 


