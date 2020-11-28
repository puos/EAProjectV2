using UnityEngine;
using System.Collections;
using System;

using EAObjID = System.UInt32;
using Debug = EAFrameWork.Debug;

public class EAWeapon : EAItem
{
    EA_ItemAttackWeaponInfo WeaponInfo = new EA_ItemAttackWeaponInfo();

    float m_fShootCoolTime  = 0;
       
    bool bFiring      = false;
    bool bHold        = false;
        
    public GameObject fireDummyObject  = null;
 
    public float m_fWeaponSpawnWaitTime = 1.0f;
    public float m_fWeaponSpawnTime     = 1.0f;


    // Update is called once per frame
    protected override void OnUpdate()
    {
        if (bFiring == true)
        {
            if(fireDummyObject != null)
            {
                DebugExtension.DebugArrow(fireDummyObject.transform.position, transform.forward * 1.5f, Color.magenta);
            }

            m_fShootCoolTime += Time.deltaTime;
            
            if (WeaponInfo.fFiringTime < m_fShootCoolTime)
            {
                if(WeaponInfo.bAutoMode)
                {
                    m_fShootCoolTime = 0;
                    ShootAction();
                }
            }
        }
    }

    //  [4/12/2015 puos] Manual setting of dummy object
    public void SetDummyObject(GameObject _DummyObject)
    {
        fireDummyObject = _DummyObject;
    }

    public void SetDummyObject(string dummyObject_id)
    {
        GameObject _DummyObject = GetObjectInItem(dummyObject_id);

        Debug.Assert(_DummyObject != null , "EAWeapon DummyObject is null");

        if(_DummyObject != null)
        {
            fireDummyObject = _DummyObject;
        }
    }
        
    public virtual void StartFire()
    {
        bFiring         = true;
        m_fShootCoolTime  = 0;
    }

    public virtual void StopFire()
    {
        bFiring     = false;
        m_fShootCoolTime = 0;
    }

    public virtual bool IsFiring()
    {
        return bFiring;
    }
   
    public virtual void Shoot()
    {
        switch (WeaponInfo.weaponType)
        {
            case eWeaponType.eWT_Unarmed:
                {
                
                }
                break;

            case eWeaponType.eWT_Sword:
                {
                
                }
                break;

            case eWeaponType.eWT_Bow:
                {
                }
                break;

            case eWeaponType.eWT_Rifle:
            case eWeaponType.eWT_Pistol:
            case eWeaponType.eWT_Turret:
            case eWeaponType.eWT_Breathing:  
                {
                    FireShoot();
                }
                break;
        }
    }
   
    public void FireShoot()
    {
        EAActor pActor = GetOwnerActor();

        Debug.Assert(pActor != null, "FireShoot Actor is null");
     
        if (fireDummyObject != null && pActor != null)
        {
            ObjectInfo ObjInfo = new ObjectInfo();

            ObjInfo.spawnPos[0] = fireDummyObject.transform.position.x;
            ObjInfo.spawnPos[1] = fireDummyObject.transform.position.y;
            ObjInfo.spawnPos[2] = fireDummyObject.transform.position.z;

            ObjInfo.spawnAngle[0] = fireDummyObject.transform.rotation.eulerAngles.x;
            ObjInfo.spawnAngle[1] = fireDummyObject.transform.rotation.eulerAngles.y;
            ObjInfo.spawnAngle[2] = fireDummyObject.transform.rotation.eulerAngles.z;

            ObjInfo.m_ModelTypeIndex = WeaponInfo.uProjectileModelType;
            ObjInfo.m_objClassType   = WeaponInfo.m_objProjectileClassType;
            ObjInfo.SetObjName("projectile");

            ItemObjInfo itemInfo = new ItemObjInfo();

            EA_CCharBPlayer pCharBase = pActor.GetCharBase();

            if (pCharBase != null)
            {
                itemInfo.m_HavenUser = pCharBase.GetObjID();
            }

            if (ObjInfo.m_objClassType == default(Type))
            {
                ObjInfo.m_objClassType = typeof(EAProjectile);
            } 

            itemInfo.m_eItemType = eItemObjType.IK_Projectile;
            EA_CItem pItem = EACObjManager.instance.CreateItem(ObjInfo, itemInfo);

            // Create if no bullet. Do not use pool
            if (pItem.GetLinkEntity() == null)
            {
                GameObject pGameObject = new GameObject("projectile", ObjInfo.m_objClassType);

                pGameObject.transform.position = new Vector3(ObjInfo.spawnPos[0], ObjInfo.spawnPos[1], ObjInfo.spawnPos[2]);
                pGameObject.transform.eulerAngles = new Vector3(ObjInfo.spawnAngle[0], ObjInfo.spawnAngle[1], ObjInfo.spawnAngle[2]);

                pItem.SetLinkEntity(pGameObject);
                pItem.SetLinkItem(pGameObject.GetComponent<EAProjectile>());
            }

            GameObject item = pItem.GetLinkEntity();
            EAProjectile pProjectile = null;

            if (item != null)
            {
                pProjectile = item.GetComponent<EAProjectile>();
            } 

            if (pProjectile != null)
            {
                if (ProjectileTransform.instance != null)
                {
                    pProjectile.transform.SetParent(ProjectileTransform.instance.transform);
                }

                // use sphereCollider
                Collider c = pProjectile.GetComponent<Collider>();
                
                if(c == null)
                {
                    SphereCollider sc = pProjectile.gameObject.AddComponent<SphereCollider>();
                    sc.radius = WeaponInfo.projectileRadius;
                }
                
                pProjectile.SetProjectileDir(fireDummyObject.transform.forward);

                pProjectile.SetOwnActor(itemInfo.m_HavenUser);

                pProjectile.SetWeapon(GetItemBase().GetObjID());

                pProjectile.SetWeaponInfo(WeaponInfo);

                Debug.Log("fire shoot - weapon user id :" + itemInfo.m_HavenUser + " projectile type : " + WeaponInfo.projectileType);
            }
            
            pItem.Use();
        }
    }
    
    public virtual void ShootAction()
    {
        Shoot();
    }

    public override void OnAction(params object[] parms)
    {
        string action       = (string)parms[0];

        switch(action)
        {
            case "startAttack":
                {
                    StartFire();
                }
                break;

            case "attack":
                {
                    StartFire();
                    ShootAction();
                }
                break;

            case "lowerAttack":
                {
                    StopFire();
                }
                break;
        }  
    }

    public EA_ItemAttackWeaponInfo GetAttackWeaponInfo()
    {
        return WeaponInfo;
    }
    
    EAProjectile SpawnAmmo()
    {
        return null;
    }

    // Carry a weapon
    public void RaiseWeapon()
    {
        EA_CItem pItemBase = GetItemBase();

        if (pItemBase != null)
        {
            if (pItemBase.GetItemInfo().m_iItemIndex != CObjGlobal.InvalidItemID)
            {
                EA_CItemUnit pItemUnit = EA_ItemManager.instance.FindItemUnit(pItemBase.GetItemInfo().m_iItemIndex);

                if (pItemUnit != null)
                {
                    WeaponInfo.Copy(pItemUnit.GetAttackWeaponInfo());
                }
            }
        }

        StopFire();
    }
        
    /// <summary>
    /// Id of the weapon owner
    /// </summary>
    /// <returns></returns>
    public EAObjID GetOwnerActorId()
    {
        EAObjID OwnActorId = CObjGlobal.InvalidObjID;

        EAActor pActor = GetOwnerActor();

        if (pActor != null && pActor.GetCharBase() != null)
        {
            OwnActorId = pActor.GetCharBase().GetObjID();
        }

        return OwnActorId;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pAttackedActor"></param>
    protected void Slice(EAActor pAttackedActor)
    {
        EAActor pActor = GetOwnerActor();

        //  [4/11/2018 puos] bug fixed attacker dies
        if (pAttackedActor != null)
        {
            if (pActor != null)
            {
                //  [4/18/2018 puos] Collided object must not be self
                if (pActor.GetCharBase().GetObjID() != pAttackedActor.GetCharBase().GetObjID())
                {
                    EA_GameEvents.onAttackMsg(pActor.GetCharBase(), pAttackedActor.GetCharBase(), GetAttackWeaponInfo(), CObjGlobal.InvalidObjID);
                }
            }
            else
            {
                EA_GameEvents.onAttackMsg(null, pAttackedActor.GetCharBase(), GetAttackWeaponInfo(), CObjGlobal.InvalidObjID);
            }
        }
    }
}  
