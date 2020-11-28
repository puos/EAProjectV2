using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = EAFrameWork.Debug;


public class SceneLoadingManager : Singleton<SceneLoadingManager>
{
    static string _prevSceneId = null;
	string _sceneNameToLoad = null;
    string _currSceneId = null;

    enum State
	{
		None,
		CloseCurrScene,
		WaitOldSceneClose,
        WaitOldSceneCloseWait,
        WaitUnloadUnusedAssets,
		StartAsync,
		OnSyncLoading,
        OnSyncLoadingWait,
		Finalize,
	}
	State _state = State.None;

	AsyncOperation _async = null;

    AsyncOperation _unloadAsync = null;

    bool isPassFadeInEffect = false;

    private float fadeOutValue = 0;
    private float fadeInValue  = 0;
    private float fadeInDelayValue = 0;

    public override GameObject GetSingletonParent()
    {
        return EAMainFrame.instance.gameObject;
    }

    protected override void Awake()
    {
        base.Awake();

        EAMainFrame.instance.OnMainFrameFacilityCreated(MainFrameAddFlags.SceneLoadingMgr);
    }

    public void Application_LoadLevel(string name)
    {
        SceneManager.LoadScene(name);
    }

    public static AsyncOperation Application_LoadLevelAsync(string name)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(name);
        return async;
    }

    public static void FinalizeLevelAsync(string name)
    {        
    }


	void Update ()
	{
        // CloseCurrScene -> WaitOldSceneCloseWait -> WaitOldSceneClose -> WaitUnloadUnusedAssets -> StartAsync -> OnSyncLoading -> OnSyncLoadingWait
        // ->  Finalize

        switch (_state)
		{
			case State.CloseCurrScene:
                {
                    Debug.Log("SceneLoadingManager -  CloseCurrScene");

                    _state = State.WaitOldSceneCloseWait;
                }
                break;

            case State.WaitOldSceneCloseWait:
                {
                    if (EASceneLogic.instance != null)
                    {
                        EASceneLogic.instance.Destroy();

                        _state = State.WaitOldSceneClose;
                    }
                }
                break;

           case State.WaitOldSceneClose:
                {
                    Debug.Log("SceneLoadingManager - WaitOldSceneClose");

                    // Wait for the previous scene to end.
                    _unloadAsync = Resources.UnloadUnusedAssets();
                    _state = State.WaitUnloadUnusedAssets;
                }
				break;

            case State.WaitUnloadUnusedAssets:
                {
                    Debug.Log("SceneLoadingManager - WaitUnloadUnusedAssets");

                    if (_unloadAsync == null || _unloadAsync.isDone)
                    {
                        _unloadAsync = null;

                        System.GC.Collect();

                        _state = State.StartAsync;
                    }
                }
                break;

			case State.StartAsync:
                {
                    Debug.Log("SceneLoadingManager - StartAsync ");

                    _async = Application_LoadLevelAsync(_sceneNameToLoad);

                    if (_async == null)
                    {
                        Debug.LogError("Cannot load scene: " + _sceneNameToLoad);
                        break;
                    }

                    _state = State.OnSyncLoading;
                }
                break;

			case State.OnSyncLoading:
                {
                    Debug.Log("SceneLoadingManager - OnSyncLoading");

                    if (_async.isDone && EASceneLogic.instance != null)
                    {
                        _state = State.OnSyncLoadingWait;

                        EASceneLogic.instance.Init();
                                                                       
                        if (isPassFadeInEffect == false)
                        {
                            _state = State.Finalize;
                        } 
                        else
                        {
                            _state = State.Finalize;
                        }
                    } 
                }
                break;

            case State.OnSyncLoadingWait:
                break;

            case State.Finalize:
                {
                    FinalizeLevelAsync(_sceneNameToLoad);

                    Debug.Log("SceneLoadingManager - Finalize");

                    if(EASceneLogic.instance != null)
                    {
                        EASceneLogic.instance.DoPostInit();

                        _async = null;
                        _state = State.None;

                        _currSceneId = _sceneNameToLoad;
                    }
                }
				break;
		}
	}

    public void LoadScene(string sceneName, bool isPassFadeOutEffect = false, bool isPassFadeInEffect = false , float fadeOutValue = 0.5f , float fadeInValue = 0.5f , float fadeInDelayValue = 0.5f)
	{
		if (_state != State.None)
		{
		    Debug.Assert(false, "warning loading scene");
			return;
		}

        this.isPassFadeInEffect = isPassFadeInEffect;

        this.fadeOutValue     = fadeOutValue;
        this.fadeInValue      = fadeInValue;
        this.fadeInDelayValue = fadeInDelayValue;

        if (isPassFadeOutEffect == false)
        {
            // Do screen fade effect
            StartLoad(sceneName);
        }
        else
        {
            StartLoad(sceneName);
        }
    }

    private void StartLoad(string sceneName)
    {
        // When testing from a particular scene in the editor, calling GetLoadedSceneName () is correct because _sceneNameToLoad does not carry the name of the first scene when moving the scene.
        _prevSceneId = GetLoadedSceneName();
        _sceneNameToLoad = sceneName;

        _state = State.CloseCurrScene;

        Debug.Log("SceneManager.LoadScene() [" + GetPrevSceneName() + "] -> [" + sceneName + "]");
    }


    public bool IsLoading()
	{
		return _state == State.None ? false : true;
	}

    /// <summary>
    /// Check for self loading
    /// </summary>
    /// <returns></returns>
    public static bool IsSelfLoading()
    {
        return string.IsNullOrEmpty(_prevSceneId);
    } 

    public string GetCurrSceneId()
	{
        return _currSceneId;
    }

	public string GetPrevSceneId()
	{
		return _prevSceneId;
	}

	public string GetPrevSceneName()
	{
		return _prevSceneId;
	}

	public string GetLoadedSceneName()
	{
        // Currently loaded sceneName
        return SceneManager.GetActiveScene().name;
    }
}
