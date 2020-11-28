using UnityEngine;
using System;

public abstract class UiCtrlBase : MonoBehaviour
{
    
    [NonSerialized]
    public Delegate _resultCbD;

    
    public abstract void _Init();
    public abstract void _Activate();
    public abstract void _Deactivate();
    public abstract void _OnWillDestroy();

    public abstract bool HandleEscapeKey();
}
