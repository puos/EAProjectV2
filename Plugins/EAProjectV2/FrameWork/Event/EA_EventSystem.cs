using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

public interface IEAEventTarget 
{
    void EventProcess();

    string GetGroupName();

    string GetTargetName();

    void SetGroupName(string sGroupName);

    void SetTargetName(string sTargetName);
}


public class EAEventTargetGroup
{ 

   public EAEventTargetGroup( string sName )
   {
       m_sName = sName;
       m_oEventTargetTable.Clear();
   }
	
   public bool AddEventTarget( IEAEventTarget pTarget) // Add event target
    {
       if (pTarget == null)
       {
           return false;
       }

       if (pTarget.GetTargetName() == "")
       {
           return false;
       }

       IEAEventTarget pInfo = GetEventTarget(pTarget.GetTargetName() );

       if(pInfo != null)
       {
            // Cancel registration because it is registered
            return false;
       }

       m_oEventTargetTable.Add(pTarget.GetTargetName(), pTarget);

       return true;
   }

	
   public void RemoveEventTarget( IEAEventTarget pTarget) // Delete event target
    {
       if (pTarget == null)
       {
           return;
       }

       if (pTarget.GetTargetName() == "")
       {
           return;
       }

       m_oEventTargetTable.Remove(pTarget.GetTargetName());
           
   }

	/*! Find event target */
	public IEAEventTarget		GetEventTarget(  string  sTargetName )
    {
        if (m_oEventTargetTable.ContainsKey(sTargetName) == true)
        {
            return m_oEventTargetTable[sTargetName];
        }  
        
        return null;
    }

    public string GetTargetGroupName()
    {
       return  m_sName;
    }
    
	Dictionary<string,IEAEventTarget> m_oEventTargetTable = new Dictionary<string, IEAEventTarget>();			// event target table
	string						     m_sName;						// group name
}


public class EA_EventSystem : EAGenericSingleton<EA_EventSystem>
{
    public enum eEA_EventGroup
    {
        eUIGroup,
        eNetworkGroup,
        eSceneGroup,
    }


    public EA_EventSystem()
    {
        m_oEventTargetTable.Clear();
    }

    void RemoveAllEventTargetGroup()
    {
        m_oEventTargetTable.Clear();
    }

    public void SendMessageTo()
    {
        EAEventTargetGroup pGroup = GetEventTargetGroup(EA_EventMsg.m_sGroupName);

        if (pGroup == null)
        {
            return;
        }

        IEAEventTarget pTarget = pGroup.GetEventTarget(EA_EventMsg.m_sTargetName);

        if (pTarget == null)
        {
            return;
        }

        pTarget.EventProcess();

        //Debug.Log(rMsg.m_sGroupName + "/" + rMsg.m_sTargetName  + " SendMessageTo " + rMsg.m_sEventName + "/" + rMsg.m_sBuffer);
    }

    EAEventTargetGroup GetEventTargetGroup(string sGroupName)
    {
        if (m_oEventTargetTable.ContainsKey(sGroupName) == true)
        {
            return m_oEventTargetTable[sGroupName];
        }

        return null;
    }

    public bool AddEventTarget(IEAEventTarget pTarget)
    {
        if (pTarget == null)
        {
            return false;
        }

        if (pTarget.GetGroupName() == "")
        {
            return false;
        }

        EAEventTargetGroup pGroup = GetEventTargetGroup(pTarget.GetGroupName());

        if (pGroup == null)
        {
            pGroup = new EAEventTargetGroup(pTarget.GetGroupName());
            m_oEventTargetTable.Add(pTarget.GetGroupName(), pGroup);
        }

        return pGroup.AddEventTarget(pTarget);
    }

    public void RemoveEventTarget(IEAEventTarget pTarget)
    {
        if (pTarget.GetGroupName() == "")
        {
            return;
        }

        EAEventTargetGroup pGroup = GetEventTargetGroup(pTarget.GetGroupName());

        if (pGroup == null)
            return;

        pGroup.RemoveEventTarget(pTarget);
    }

    Dictionary<string, EAEventTargetGroup> m_oEventTargetTable = new Dictionary<string, EAEventTargetGroup>();	// event target group table
}