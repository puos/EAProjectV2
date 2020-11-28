using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = EAFrameWork.Debug;

public class EA_CEffectNode
{
    public EA_EffectBaseInfo m_EffectBaseInfo = new EA_EffectBaseInfo();
    protected EAEffectModule m_pEffectEntity  = null;

    public virtual void SetLinkEffect(EAEffectModule pLinkEffect)
    {
        if (m_pEffectEntity != pLinkEffect && pLinkEffect != null)
        {   
            m_pEffectEntity = pLinkEffect;
            m_pEffectEntity.Init(GetEffectBaseInfo());
        }
    }

    public EA_EffectBaseInfo GetEffectBaseInfo() { return m_EffectBaseInfo; }

    public EAEffectModule GetLinkEffect() { return m_pEffectEntity; }

    public virtual bool SetObjInfo(EA_EffectBaseInfo EffectBaseInfo)
    {
        m_EffectBaseInfo.Copy(EffectBaseInfo);

        switch (m_EffectBaseInfo.m_eEffectState)
        {
            case eEffectState.ES_Load:
                {
                    if (m_pEffectEntity == null)
                    {
                        EA_CEffectResourceLoader.EffectSetting(this, EffectBaseInfo);
                    }
                }
                break;

            case eEffectState.ES_UnLoad:
                {
                    if (m_pEffectEntity != null)
                    {
                        EA_CEffectResourceLoader.EffectUnSetting(this);
                    }
                }
                break;

            case eEffectState.ES_Start:
                {
                    if (m_pEffectEntity != null)
                    {
                        m_pEffectEntity.Play();               
                    }
                }
                break;
            
            case eEffectState.ES_Stop:
                {
                    if (m_pEffectEntity != null)
                    {
                        m_pEffectEntity.Stop();
                    }
                }
                break;

            case eEffectState.ES_ForceStop:
                {
                    if (m_pEffectEntity != null)
                    {
                        m_pEffectEntity.ForceStop();
                    }
                }
                break;
        }

        ReSetWorldPosDir(EffectBaseInfo.m_EmitPos, EffectBaseInfo.m_EmitAngle);

        return true;
    }

    bool ReSetWorldPosDir(float[] fPos, float[] fAngle)
    {
        if (m_pEffectEntity == null)
        {
            return false;
        }

        Vector3 pos = Vector3.zero;
        Vector3 tAng = Vector3.zero;
        
        pos.Set(fPos[0], fPos[1], fPos[2]);
        tAng.Set(fAngle[0], fAngle[1], fAngle[2]);

        bool isChangePos = (pos.magnitude > 0) ? true : false;
        bool isChangeAng = (tAng.magnitude > 0) ? true : false;

        if (isChangePos || isChangeAng)
        {
           Quaternion q = Quaternion.identity;


            if (
                m_EffectBaseInfo.m_eAttachType == eEffectAttachType.eLinkOffset || 
                m_EffectBaseInfo.m_eAttachType == eEffectAttachType.eLinkBone
               )
            {
                if (isChangePos == true)
                {
                    m_pEffectEntity.transform.localPosition = pos;
                }
                
                if (isChangeAng == true)
                {
                    m_pEffectEntity.transform.localRotation = Quaternion.Euler(tAng);
                }
            }
            else
            {
                if (isChangePos == true)
                {
                    m_pEffectEntity.transform.position = pos;
                }
  
                if (isChangeAng == true)
                {
                    m_pEffectEntity.transform.rotation = Quaternion.Euler(tAng);
                }
            }
                      

            //m_pEntity->SetWorldTM(Matrix34::Create(Vec3(1,1,1), Quat(tAng), pos));
        }

        return true;
    }

    public virtual bool ResetInfo(eEffectState eChangeState)
    {
        m_EffectBaseInfo.m_eEffectState = eChangeState;
        SetObjInfo(m_EffectBaseInfo);
        return true;
    }

    public void Start()
    {
        ResetInfo(eEffectState.ES_Start);
    }

    public void Stop()
    {
        ResetInfo(eEffectState.ES_Stop);
    }

    public void ForceStop()
    {
        ResetInfo(eEffectState.ES_ForceStop);
    }
   
    public void AutoDelete()
    {
        if (m_pEffectEntity != null && GetEffectBaseInfo().m_eLifeType == eEffectLifeType.eLimitTime)
        {
            m_pEffectEntity.AutoDelete();
        }
    }
}
