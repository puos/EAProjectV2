using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using EAObjID = System.UInt32;
using EATblID = System.UInt32;


public class EA_CObjectBase
{

    protected ObjectInfo		m_ObjInfo = new ObjectInfo();           //	Basic information
    protected GameObject	    m_pEntity;	        //	unity Engine Entity
    protected Collider          m_pCollider;      // unity Engine Collider

    public Collider collider { get { return m_pCollider; }  }

   	
    public EA_CObjectBase()
    {     
    }

    public virtual eObjectKind GetKind() { return eObjectKind.CK_OBJECT; }

    public EAObjID	GetObjID()	
    {
        return m_ObjInfo.m_GDObjId; 
    }

    public virtual bool SetObjInfo(ObjectInfo ObjInfo)
    {
        m_ObjInfo.Copy(ObjInfo);

        switch (m_ObjInfo.m_eObjState)
        {
            case eObjectState.CS_MYENTITY:
                {
                }
                break;

            case eObjectState.CS_READY:
            case eObjectState.CS_UNENTITY:
            case eObjectState.CS_DEAD:
                {
                    if (m_pEntity != null)
                    {
                        EA_ObjectFactory.EntityUnSetting(this);
                    }
                    
                }
                break;

            
            case eObjectState.CS_SETENTITY:
                {
                    if (m_pEntity == null)
                    {
                        EA_ObjectFactory.EntitySetting(this, m_ObjInfo);
                    }

                    ChangeModel(m_ObjInfo.m_ModelTypeIndex);
                }
                break;

            case eObjectState.CS_HIDE:
                {
                    if (m_pEntity != null)
                    {
                        m_pEntity.SetActive(false);
                    }
                }
                break;

            case eObjectState.CS_SHOW:
                {
                    if (m_pEntity != null)
                    {
                        m_pEntity.SetActive(true);
                    }
                }
                break;
        }

        ReSetWorldPosDir(m_ObjInfo.spawnPos, m_ObjInfo.spawnAngle);

        if (m_pEntity != null)
        {
            if (!string.IsNullOrEmpty(m_ObjInfo.m_strGameName))
            {
                m_pEntity.name = m_ObjInfo.m_strGameName;
            }
        }

        return true;
    } 

	public ObjectInfo				GetObjInfo() { return m_ObjInfo; }

    public virtual void SetLinkEntity(GameObject pLinkEntity)
    {
        if (m_pEntity != pLinkEntity)
        {
            m_pEntity = pLinkEntity;
        }

        if(m_pEntity != null)
        {
            m_pCollider = m_pEntity.GetComponent<Collider>();
        }  
        else
        {
            m_pCollider = null;
        }   
    }

	public GameObject				GetLinkEntity()		{ return m_pEntity; }

	public virtual bool ChangeModel(string modelType)
    {
        return true;
    } 
	
    public virtual	bool ChangeParts() { return true; }
    
    public virtual bool  ChangeAnimation(EATblID _iAniType)
    {
        return true;
    }
    
    public virtual bool ChangeTexture(EATblID _iTexType)
    {
        return true;
    }

    public virtual bool ResetInfo(eObjectState eChangeState)
    {
        m_ObjInfo.m_eObjState = eChangeState;
        SetObjInfo( m_ObjInfo );
        return true;
    }


    // The ability to change the name of a character  [9/16/2009 jgb] 
    public void ChangeName(string strGameName)
    {
        if (!m_ObjInfo.m_strGameName.Equals(strGameName))
        {
            m_ObjInfo.SetObjName(strGameName);
        }

        if (m_pEntity != null)
        {
            if (m_pEntity.name != strGameName)
            {
                m_pEntity.name = strGameName;
            }
        } 
    }

    // Change the object's transform
    public bool ReSetWorldPosDir(float[] fPos, float[] fAngle)
    {
        if (m_pEntity == null)
        {
            return false;
        }

        Vector3 pos = Vector3.zero;
        pos.Set(fPos[0], fPos[1], fPos[2]);

        if (!Vector3.Equals(pos, m_pEntity.transform.localPosition))
        {
           m_pEntity.transform.localPosition = pos;
        }

        Vector3 tAng = Vector3.zero;
        tAng.Set(fAngle[0], fAngle[1], fAngle[2]);

        if (!Vector3.Equals(tAng, m_pEntity.transform.localRotation.eulerAngles))
        {
           m_pEntity.transform.localRotation = Quaternion.Euler(tAng);
        }   
             
        return true;
    }

    public virtual GameObject GetObjectInActor(string szBoneName)
    {
        return null;
    }
}
