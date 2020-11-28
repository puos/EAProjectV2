
public class EA_CCharNPC : EA_CCharBPlayer
{
    NPCInfo m_NPCInfo = null;
      
    public EA_CCharNPC()
    {
    }
	
	public bool	SetNPCInfo( NPCInfo npcInfo )
    {
        m_NPCInfo = npcInfo;
  
        return true;
    }
    
	public NPCInfo 	GetNPCInfo()	
    {
        return m_NPCInfo; 
    }

    public override bool ResetInfo(eObjectState eChangeState)
    {
        m_ObjInfo.m_eObjState = eChangeState;
		SetObjInfo( m_ObjInfo );
		SetNPCInfo( m_NPCInfo );

        return true;
    }

}
