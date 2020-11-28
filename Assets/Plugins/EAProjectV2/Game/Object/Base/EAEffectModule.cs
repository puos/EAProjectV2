using UnityEngine;
using System.Collections;
using Debug = EAFrameWork.Debug;

public class EAEffectModule : MonoBehaviour
{
    EA_EffectBaseInfo m_pEffectBaseInfo  = new EA_EffectBaseInfo();
    Sfx    m_pSfx  = null;
    
    bool m_bAutoDelete = false;

    // Use this for initialization
    protected void Start()
    {
    }

    // Update is called once per frame
    protected void LateUpdate()
    {
        if (
            m_pEffectBaseInfo.m_EffectResourceType == eEffectResourceType.eDecal &&
            m_pEffectBaseInfo.m_bForceAxis == true
           )
        {
            EA_CObjectBase pObjectBase = EACObjManager.instance.GetGameObject(m_pEffectBaseInfo.m_AttachObjectId);

            if (pObjectBase != null && pObjectBase.GetLinkEntity() != null)
            {
                Vector3 position = Vector3.zero;

                position.x = pObjectBase.GetLinkEntity().transform.position.x;
                position.y = m_pEffectBaseInfo.m_fForceYpos;
                position.z = pObjectBase.GetLinkEntity().transform.position.z;

                transform.position = position;
            }
        }

        if (m_pSfx != null && m_bAutoDelete == true)
        {
            if (m_pSfx.IsAlive() == false)
            {
                if (m_pEffectBaseInfo.m_GDEffectId != CObjGlobal.InvalidEffectID)
                {
                    //Debug.Log("delete sfx :" + m_pEffectBaseInfo.m_GDEffectId );
                    EACEffectManager.instance.RemoveEffect(m_pEffectBaseInfo.m_GDEffectId);
                }
            }
        }
    }

    public void Init(EA_EffectBaseInfo pEffectBaseInfo)
    {
        m_pEffectBaseInfo.Copy(pEffectBaseInfo);
        ResetComponent();
    }
    
    private void ResetComponent()
    {
        m_pSfx = GetComponent<Sfx>();
    }

    public EA_EffectBaseInfo GetEffectBaseInfo() { return m_pEffectBaseInfo; }

    public void Play()
    {
        if (m_pSfx != null)
        {
            m_pSfx.StartSfx();
        }
    }

    public void Stop()
    {
        if (m_pSfx != null)
        {
           m_pSfx.StopSfx();
        }
    }

    public void ForceStop()
    {
        if (m_pSfx != null)
        {
            m_pSfx.StopSfx();
        }
    }

    public void AutoDelete()
    {
        m_bAutoDelete = true;
    }
    
}
