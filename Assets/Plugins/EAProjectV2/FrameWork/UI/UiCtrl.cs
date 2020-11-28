using UnityEngine;
using System;
using Debug = EAFrameWork.Debug;


public abstract class UiCtrl : UiCtrlBase, ISetResultCb, IEAEventTarget
{
    [SerializeField]
    public int sortingOrder = 0;

    protected UiId _uiId = UiId.CreateInvalid();

    public UiId uiId
    {
        get { return _uiId; }
        set { _uiId = value; }
    }

    public virtual UiResult uiResult
    {
        get
        {
            if (_uiResultInternal == null)
                _uiResultInternal = new UiResult();
            return _uiResultInternal;
        }
    }

    private int _refreshFlags = 0;

    public UiParam _uiParamBase { get; private set; }

    public UiState _uiState { get; private set; }

    protected UiResult _uiResultInternal = null;

    //float _initializedTime;
    bool _isCreated = false;
   
    public Action<UiCtrl, bool> _onUiEnabled = delegate (UiCtrl ui, bool state) { EA_FrameWorkEvents.onUiEnabled(ui, state); };
    public Action<UiCtrl, UiState> _onUiStateChanged = delegate (UiCtrl ui, UiState newState) { EA_FrameWorkEvents.onUiStateChanged(ui, newState); };

    protected virtual void OnCreate()
    {
    }

    protected abstract void OnInit();
    
    protected abstract void OnClose();

    protected abstract void OnUpdate();

    protected abstract void OnEnableUI();

    protected abstract void OnDisableUI();

    protected virtual void OnRefresh(int flags)
    {
    }

    protected virtual bool OnEscapeKey()
    {
        return false;
    }

    public override bool HandleEscapeKey()
    {
        return OnEscapeKey();
    }

    public virtual void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    protected virtual void _RelayUiParam() { }

    public virtual Type GetResultType()
    {
        return typeof(UiResult);
    }

    protected virtual void Awake()
    {
        if (!_isCreated)
            _Create();
    }

    void _Create()
    {
        if (_isCreated)
        {
            return;
        }

        OnCreate();

        _isCreated = true;
    }

    protected virtual void OnDestroy()
    {
        if (_uiState != UiState.WillDestroy) // Destroy itself if not closed directly
        {
            _OnWillDestroy();
        }
    }

    /*protected virtual */
    private void OnEnable()
    {
        if (_uiState > UiState.None && _uiState < UiState.WillDestroy)
        {
            OnEnableUI();

            _onUiEnabled(this, true);

            if (!_uiId.IsInvalid())
            {
                Debug.Log("<color=yellow>Ui Activate :" + _uiId + "</color>");
            }
        }
    }
    /*protected virtual */
    private void OnDisable()
    {
        if (_uiState >= UiState.Inited && _uiState < UiState.WillDestroy)
        {
            OnDisableUI();

            _onUiEnabled(this, false);
            if (!_uiId.IsInvalid())
            {
                Debug.Log("<color=lightblue>Ui deActivate :" + _uiId + "</color>");
            }
        }
    }

    sealed public override void _Init()
    {
        Debug.Assert(_uiState < UiState.Inited, @"UiCtrl(" + name + @") has already initialized.");

        if (!_isCreated)
            _Create();

        _ChangeUiState(UiState.Inited);

        _RelayUiParam();

        if (_uiParamBase != null && _uiParamBase.onLoadCompleted != null)
        {
            _uiParamBase.onLoadCompleted(this);
        }

        OnInit();
             
        // Refresh () is executed when refreshFlag is specified in OnInit ().
        TryRefresh();

        if (!_uiId.IsInvalid())
        {
            Debug.Log("<color=lightblue>Ui Activate :" + _uiId + "</color>");
        }
    }


    private void Update()
    {
        if (_uiState == UiState.WillDestroy)
            return;

        TryRefresh();

        OnUpdate();
    }

    void LateUpdate()
    {
        TryRefresh();
    }

    public override void _Activate()
    {
        _ChangeUiState(UiState.Enable);
    }

    public override void _Deactivate()
    {
        if (this == null)
        {
            Debug.Assert(false);
            return;
        }

        if (gameObject == null)
        {
            Debug.Assert(false);
            return;
        }

        SetActive(false);
    }

    public override void _OnWillDestroy()
    {
        bool notInited = (_uiState == UiState.None);

        _ChangeUiState(UiState.WillDestroy);

        {
            // Call OnClose, but skip if it doesn't go through initialization.
            if (!notInited)
                OnClose();
        }
    }

    public virtual void Close(UiResult result = null)
    {
        if (result != null)
            _uiResultInternal = result;

        UIManager.instance.CloseUi(this, _uiResultInternal);

        _ChangeUiState(UiState.None);
    }


    public virtual void OnCancelClick()
    {
        NoResultClose();
        UIManager.instance.StackActiveUi(this.uiId);
    }

    public virtual void OnClickEscapeKey()
    {
        OnCancelClick();
    }

    public void NoResultClose()
    {
        Delegate cb = _resultCbD;

        _resultCbD = null;

        Close();

        _resultCbD = cb;
    }

    public void SetResultCb<T>(UiCallbackType<T> cb)
    {
        _resultCbD = cb;

        if (_resultCbD == null)
            return;

        Debug.Assert(GetResultType() == typeof(T), @"SetResultCb() - Callback types do not match. '" + this.GetType().ToString() + @"' The return type of type '" + GetResultType());
    }

    public void SetNeedRefresh(int flags = Flags32.All)
    {
        _refreshFlags |= flags;
    }

    /// <summary>
    /// Force OnRefresh to be called. refreshFlag is reset.
    /// </summary>
    public void Refresh(int flags = Flags32.All)
    {
        if (_uiState < UiState.WillDestroy)
        {
            //Debug.Log("OnRefresh : " + name);
            OnRefresh(flags);
            _refreshFlags &= ~flags;
        }
    }

    /// <summary>
    /// Refresh the UI if there is a refreshFlag.
    /// </summary>
    public void TryRefresh()
    {
        if (_refreshFlags != 0)
        {
            //Debug.Log("OnRefresh : " + name);
            OnRefresh(_refreshFlags);
        }
        _refreshFlags = 0;
    }

    public void SetUiParam(UiParam uiParam)
    {
        _uiParamBase = uiParam;
        _RelayUiParam();
    }

    protected void _ChangeUiState(UiState state)
    {
        Debug.Assert(_uiState != state);
        _uiState = state;

        _onUiStateChanged(this, state);
    }

    protected void SendCloseClick(string btnName)
    {
        uiResult.btId = btnName;

        Close();

        Debug.Log("SendCloseClick :" + btnName);
    }

    protected UiResult CreateDefaultResult()
    {
        if (GetResultType() == typeof(UiResult))
            return new UiResult();
        else
            return System.Activator.CreateInstance(GetResultType()) as UiResult;
    }

    /// <summary>
    /// Utility for testing RefreshFlags Bit
    /// </summary>
    /// <typeparam name="T">enum type for int or bit flags</typeparam>
    public static bool MatchFlags<T>(int refreshFlags, T userFlags)
    {
        return (refreshFlags & userFlags.GetHashCode()) != 0;
    }

    #region EVENT_SYSTEM

    protected virtual void OnEventProcess()
    {

    }

    string m_sGroupName = "";
    string m_sTargetName = "";

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
}

public interface ISetResultCb
{
    void SetResultCb<T>(UiCallbackType<T> cb);
}