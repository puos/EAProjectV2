
public class EA_CCharMob : EA_CCharBPlayer
{
    MobInfo m_MobInfo = null;

    public EA_CCharMob()
    {
    }

    public bool SetMobInfo(MobInfo mobInfo)
    {
        m_MobInfo = mobInfo;

        //	Change according to material
        if (m_pLinkActor != null)
        {
           
        }

        return true;
    }

    public MobInfo GetMobInfo()
    {
        return m_MobInfo; 
    }

    public override bool	ResetInfo( eObjectState eChangeState )
    {
        m_ObjInfo.m_eObjState = eChangeState;
		SetObjInfo(  m_ObjInfo  );
        SetMobInfo(  m_MobInfo );
        return true;
    }
         

    public void OnActorInput(string command)
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
}
