using System.Collections.Generic;
using UnityEngine;
using System;
using Debug = EAFrameWork.Debug;

public class UIManager : Singleton<UIManager>
{
    public enum UISpawntype
    {
        EUIPage,
        EUIAbove,
        EUIPopup,
    }

    public static string UI_ROOT_NAME = "UIRoot";
    public static string UI_ROOT_PAGE = "Page";
    public static string UI_ROOT_ABOVE = "Above";
    public static string UI_ROOT_POPUP = "Popup";
   
    UiCtrl _currUi = null;

    List<UiCtrl> uiCtrlList = new List<UiCtrl>();
    List<UiCtrl> uiPopupList = new List<UiCtrl>();

    private Transform m_tUIRoot    = null;
    private Transform m_tRootPage  = null;
    private Transform m_tRootAbove = null;
    private Transform m_tRootPopup = null;

    public bool enableEscapeKey = true;

    public class StackSection
    {
        public UiId uiId = UiId.CreateInvalid();
        public UiParam param;
        public UISpawntype spawnType;
    }

    List<StackSection> _prevSections = new List<StackSection>();

    public override GameObject GetSingletonParent()
    {
        return EAMainFrame.instance.gameObject;
    }

    protected override void Awake()
    {
        base.Awake();

        uiCtrlList.Clear();
        uiPopupList.Clear();

        _currUi = null;

        EAMainFrame.instance.OnMainFrameFacilityCreated(MainFrameAddFlags.UiManager);
    }

    public void Init()
    {
        uiCtrlList.Clear();
        uiPopupList.Clear();

        GameObject goRoot = GameObject.Find(UI_ROOT_NAME);

        if(goRoot == null)
        {
            goRoot = new GameObject(UI_ROOT_NAME);

            GameObject page   = new GameObject(UI_ROOT_PAGE);
            GameObject above  = new GameObject(UI_ROOT_ABOVE);
            GameObject popup = new GameObject(UI_ROOT_POPUP);

            page.transform.SetParent(goRoot.transform);
            above.transform.SetParent(goRoot.transform);
            popup.transform.SetParent(goRoot.transform);
        }

        if (null != goRoot)
        {
            m_tUIRoot    = goRoot.transform;
            m_tRootPage  = m_tUIRoot.Find(UI_ROOT_PAGE);
            m_tRootAbove = m_tUIRoot.Find(UI_ROOT_ABOVE);
            m_tRootPopup = m_tUIRoot.Find(UI_ROOT_POPUP);
        }

        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        rootObjects.ForEach(s =>
        {
            UiCtrl[] uis = s.GetComponentsInChildren<UiCtrl>(true);

            for (int i = 0; i < uis.Length; ++i)
            {
                UiCtrl v = uis[i];
                string value = v.GetType().ToString();
                v.uiId.Set(value);
                v.SetActive(false);
                Canvas c = v.GetComponent<Canvas>();

                bool isWorldUi = false;

                if (c != null)
                {
                    if(v.sortingOrder > 0)
                        c.sortingOrder = v.sortingOrder;
                }

                if(isWorldUi == false)
                {
                    RectTransform rt = v.GetComponent<RectTransform>();

                    rt.SetParent(m_tRootPage);
                    rt.Reset();
                }      
   
                uiCtrlList.Add(v);
            }
        });
    }

    public bool HandleEscapeKey()
    {
        if (uiPopupList.Count > 0)
        {
            UiCtrlBase ui = uiPopupList[uiPopupList.Count - 1];

            if (ui != null && ui.isActiveAndEnabled)
            {
                string uiName = ui.name;
                bool handled = ui.HandleEscapeKey();

                if (handled)
                {
                    Debug.Log("EscapeKey has handled by PopupUi[" + uiName + "].");
                    return handled;
                }
            }
        }

        if (_currUi != null)
        {
            _currUi.OnClickEscapeKey();
            return false;
        }

        return true;
    }

    public UiCtrl ActiveUi(UiId uiId, UiParam param = null,
                           UISpawntype spawnType = UISpawntype.EUIPage ,
                           bool push = true, bool clearstack = false)
    {
        UiCtrl ui = GetUi(uiCtrlList, uiId);

        if(ui == null)
        {
            var prefab = ResourceManager.instance.CreateUI(spawnType , uiId.ToString());
            
            if(prefab == null)
            {
                Debug.Assert(prefab != null, " ui resource is null spawnTyp : " + spawnType + " uiId :" + uiId.ToString());
                return null;
            }
            
            ui = prefab.GetComponent<UiCtrl>();
            string value = ui.GetType().ToString();
            ui.uiId.Set(value);
            ui.SetActive(false);
        }

        if (ui != null && ui.gameObject != null)
        {
            RectTransform rt = ui.GetComponent<RectTransform>();

            Transform parent = (spawnType == UISpawntype.EUIPage) ? m_tRootPage : (spawnType == UISpawntype.EUIAbove) ? m_tRootAbove : m_tRootPopup;

            rt.SetParent(parent);
            rt.Reset();

            if (param == null)
                param = new UiParam();

            if (ui._uiParamBase != null)
                ui._uiParamBase.Clear();

            if (ui.uiResult != null)
                ui.uiResult.Clear();

            ui.SetUiParam(param);

            ui._Activate();

            ui.SetActive(true);

            ui._Init();

            if (clearstack)
            {
                _prevSections.Clear();
            }

            if (push)
            {
                UiCtrl uisection = ui.GetComponent<UiCtrl>() as UiCtrl;

                if (uisection != null)
                {
                    StackSection s = new StackSection();
                    s.uiId.Set(uisection.uiId);
                    s.param = param;
                    s.spawnType = spawnType;
                    _prevSections.Insert(0, s);
                }
            }

            bool bPopup = (UISpawntype.EUIPopup == spawnType);

            if (!bPopup)
            {
                _currUi = ui;
                uiCtrlList.Add(ui);
            } 
                        
            if(bPopup)
                uiPopupList.Add(ui);
        }

        return ui;
    }

   // get ui
    public UiCtrl GetUi(List<UiCtrl> list, UiId uiId)
    {
        UiCtrl result = null;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].uiId == uiId)
            {
                result = list[i];
                break;
            }
        }

        return result;
    }

    public void StackActiveUi(UiId uiId)
    {
        _prevSections.RemoveAll(s => s.uiId == uiId);

        if (_prevSections.Count <= 0)
        {
            return;
        }

        StackSection uisection = _prevSections[0];

        int count = 0;

        _prevSections.ForEach(x =>
        {
            Debug.Log("stack" + (count++) + " next ui : " + x.uiId);
        });

        if (uisection != null)
        {
            uisection.param.ResetParam();
            ActiveUi(uisection.uiId, uisection.param, uisection.spawnType , false);
        }
    }

    public UiId GetPrevUi(UiId _uiId)
    {
        UiId _prevUiId = UiId.CreateInvalid();

        for (int i = 0; i < _prevSections.Count; ++i)
        {
            if (_prevSections[i].uiId == _uiId)
                continue;

            _prevUiId.Set( _prevSections[i].uiId);
            break;
        }

        return _prevUiId;
    }

    public void CloseUi(UiId uiId)
    {
        UiCtrl ui = GetUi(uiCtrlList, uiId);

        if (ui != null)
        {
            ui.Close();
        }
    } 

    public void CloseUi(UiCtrlBase uiCtrlBase, UiResult result = null)
    {
        UiCtrl uiCtrl = (UiCtrl)uiCtrlBase as UiCtrl;

        if (uiCtrl == null)
            return;

        if (uiCtrl._uiState == UiState.WillDestroy)
            return;

        if (_currUi == uiCtrl)
            _currUi = null;

        UiCtrl popup_ui = GetUi(uiPopupList, uiCtrl.uiId);

        if (popup_ui != null)
        {
            uiPopupList.Remove(popup_ui);
        }

        uiCtrl._Deactivate();
        uiCtrl._OnWillDestroy();

        Delegate resultCbD = uiCtrl._resultCbD;

        if (uiCtrl._resultCbD != null)
        {
            if (result == null)
            {
                result = uiCtrl.uiResult;
                result.btId = ButtonId.Cancel;
            }
            else
            {
                if (result.GetType() != uiCtrl.GetResultType())
                {
                    Debug.Assert(false, @"ResultType must be '" + uiCtrl.GetResultType() + @"' not '" + result.GetType() + @"'.");
                }
            }
            result.uiCtrl = uiCtrl;
        }

        if (uiCtrl._resultCbD != null)
        {
            if (result == null)
            {
                result = uiCtrl.uiResult;
                result.btId = ButtonId.Cancel;
            }
            else
            {
                if (result.GetType() != uiCtrl.GetResultType())
                {
                    Debug.Assert(false, @"ResultType must be '" + uiCtrl.GetResultType() + @"' not '" + result.GetType() + @"'.");
                }
            }
            result.uiCtrl = uiCtrl;
        }

        if (resultCbD != null)
        {
            resultCbD.DynamicInvoke(new object[] { result });
        }
    }

    public bool IsUseUICtrl()
    {
        return (_currUi != null) ? true : false;
    }

    public UiId GetCurrUiId()
    {
        return _currUi == null ? UiId.CreateInvalid() : _currUi.uiId;
    }

    public UiCtrl GetCurrUi()
    {
        return _currUi;
    }
}
