using UnityEngine;
using Debug = EAFrameWork.Debug;

public class EAActorInput : MonoBehaviour
{
    protected EAFSMMaker fsmMaker = new EAFSMMaker();

    enum eInputType
    {
        ePlayer_Input,
        eAI_Input,
    }

    public delegate void OnAction(params object[] parms);
    public OnAction actorSendEvent;

    private void Awake()
    {
        
    }

    public virtual void SetLinkActor(EAActor pActor)
    {
        if (pActor != null)
        {
            actorSendEvent -= pActor.OnAction;
            actorSendEvent += pActor.OnAction;

            pActor.actorInputSendEvent -= OnActorRecieveEvent;
            pActor.actorInputSendEvent += OnActorRecieveEvent;
        }
    }
        
    public void OnActorSendEvent(params object[] parms)
    {
        if (actorSendEvent != null)
        {
            actorSendEvent(parms);
        }  
    }

    public virtual void OnActorRecieveEvent(params object[] command)
    {
    }

    private void Update()
    {
        fsmMaker.Update();
    }
}
