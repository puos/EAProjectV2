using UnityEngine;

using EAObjID = System.UInt32;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

public class EAProjectile : EAItem
{
    protected EA_ItemAttackWeaponInfo WeaponInfo = new EA_ItemAttackWeaponInfo();

    protected Vector3 m_vStartPos;
    protected Vector3 m_vStartDir;
    protected Vector3 m_vDir;
    protected Vector3 m_prePos;

    EAObjID m_weaponId = CObjGlobal.InvalidObjID;

    Collider m_collider = null;

    uint[] excludeIds = new uint[2];

    protected override void OnInit()
    {
        m_collider = GetComponent<Collider>();
    }

    protected override void OnUpdate()
    {
        switch (WeaponInfo.projectileType)
        {
            case eProjectileType.ePT_General:
            case eProjectileType.ePT_Custom_B:
            case eProjectileType.ePT_Custom_A:
                {
                    GeneralProjectileType();
                }
                break;

            case eProjectileType.ePT_Guided:
                {
                    GuidedProjectileType();
                }
                break;
        }

        HitCheck();

        Vector3 vDistancePos = transform.position - m_vStartPos;

        float fDistance = vDistancePos.magnitude;

        if (fDistance > WeaponInfo.fKillDistance)
        {
            RemoveProjectile();
        }
    }

    protected void HitCheck()
    {
        if (GetItemBase() == null)
        {
            return;
        }

        excludeIds[0] = GetItemBase().GetItemInfo().m_HavenUser;
        excludeIds[1] = m_weaponId;

        List<Collider> hits = HitHelper.HitTestProjectile(m_collider, m_prePos, m_vDir, WeaponInfo.fProjectileSpeed * Time.deltaTime, false , false, excludeIds);

        if (hits != null && hits.Count > 0)
        {
            OnTriggerEnter(hits[0]);
        }

        Debug.DrawLine(m_prePos, transform.position, Color.blue);
        m_prePos = transform.position;
    }

    public void RemoveProjectile()
    {
        EA_CItem pItemBase = GetItemBase();
        
        if (pItemBase != null)
        {
           EACObjManager.instance.RemoveItem(pItemBase.GetObjID());
        }
    }

    protected void GeneralProjectileType()
    {
        Debug.DrawLine(transform.position, transform.position + m_vDir * WeaponInfo.fProjectileSpeed , Color.yellow);
        
        Vector2 vPos = transform.position + m_vDir * WeaponInfo.fProjectileSpeed * Time.deltaTime;

        SetVPos(vPos);
    }

    protected void GuidedProjectileType()
    {
        GameObject pEntity = null;
        EA_CObjectBase pObjectBase = null;

        if (!string.IsNullOrEmpty(WeaponInfo.m_strTargetActorBoneName))
        {
            pObjectBase = EACObjManager.instance.GetActor(WeaponInfo.m_TargetActorId);
        }
        else
        {
            pObjectBase = EACObjManager.instance.GetGameObject(WeaponInfo.m_TargetActorId);
        }

        if (pObjectBase != null && pObjectBase.GetLinkEntity())
        {
            pEntity = pObjectBase.GetLinkEntity();

            Collider c = pObjectBase.collider;

            Vector3 vColliderPos = Vector3.zero;

            if(c != null)
            {
                vColliderPos = c.bounds.extents;
            }

            Vector3 vTargetPos = (pEntity.gameObject.transform.position) + vColliderPos;

            Vector3 vDiff = vTargetPos - transform.position;

            m_vDir = vDiff;
            m_vDir.Normalize();
        } 
        else
        {
            WeaponInfo.m_TargetActorId = CObjGlobal.InvalidObjID;
            m_vDir = m_vStartDir;
        }   
        
        Debug.DrawLine(transform.position , transform.position + m_vDir * WeaponInfo.fProjectileSpeed * Time.deltaTime, Color.yellow);

        Vector3 vPos = transform.position + m_vDir * WeaponInfo.fProjectileSpeed * Time.deltaTime;

        SetVPos(vPos);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vPos"></param>
    public void SetVPos(Vector3 vPos)
    {
        transform.position = vPos;
    }

    /// <summary>
    /// 
    /// </summary>
    private void RayProjectileType()
    {
        Ray ray = new Ray(transform.position, m_vDir);

        List<RaycastHit> hits = new List<RaycastHit>();

        EACObjManager.instance.For(x => 
        {
            GameObject obj = x.GetLinkEntity();

            if(obj != null)
            {
                float distance = (obj.transform.position - transform.position).magnitude;

                if(distance <= WeaponInfo.fKillDistance)
                {
                    Collider c = x.collider;

                    if(c != null)
                    {
                        RaycastHit hitInfo;

                        if(c.Raycast(ray,out hitInfo , WeaponInfo.fKillDistance))
                        {
                            hits.Add(hitInfo);
                        }    
                    }
                }
            }

            return false;
        });

        if (hits != null && 0 < hits.Count)
        {
            hits.Sort(delegate(RaycastHit a, RaycastHit b) 
            {
                // 3 decimal places
                return (int)((a.distance - b.distance) * 1000);
            });

            EAActor pActor = GetOwnerActor();

            foreach (RaycastHit hit in hits)
            {
                if(hit.collider != null)
                {
                    Collider c = hit.collider;

                    EAActor pAttackedActor = c.gameObject.GetComponent<EAActor>();

                    if( (pActor && pAttackedActor) && 
                        (pActor.GetCharBase().GetObjID() != pAttackedActor.GetCharBase().GetObjID()))
                    {
                        OnTriggerEnter(c);
                        break;
                    } 
                }
            } 
        }

        ExPlosion();
    } 

    void OnTriggerEnter(Collider c)
    {
        EAActor pAttackedActor = c.gameObject.GetComponent<EAActor>();

        EAActor pActor = GetOwnerActor();

        if(pAttackedActor != null)
        {
            //  [4/11/2018 puos] attacker is not dead
            if (pActor != null)
            {
                if (pActor.GetCharBase().GetObjID() != pAttackedActor.GetCharBase().GetObjID())
                {
                    EA_GameEvents.onAttackMsg(pActor.GetCharBase(), pAttackedActor.GetCharBase(), WeaponInfo, GetItemBase().GetObjID());
                }
                else
                {
                    //  [12/2/2019 puos] If attacker and attacked are the same, passing
                    return;
                } 
            }
            else
            {
                EA_GameEvents.onAttackMsg(null, pAttackedActor.GetCharBase(), WeaponInfo, GetItemBase().GetObjID());
            }
        }      

        if(WeaponInfo.onExplosionEvent != null)
        {
            WeaponInfo.onExplosionEvent(GetItemBase().GetObjID());
        }  
    }

    public void ExPlosion(bool bProjectileDestroy = true)
    {
        if (bProjectileDestroy == true)
        {
            RemoveProjectile();
        }
    }
        
    public void SetWeaponInfo(EA_ItemAttackWeaponInfo _WeaponInfo)
    {
        WeaponInfo.Copy(_WeaponInfo);
    }

    public EA_ItemAttackWeaponInfo GetWeaponInfo()
    {
        return WeaponInfo;
    }

    public void SetOwnActor(EAObjID _ActorId)
    {
        GetItemBase().GetItemInfo().m_HavenUser = _ActorId;
    }

    public void SetWeapon(EAObjID _weaponId)
    {
        m_weaponId = _weaponId;
    }

    /// <summary>
    /// If the type is derived, set the target
    /// </summary>
    /// <param name="_ActorId"></param>
    public void SetTargetActorId(EAObjID _ActorId)
    {
        WeaponInfo.m_TargetActorId = _ActorId;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vDir"></param>
    public void SetProjectileDir(Vector3 vDir)
    {
        m_vDir      = vDir;
        m_vStartDir = vDir;
    }
        
    public override bool Use()
    {
       m_vStartPos   = transform.position;
       m_prePos      = m_vStartPos;

        switch (WeaponInfo.projectileType)
        {
            case eProjectileType.ePT_General:
            case eProjectileType.ePT_Custom_B:
            case eProjectileType.ePT_Custom_A:
                {
                    GeneralProjectileType();
                }
                break;
            case eProjectileType.ePT_Ray:
                {
                    RayProjectileType();
                }
                break;

            case eProjectileType.ePT_Guided:
                {
                    GuidedProjectileType();
                }
                break;
        }

       return true;
    }

    private void OnDrawGizmos()
    {
        DebugExtension.DrawCircle(transform.position, Vector3.up, Color.magenta , m_collider.bounds.extents.magnitude);
    }
}
