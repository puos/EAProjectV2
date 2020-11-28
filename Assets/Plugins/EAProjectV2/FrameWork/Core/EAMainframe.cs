using UnityEngine;
using System;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

[Flags]
public enum MainFrameAddFlags
{
	UiManager = 1 << 0,
	ClockMgr = 1 << 1,
	NetManager = 1 << 2,
    SoundManager = 1 << 3,
	AssetBundleLoadModule = 1 << 4,
	EffectScreenController = 1 << 5,
	SceneLoadingMgr = 1 << 6,
	ObjectPoolManager = 1 << 7, 
	SfxPoolManager = 1 << 8,
	OptionManager = 1 << 9,

	// 위에 추가하세요
	_NumOfType,

	AllFlags = UiManager | ClockMgr | SceneLoadingMgr | ObjectPoolManager | AssetBundleLoadModule | SfxPoolManager | SoundManager | OptionManager,
}

public partial class EAMainFrame : Singleton<EAMainFrame>
{
	public static bool isApplicationQuiting = false;
 
	[System.NonSerialized]	public bool _started = false;
	[System.NonSerialized]	public bool _postInitCalled = false;
	  
    public Camera worldCamera
    {
        get
        {
            return _worldCamera;
        }
    }
        
    Camera _worldCamera = null;
          
    MainFrameAddFlags _facilityCreatedFlags;

    float _nextLazyUpdateCheckTime = 0;
	long _lazyUpdateSeq = 0;
    
    protected override void Awake()
	{
        base.Awake();
	}
    
	// Use this for initialization
	protected override void Start()
	{
        base.Start();

         _started = true;

        if (!_postInitCalled)
            ExceptionUtil.Throw("Do not initialize MainFrame yet. see MainFrame.OnMainFrameFacilityCreated()");
	}

	// Update is called once per frame
	void Update()
	{
       HandleInput();

        if (_nextLazyUpdateCheckTime < Time.time)
        {
            OnLazyUpdate(LazyUpdateType.Every25ms);
            _nextLazyUpdateCheckTime += 0.025f; // Starting at least 25ms

            if (_nextLazyUpdateCheckTime < Time.time)
                _nextLazyUpdateCheckTime = Time.time;

            _lazyUpdateSeq++;

            if (_lazyUpdateSeq % 2 == 0)
                OnLazyUpdate(LazyUpdateType.Every50ms);
            if (_lazyUpdateSeq % 4 == 0)
                OnLazyUpdate(LazyUpdateType.Every100ms);
            if (_lazyUpdateSeq % 20 == 0)
                OnLazyUpdate(LazyUpdateType.Every500ms);
            if (_lazyUpdateSeq % 40 == 0)
                OnLazyUpdate(LazyUpdateType.Every1s);
            if (_lazyUpdateSeq % 200 == 0)
                OnLazyUpdate(LazyUpdateType.Every5s);
            if (_lazyUpdateSeq % 400 == 0)
                OnLazyUpdate(LazyUpdateType.Every10s);
            if (_lazyUpdateSeq % 2400 == 0)
                OnLazyUpdate(LazyUpdateType.Every60s);
        }

        ResourceManager.instance.Update();
    }

	public void OnSceneLoaded()
	{
       _worldCamera = CameraUtil.FindMainCamera();

        SoundManager.instance.CreateLowPassFilter();

        Debug.Log("EaMainframe - OnSceneLoaded frameCount : " + Time.frameCount);
    }

    void OnLazyUpdate(LazyUpdateType type)
    {
        if (EASceneLogic.instance != null)
        {
            EASceneLogic.instance.SceneLogicOnLazyUpdate(type);
        }

        EA_FrameWorkEvents.onLazyUpdate(type);

        if (type == LazyUpdateType.Every1s)
        {
            if(ClockManager.instance != null)
            {
                ClockManager.instance.UpdateClock();
            }
        }

        if (type == LazyUpdateType.Every5s)
        {
           // FileLogger.UpdateAllFileLoggers(5f);
        }
    }
    	
	void HandleInput()
	{
		if (Input.GetKeyUp("escape"))
		{
			HandleEscapeKey();
		}
	}

	public void HandleEscapeKey()
	{
		if(EASceneLogic.instance != null)
        {
            EASceneLogic.instance.HandleEscapeKey();
        } 
	}

	public void OnApplicationQuit()
	{
		Debug.Log("MainFrame.OnApplicationQuit()");

        isApplicationQuiting = true;

        if (EASceneLogic.instance != null)
            EASceneLogic.instance.Destroy();

      FileLogger.CloseAllFileLoggers();
	}

    public void QuitApplication()
	{
		Debug.Log("MainFrame.QuitApplication");

		isApplicationQuiting = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
         using( AndroidJavaClass JavaSystemClass = new AndroidJavaClass("java.lang.System"))
        {
             JavaSystemClass.CallStatic("exit", 0);
        }
#else
        Application.Quit();
#endif
    }

    void OnEditorResumed()
    {
        // Since the ApplicationPause signal does not occur in the editor Emulates a signal when the Editor PlayMode changes from 'pause'-> 'play'.

        OnApplicationPause(false);
    }

    void OnApplicationPause(bool paused)
	{
		FileLogger.FlushAllFileLoggers();

        if (!paused)
		{
			EA_FrameWorkEvents.onApplicationResumed();
		}
	}

	void OnDestroy()
	{
		Debug.Assert(instance != null);
	}

	/// <summary>
    /// Called after subframe managers of MainFrame are initialized
    /// </summary>
    public void PostInit()
	{
		if(Application.isPlaying)
			Debug.Assert(IsMainFramePosInitReady());

		_postInitCalled = true;
    }

    public bool IsMainFramePosInitReady()
    {
        return _facilityCreatedFlags == MainFrameAddFlags.AllFlags;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flag"></param>
    public void OnMainFrameFacilityCreated(MainFrameAddFlags flag)
	{
        // In the case of instant test, this is the code to determine when all GameObjects of MainFrame are awakened because MainFrame is already created.
        _facilityCreatedFlags |= flag;

        // When all MainFrame devices are awake, we immediately proceed with PostInit () of MainFrame.
        if (IsMainFramePosInitReady())
			TryPostInit();
	}

	public void TryPostInit()
	{
		if(!_postInitCalled)
			PostInit();
	}

	public static void ApplyDeviceOption(int level ,int frameRateType = 3 , bool keepScreenOn = false)
	{
        List<float> resolution = new List<float>() { 1280 , 1500 , 1920 };

        level = Math.Min(level, resolution.Count - 1);
        
        if(level >= 0)
        {
            float windowX = resolution[level];
            float windowY = (Screen.height / (float)Screen.width) * windowX;
            Screen.SetResolution((int)windowX, (int)windowY, true);
        } 

        ApplyFrameRate(frameRateType);
		
		Screen.sleepTimeout = keepScreenOn ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
		
		if(Application.isMobilePlatform)
			Application.runInBackground = false;
	}

	static void ApplyFrameRate(int frameRateType)
	{
		switch (frameRateType)
		{
			case 0:
				Application.targetFrameRate = 20;
				QualitySettings.vSyncCount = CoreApplication.IsMobile ? 0 : 0;
				break;
			case 1:
				Application.targetFrameRate = 30;
                QualitySettings.vSyncCount = CoreApplication.IsMobile ? 1 : 0;
				break;
			case 2:
				Application.targetFrameRate = 40;
                QualitySettings.vSyncCount = CoreApplication.IsMobile ? 1 : 0;
				break;
			case 3:
				Application.targetFrameRate = 60;
                QualitySettings.vSyncCount = CoreApplication.IsMobile ? 1 : 0;
				break;
		}

		Debug.Log("FrameRate:" + Application.targetFrameRate + " IsMobile : " + CoreApplication.IsMobile);
	}
}
