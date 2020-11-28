using System;
using UnityEngine;
using Debug = EAFrameWork.Debug;


public abstract class EASceneLogic : MonoBehaviour ,
                                    IEAEventTarget
{
	static public EASceneLogic instance;
    	
    protected enum SceneLoadingState
	{
		None,
		Inited,
		PostInited,
		WillDestroy,
	}

    protected SceneLoadingState _sceneLoadingState { get; private set; }

    public string prevSceneName { get; private set; }

    public string sceneName { get; private set; }

	public Action onSceneDestroy = null;

	bool _selfLoading = false;
	    
    abstract protected void OnInit();

    abstract protected void OnPostInit();

    abstract protected void OnUpdate();

    abstract protected void OnClose();

   	protected virtual void OnLazyUpdate(LazyUpdateType lazyType) { }
  
    protected virtual void OnEventProcess() { }

    void Awake()
    {
        if (instance != null && !object.ReferenceEquals(this, instance))
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;

        prevSceneName = SceneLoadingManager.instance.GetPrevSceneName();
    }

    void Start()
    {
    }

    void OnDestroy()
    {
        if (onSceneDestroy != null)
        {
            onSceneDestroy();
        }

        Debug.Log("EASceneLogic.OnDestroy()");

        Debug.Assert(instance != null);
        instance = null;
    }

    public void Init()
	{
		_sceneLoadingState = SceneLoadingState.Inited;

		sceneName = SceneLoadingManager.instance.GetLoadedSceneName();

        if (EAMainFrame.instance != null)
        {
            EAMainFrame.instance.OnSceneLoaded();
        }

        Debug.Log("EASceneLogic.Init() frameCount:" + Time.frameCount);

        OnInit();
	}

	public void DoPostInit()
	{
		if (_sceneLoadingState == SceneLoadingState.PostInited)
			return;

        Debug.Log("EASceneLogic.DoPostInit() frameCount:" + Time.frameCount);

        OnPostInit();

		_sceneLoadingState = SceneLoadingState.PostInited;
	}
    		
	void OnEnable()
	{
        SetGroupName(EA_EventSystem.eEA_EventGroup.eSceneGroup.ToString());
        SetTargetName(SceneLoadingManager.instance.GetLoadedSceneName());
        EA_EventSystem.instance.AddEventTarget(this);
    }

    void OnDisable()
	{
        EA_EventSystem.instance.RemoveEventTarget(this);
    }

	public void Destroy()
	{
        Debug.Log("EASceneLogic.Destroy() frameCount:" + Time.frameCount);

        if (!EAMainFrame.isApplicationQuiting)
			EA_FrameWorkEvents.onSceneWillChange();

        OnClose();

        ResourceManager.instance.Clear();

        if (gameObject != null)
        {
            gameObject.transform.SetParent(null);
            GameObject.Destroy(gameObject);
        }
    }
    
    protected void Update()
	{
        //None -> Inited -> PostInited
        if (_sceneLoadingState == SceneLoadingState.None)
        {
            // If there is no previous scene, it is judged as a single scene call
            _selfLoading = SceneLoadingManager.IsSelfLoading();

            if(_selfLoading)
            {
                Init();
            }
        }
        else if (_sceneLoadingState == SceneLoadingState.Inited)
        {
            if (_selfLoading)
            {
                DoPostInit();
            }
        }
        else if (_sceneLoadingState == SceneLoadingState.PostInited)
        {
            OnUpdate();
        }
    }

    // run in mainframe
    public void SceneLogicOnLazyUpdate(LazyUpdateType lazyType)
    {
        if (_sceneLoadingState != SceneLoadingState.PostInited)
            return;

        OnLazyUpdate(lazyType);
    }
    
    #region EVENT_SYSTEM
    string m_sGroupName = string.Empty;
    string m_sTargetName = string.Empty;

    public void EventProcess()
    {
        OnEventProcess();
    }

    public string GetGroupName()
    {
        return m_sGroupName;
    }

    public string GetTargetName()
    {
        return m_sTargetName;
    }

    public void SetGroupName(string sGroupName)
    {
        m_sGroupName = sGroupName;
    }

    public void SetTargetName(string sTargetName)
    {
        m_sTargetName = sTargetName;
    }
    #endregion  

    protected virtual bool OnEscapeKey()
	{
		return false;
	}

	public bool HandleEscapeKey()
	{
		if (_sceneLoadingState != SceneLoadingState.PostInited)
			return true;

		return OnEscapeKey();
	}
    		
	public void WillDestroy()
	{
		_sceneLoadingState = SceneLoadingState.WillDestroy;
	}
}

