

public class EA_CCharUser : EA_CCharBPlayer
{
    CharInfo m_ActorInfo = null;

    public EA_CCharUser()
    {        
    }
	  
	public bool	SetCharInfo( CharInfo charInfo )
    {
        m_ActorInfo = charInfo;

        return true;
    }
	    
    public CharInfo				GetCharInfo()	
    { return m_ActorInfo; 
    }

	public override bool	ResetInfo( eObjectState eChangeState )
    {
        m_ObjInfo.m_eObjState = eChangeState;
		SetObjInfo(  m_ObjInfo  );
		SetCharInfo( m_ActorInfo );
        return true;
    }

    public override void OnAction(params object[] parms)
    {
          base.OnAction(parms);
    }

    /// <summary>
    /// Change if there are set parts
    /// </summary>
    /// <returns></returns>
    public override bool ChangeParts()
    {
        EAActor actor = GetLinkIActor();

        if(actor != null)
        {
            actor.FindParts(m_ActorInfo.m_PartTblId);
        }

        return true;
    }
}