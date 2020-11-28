using UnityEngine;
using System;


/// <summary>
/// Manage server time.
/// </summary>
public class ClockManager : Singleton<ClockManager>
{
    long _savedServerTime = 0;
    long _tickAtServerTimeSaved;
    long _currServerTime;

#if UNITY_EDITOR
    public long _debugCurrServerTime;       // For observation in the Inspector.
    public string _debugCurrServerTimeDesc; // For observation in the Inspector.
#endif

    public override GameObject GetSingletonParent()
    {
        return EAMainFrame.instance.gameObject;
    }

    protected override void Awake()
    {
        base.Awake();

        EAMainFrame.instance.OnMainFrameFacilityCreated(MainFrameAddFlags.ClockMgr);
    }
    
    public long serverTime
    {
        get { return _currServerTime; }
    }

    public DateTime serverTimeAsLocalDateTime
    {
        get { return TimeUtil.TimestampToLocalDateTime(_currServerTime); }
    }
    
    public void SetServerTime(long utc)
    {
        _savedServerTime = utc;
        _tickAtServerTimeSaved = System.Environment.TickCount & Int32.MaxValue;
    }
   
    public void UpdateClock()
    {
        if(_savedServerTime != 0)
        {
            // Current server time = stored server time + elapsed time
            long currTick = System.Environment.TickCount & Int32.MaxValue;
            
            if (currTick < _tickAtServerTimeSaved) // Tick ​​starts from 0 again beyond 32 bits 
            {
                _savedServerTime += (currTick + Int32.MaxValue) - _tickAtServerTimeSaved;
                _tickAtServerTimeSaved = currTick;
            }

            long timeSinceSaved = currTick - _tickAtServerTimeSaved;

            _currServerTime = _savedServerTime + timeSinceSaved;
        }   
        else
        {
#if UNITY_EDITOR
            // Offline available
            _currServerTime = TimeUtil.GetDeviceTimeForOffline();
#endif
        }

#if UNITY_EDITOR
        _debugCurrServerTime     = _currServerTime;
        _debugCurrServerTimeDesc = serverTimeAsLocalDateTime.ToLongTimeString();
#endif

    }
}
