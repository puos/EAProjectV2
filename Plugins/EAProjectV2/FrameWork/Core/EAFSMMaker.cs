using System.Collections;
using System.Collections.Generic;
using System;
using Debug = EAFrameWork.Debug;


public class EAFSMMaker 
{
    public delegate int EACommanMethod(EAFSMCommand command);           

    public class EAFSMCommand
    {
        public const int current     = -1;
        public const int next_first  = 0;
        public const int next_second = 1;
        public const int next_third  = 2;
        public const int next_fourth = 3;

        public enum eFSMState
        {
            eInit,
            eRun,
            eComplete,
        }

        public uint[]                m_eTransitionCommand;
        
        public EACommanMethod        m_funcCommand = null;

        public eFSMState             m_efsmState = eFSMState.eInit;

        public EAFSMCommand(EACommanMethod funcCommand, uint eCommand1 = 0, uint eCommand2 = 0, uint eCommand3 = 0, uint eCommand4 = 0)
        {
            m_eTransitionCommand = new uint[4];

            m_eTransitionCommand[0] = eCommand1;
            m_eTransitionCommand[1] = eCommand2;
            m_eTransitionCommand[2] = eCommand3;
            m_eTransitionCommand[3] = eCommand4;

            m_funcCommand = funcCommand;
        }
        
        public void Run()
        {
            m_efsmState = EAFSMCommand.eFSMState.eRun;
        }

        public void Init()
        {
            m_efsmState = EAFSMCommand.eFSMState.eInit;
        }

        public void End()
        {
            m_efsmState = EAFSMCommand.eFSMState.eComplete;
        }
   }

    private Queue<uint> m_CurrentCommandQueue  = new Queue<uint>();
    
    private Dictionary<uint, EAFSMCommand> myState = new Dictionary<uint, EAFSMCommand>();

    public uint m_nCurrState = 0;
    public uint m_nPrevState = 0;

    public EAFSMMaker()
    {
        Init();
    }

    // check if fsm was triggered first
    private bool bInit = false;

    public void Init()
    {
        if (bInit == false)
        {
            myState.Clear();
            m_CurrentCommandQueue.Clear();
            bInit = true;
        } 
    }

    public void Clear()
    {
        myState.Clear();
        m_CurrentCommandQueue.Clear();
        bInit = false;
    }


   public void Update() 
   {
       while (m_CurrentCommandQueue.Count > 0)
       {
           uint eCommandType = m_CurrentCommandQueue.Peek();
                        
           if (myState.ContainsKey(eCommandType))
           {
               EAFSMCommand command = myState[eCommandType];

               int nCheck = -1;

               if (command.m_funcCommand != null)
               {
                   nCheck = command.m_funcCommand(command);
                    // puos 20180830 Fixed bug where incorrectly referenced index would go to run without going through init
                    Run(eCommandType);
               }

               if (nCheck >= 0 && command.m_eTransitionCommand.Length > nCheck)
               {
                   bool bsearchCommand = TransitionCommand(command.m_eTransitionCommand[nCheck]);

                    // puos 20140917 Fixed bug where infinite value could be found if command value could not be found
                    if (bsearchCommand == false)
                   {
                        break;
                   }
               }
               else
               {
                    break;
               }

           }
           else
           {
                break;
           } 
       }
 	}

    void Run(uint eCommandType)
    {
        if (myState.ContainsKey(eCommandType))
        {
             EAFSMCommand command = myState[eCommandType];
             command.Run();
        }  
    }

    void Init(uint eCommandType)
    {
        if (myState.ContainsKey(eCommandType))
        {
            EAFSMCommand command = myState[eCommandType];
            command.Init();
        }
    }

    //  [4/22/2014 puos] Transition state
    public bool TransitionCommand(uint eCommand)
    {  
        if (myState.ContainsKey(eCommand))
        {
            if (m_CurrentCommandQueue.Count > 0)
            {
                m_CurrentCommandQueue.Dequeue();
            }
            
            m_CurrentCommandQueue.Enqueue(eCommand);
       
            EAFSMCommand command = myState[eCommand];
            m_nPrevState = m_nCurrState;
            m_nCurrState = eCommand;
            Init(m_nCurrState);

            //  [11/22/2017 puos] prevent loop turning
            if (m_CurrentCommandQueue.Count == 1)
            {
                return false;
            }  
            else
            {
                return true;
            } 
        }

        return false;
    }

    public void AddState(uint nState, EAFSMCommand command)
    {
        myState.Add(nState , command);
    }

    // 10/17/2014 Control the command of a specific state
    public EAFSMCommand GetStateCommand(uint nState)
    {
        if (myState.ContainsKey(nState) == true)
        {
            return myState[nState];
        }

        return null;
    }

    public int StateCount()
    {
        return myState.Count;
    }

    /// <summary>
    /// Get the current state
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetCurrentState<T>()
    {
        Type typeParameterType = typeof(T);

        T current_state = (T)Enum.ToObject(typeParameterType, m_nCurrState);

        return current_state;
    }

    /// <summary>
    /// Get the current fsm run state
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public EAFSMCommand.eFSMState GetCurrentFSMRunState()
    {
        EAFSMCommand.eFSMState eState = EAFSMCommand.eFSMState.eInit;

        EAFSMCommand command = GetStateCommand(m_nCurrState);

        if(command != null)
        {
            eState =  command.m_efsmState;
        }

        return eState;
    }

    /// <summary>
    ///  Get the previous state
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetPrevState<T>()
    {
        Type typeParameterType = typeof(T);

        T prev_state = (T)Enum.ToObject(typeParameterType, m_nPrevState);

        return prev_state;
    }

    /// <summary>
    ///  add fsm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nState"></param>
    /// <param name="eCommand1"></param>
    /// <param name="eCommand2"></param>
    /// <param name="eCommand3"></param>
    /// <param name="eCommand4"></param>
    /// <param name="funcCommand"></param>
    private void AddFSM<T>(T nState,EACommanMethod funcCommand, T eCommand1, T eCommand2, T eCommand3, T eCommand4)
    {
        Type typeParameterType = typeof(T);

        if (typeParameterType.IsEnum == false)
        {
            Debug.Log("invalid type - type is not null");
            return;
        }
        uint idx_1 = System.Convert.ToUInt32(eCommand1);
        uint idx_2 = System.Convert.ToUInt32(eCommand2);
        uint idx_3 = System.Convert.ToUInt32(eCommand3);
        uint idx_4 = System.Convert.ToUInt32(eCommand4);

        EAFSMCommand command = new EAFSMCommand(funcCommand, idx_1, idx_2, idx_3, idx_4);

        uint idx = System.Convert.ToUInt32(nState);

        AddState(idx, command);
    }

    /// <summary>
    ///  add fsm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nState"></param>
    /// <param name="command"></param>
    public void AddFSMState<T>(T nState, EACommanMethod funcCommand)
    {

        T eCommand1 = (T)Enum.ToObject(typeof(T), 0);
        T eCommand2 = (T)Enum.ToObject(typeof(T), 0);
        T eCommand3 = (T)Enum.ToObject(typeof(T), 0);
        T eCommand4 = (T)Enum.ToObject(typeof(T), 0);

        AddFSM<T>(nState, funcCommand, eCommand1, eCommand2, eCommand3, eCommand4);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nState"></param>
    /// <param name="funcCommand"></param>
    /// <param name="eCommand1"></param>
    public void AddFSMState<T>(T nState, EACommanMethod funcCommand, T eCommand1)
    {
        T eCommand2 = (T)Enum.ToObject(typeof(T), 0);
        T eCommand3 = (T)Enum.ToObject(typeof(T), 0);
        T eCommand4 = (T)Enum.ToObject(typeof(T), 0);

        AddFSM<T>(nState, funcCommand, eCommand1, eCommand2, eCommand3, eCommand4);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nState"></param>
    /// <param name="funcCommand"></param>
    /// <param name="eCommand1"></param>
    /// <param name="eCommand2"></param>
    public void AddFSMState<T>(T nState, EACommanMethod funcCommand, T eCommand1 , T eCommand2)
    {
        T eCommand3 = (T)Enum.ToObject(typeof(T), 0);
        T eCommand4 = (T)Enum.ToObject(typeof(T), 0);

        AddFSM<T>(nState, funcCommand, eCommand1, eCommand2, eCommand3, eCommand4);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nState"></param>
    /// <param name="funcCommand"></param>
    /// <param name="eCommand1"></param>
    /// <param name="eCommand2"></param>
    /// <param name="eCommand3"></param>
    public void AddFSMState<T>(T nState, EACommanMethod funcCommand, T eCommand1, T eCommand2, T eCommand3)
    {
        T eCommand4 = (T)Enum.ToObject(typeof(T), 0);

        AddFSM<T>(nState, funcCommand, eCommand1, eCommand2, eCommand3, eCommand4);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nState"></param>
    /// <param name="funcCommand"></param>
    /// <param name="eCommand1"></param>
    /// <param name="eCommand2"></param>
    /// <param name="eCommand3"></param>
    /// <param name="eCommand4"></param>
    public void AddFSMState<T>(T nState, EACommanMethod funcCommand, T eCommand1, T eCommand2, T eCommand3, T eCommand4)
    {
       AddFSM<T>(nState, funcCommand, eCommand1, eCommand2, eCommand3, eCommand4);
    }

    /// <summary>
    ///  Transition FSM
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="nState"></param>
    public void TransitionFSMCommand<T>(T nState)
    {
        Type typeParameterType = typeof(T);

        if (typeParameterType.IsEnum == false)
        {
            Debug.Log("invalid type - type is not null");
            return;
        }

        T current_state = (T)Enum.ToObject(typeParameterType, nState);
        uint idx = System.Convert.ToUInt32(current_state);
        
        TransitionCommand(idx);
    }

    /// <summary>
    /// Basic fsm structure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="state"></param>
    /// <param name="action"></param>
    /// <param name="run_action"></param>
    public void FSMAddState<T>(T state, System.Action  action , System.Action run_action = null)
    {
       AddFSMState<T>(state,
       delegate (EAFSMMaker.EAFSMCommand c)
       {
           if (c.m_efsmState == EAFSMMaker.EAFSMCommand.eFSMState.eInit)
           {
               Debug.Log("<color=#FFFF00> EAFSM : " + GetCurrentState<T>() + "</color>");

               if (action != null)
               {
                   action();
               }
           }

           if(run_action != null)
           {
               run_action();
           } 

           return EAFSMMaker.EAFSMCommand.current;
       });
    }

}
