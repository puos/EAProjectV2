using UnityEngine;


public class EA_CCharBPlayer : EA_CObjectBase
{
    public EA_CCharBPlayer()
    {
    }

    public override eObjectKind GetKind() { return eObjectKind.CK_ACTOR; }

    public virtual void SetLinkActor(EAActor  pLinkActor )	
    {
        if (m_pLinkActor != pLinkActor && pLinkActor != null)
        {
            m_pLinkActor = pLinkActor;
            m_pLinkActor.SetCharBase(this);
        }
    }

    public void DoSwitchWeapons(uint weaponState)
    {
        if (m_pLinkActor != null)
        {
            m_pLinkActor.DoSwitchWeapons(weaponState);
        }
    }

    public void PosInit()
    {
        if (m_pLinkActor != null)
        {
            m_pLinkActor.PosInit();
        }
    }
           
    public override bool SetObjInfo(ObjectInfo ObjInfo)
    {
        m_ObjInfo.Copy(ObjInfo);

        switch (m_ObjInfo.m_eObjState)
        {

            case eObjectState.CS_DEAD:
                {
                    if (m_pLinkActor != null)
                    {
                        m_pLinkActor.DeSpawnAction();
                        m_pLinkActor.SetCharBase(null);                     
                    }
                    
                    m_pLinkActor = null;

                }
                break;
        }

        base.SetObjInfo(ObjInfo);

        switch (m_ObjInfo.m_eObjState)
        {
            case eObjectState.CS_SETENTITY:
                {
                    if (m_pLinkActor != null)
                    {
                        m_pLinkActor.SetSkeleton();

                        ChangeParts();

                        m_pLinkActor.SetRenderer();

                        m_pLinkActor.SpawnAction();
                    }
                }
                break;
        }

        return true;
    }

    public void SetWeaponAttachment(eWeaponType _WeaponType,GameObject gameobject)
    {
        if (m_pLinkActor != null)
        {
            m_pLinkActor.SetWeaponAttachment(_WeaponType, gameobject);
        }
    }
       
    public virtual void OnAction(params object[] parms)
    {
        if (m_pLinkActor != null)
        {
            EAActor pActor = m_pLinkActor.gameObject.GetComponent<EAActor>();

            if (pActor != null)
            {
                pActor.OnAction(parms);
            }
        }
    }

    public void OnActorInputSendEvent(params object[] command)
    {
        if (m_pLinkActor != null)
        {
            EAActor pActor = m_pLinkActor.gameObject.GetComponent<EAActor>();

            if (pActor != null)
            {
                pActor.OnActorInputSendEvent(command);
            }
        }
    }

    protected EAActor m_pLinkActor = null;

    public virtual EAActor GetLinkIActor() { return m_pLinkActor; }

    // Find an object inside an actor.
    public override GameObject GetObjectInActor(string szBoneName)
    {
        if (m_pLinkActor != null)
        {
            EAActor pActor = m_pLinkActor.gameObject.GetComponent<EAActor>();

            if (pActor != null)
            {   return pActor.GetObjectInActor(szBoneName);
            }
        }

        return null;
    }
    /// <summary>
    /// String value in OnAction; Parsed as character
    /// </summary>
    /// <param name="szValues"></param>
    /// <returns></returns>
    protected string[] GetSenderValues(string szValues)
    {
        string szSeparateExt = ";";
        string[] arValueParser = szValues.Split(szSeparateExt.ToCharArray());
        return arValueParser;
    }
}
