using UnityEngine;
using Holoville.HOTween;
using System;
using Debug = EAFrameWork.Debug;

/// <summary>
/// Use real number increasing hotweener
/// http://hotween.demigiant.com/
/// </summary>
public class EANumberTween
{
	public const float DEFAULT_SPEED = 5;
	public const float DEFAULT_MAXDURTIME = 1;

	public float _number;
	public struct Event
	{
		public float number;
		public object[] parms;
	}
	public delegate void Callback(Event tweenEvent);
	public Callback onUpdate;
	public Callback onComplete;

	Tweener _tweener;

    EaseType easeType = EaseType.Linear;

    /// <summary>
    /// Specify initial value
    /// </summary>
    public EANumberTween(float initNum , EaseType easeType = EaseType.Linear)
	{
		_number = initNum;
        this.easeType = easeType;
    }

    /// <summary>
    /// Start increasing animation. Recall is possible, at which point it will continue with the existing value.
    /// </summary>
    /// <param name="numTo">Final value</param>
    /// <param name="userData">User data to be passed to the callback</param>
    public void StartTween(float numTo, params object[] userData)
	{
		StartTween(numTo, EANumberTween.DEFAULT_SPEED, EANumberTween.DEFAULT_MAXDURTIME, userData);
	}

    /// <summary>
    /// Start increasing animation. Recall is possible, in which case it will continue with the value that was increasing.
    /// </summary>
    /// <param name="numTo">Final value</param>
    /// <param name="speed">Increments per second</param>
    /// <param name="maxDurTime">Incremental maximum time</param>
    /// <param name="userData">User data to be passed to the callback</param>
    public void StartTween(float numTo, float speed, float maxDurTime, params object[] userData)
	{
		if (_tweener != null)
			_tweener.Kill();

		float unitTime = (speed == 0) ? 0.001f : (1f / speed);
		float dTime = Mathf.Min(maxDurTime, Mathf.Abs(numTo - _number) * unitTime);

		_tweener = HOTween.To(this, dTime, new TweenParms().Prop(@"_number", numTo).Ease(easeType));

		_tweener.ApplyCallback(Holoville.HOTween.CallbackType.OnUpdate, delegate(TweenEvent te)
		{
			_OnTweenUpdate(te);
			
		}, userData);

		_tweener.ApplyCallback(Holoville.HOTween.CallbackType.OnComplete, delegate(TweenEvent te)
		{
			_OnTweenUpdate(te);
			_OnTweenComplete(te);
				
		}, userData);
	}

	void _OnTweenUpdate(TweenEvent te)
	{
		if (onUpdate != null)
		{
			Event e = new Event();
			e.number = _number;
			e.parms = te.parms;
			try
			{
				onUpdate(e);
			}
			catch (MissingReferenceException)
			{
                // When GameObject is destroyed during Tween Update.
                _tweener.Kill();
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
		}
	}
	void _OnTweenComplete(TweenEvent te)
	{
		if (onComplete != null)
		{
			Event e = new Event();
			e.number = _number;
			e.parms = te.parms;
			try
			{
				onComplete(e);
			}
			catch (MissingReferenceException)
			{
                // When GameObject is destroyed during Tween Update.
            }
            catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
		}
	}

    /// <summary>
    ///Simple static version of NumberTween.StartTween (). Convenient to use if you do not need to restart.
    /// </summary>
    /// <param name="numFrom">Initial Value</param>
    /// <param name="numTo">Final value</param>
    /// <param name="userData">User data to be passed to the callback</param>
    /// <returns></returns>
    public static EANumberTween Start(float numFrom, float numTo, params object[] userData)
	{
		return Start(numFrom, numTo, EANumberTween.DEFAULT_SPEED, EANumberTween.DEFAULT_MAXDURTIME, userData);
	}

    /// <summary>
    /// Simple static version of NumberTween.StartTween (). Convenient to use if you do not need to restart.
    /// </summary>
    /// <param name="numFrom">Initial Value</param>
    /// <param name="numTo">Final value</param>
    /// <param name="speed">Increments per second</param>
    /// <param name="maxTime">Incremental maximum time</param>
    /// <param name="userData">User data to be passed to the callback</param>
    /// <returns></returns>
    public static EANumberTween Start(float numFrom, float numTo, float speed, float maxTime, params object[] userData)
	{
		EANumberTween numTween = new EANumberTween(numFrom);

		numTween.StartTween(numTo, speed, maxTime, userData);

		return numTween;
	}

    public static EANumberTween Start(float numFrom, float numTo, float speed, float maxTime , EaseType easeType , params object[] userData)
    {
        EANumberTween numTween = new EANumberTween(numFrom , easeType);

        numTween.StartTween(numTo, speed, maxTime, userData);

        return numTween;
    }


    /// <summary>
    /// pause the tweener
    /// </summary>
    public void Pause()
    {
		if (_tweener != null)
        {
			HOTween.Pause(_tweener);
		}  
	}

	public void Restart()
	{
		if (_tweener != null)
		{
			HOTween.Restart(_tweener);
		}
	}

    public void Complete()
    {
        if (_tweener != null)
        {
            HOTween.Complete(_tweener);
        }
    }

    public void Kill()
    {
        if (_tweener != null)
        {
            HOTween.Kill(_tweener);
        }
    }
}
