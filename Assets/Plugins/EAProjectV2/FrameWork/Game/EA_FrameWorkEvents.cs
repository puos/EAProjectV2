using UnityEngine;
using System.Collections;
using System;
using Debug = EAFrameWork.Debug;

public class EA_FrameWorkEvents
{
	public static bool showLog = true;

	public delegate void OnGameMsg(uint actorId,string msg);
    public static OnGameMsg onGameMsg = delegate (uint actorId, string msg)
    {
        if (showLog) Debug.Log("LogicEvent - OnGameMsg actorId : " + actorId  + " command : " + msg );
    };

    public static string[] GetGameCommands(string szValues)
    {
        string szSeparateExt = ";";
        string[] arValueParser = szValues.Split(szSeparateExt.ToCharArray());
        return arValueParser;
    }

    public delegate void OnSceneWillChange();
    public static OnSceneWillChange onSceneWillChange = delegate () { if (showLog) Debug.Log("LogicEvents - onSceneWillChange"); };
    public delegate void OnReconnected();
    public static OnReconnected onReconnected = delegate () { if (showLog) Debug.Log("LogicEvents - onReconnected"); };

    public delegate void OnLanguageChanged();
    public static OnLanguageChanged onLanguageChanged = delegate () { if (showLog) Debug.Log("LogicEvents - OnLanguageChanged"); };

    // Application released in paused state
    public static Action onApplicationResumed = delegate () { if (showLog) Debug.Log("LogicEvents - onApplicationResumed"); };

    // [Editor only] Call immediately before entering Play mode
    public static Action onEditorAboutToStart = delegate () { };

    // [Editor only] Called when the pause button is released (not called when entering Play mode)
    public static Action onEditorResumed = delegate () { };

    // [Editor only] Error or pause directly
    public static Action onEditorPaused = delegate () { };

    // Lazy' updates to reduce load
    public delegate void OnLazyUpdate(LazyUpdateType type);
    public static OnLazyUpdate onLazyUpdate = delegate (LazyUpdateType type) { };

    public delegate void OnUiEnabled(UiCtrl uiCtrl, bool enabled);
    public static OnUiEnabled onUiEnabled = delegate (UiCtrl uiCtrl, bool enabled) { };

    public delegate void OnUiStateChanged(UiCtrl uiCtrl, UiState newState);
    public static OnUiStateChanged onUiStateChanged = delegate (UiCtrl uiCtrl, UiState newState) { };
}