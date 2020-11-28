using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Debug = EAFrameWork.Debug;

public abstract class EAScene : MonoBehaviour
{
    public string controllerClass;

    public static EAScene instance { get; private set; }

	bool _createSceneLogic = false;

    protected static Dictionary<string, System.Type> _sceneInfo = new Dictionary<string, System.Type>();

    protected virtual void Awake()
	{
        if (instance != null && !object.ReferenceEquals(this, instance))
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;

	   	Init();

        if(SceneLoadingManager.IsSelfLoading())
        {
            OnSetting();
        }
    }

    void Init()
	{
        EAMainFrame mainframe = FindObjectOfType<EAMainFrame>();

        // When the first scene runs, it creates a MainFrame.
        if (mainframe == null)
		{
			EAMainframeUtil.CreateMainFrameTree();
			Debug.Log("EA SceneConfig.Init - Call Init() frameCount:" + Time.frameCount);
		}
	}

    protected virtual void OnSetting()
    {

    } 

    public void InitializeSceneInfo()
    {
        var types = from a in AppDomain.CurrentDomain.GetAssemblies()
                    from t in a.GetTypes()
                    where t.IsDefined(typeof(EASceneInfoAttribute), false)
                    select t;

        foreach (var type in types)
        {
            var attrs = type.GetCustomAttributes(typeof(EASceneInfoAttribute), false);
            if (attrs == null) continue;
            var attr = attrs[0] as EASceneInfoAttribute;
            _sceneInfo.Add(attr._className, attr._classType);
        }
    }

    void CreateSceneLogic()
	{
        EASceneLogic sm = null;

        Type t = null;

        _sceneInfo.TryGetValue(controllerClass, out t);
              
        sm = (EASceneLogic)EAFrameUtil.AddChild(EAMainFrame.instance.gameObject, t , "gameLogic") as EASceneLogic;

        Debug.Log("EA SceneConfig.CreateSceneLogic - call CreateSceneLogic sm is " + ((sm == null) ? "null" : "valid") + " controller class :" + controllerClass + " frameCount:" + Time.frameCount);
    }

    void Update()
    {
        if(_createSceneLogic == false && EAMainFrame.instance.IsMainFramePosInitReady())
        {
            CreateSceneLogic();
            _createSceneLogic = true;
        } 
    }
}
