using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

public class AnimUtil
{
    public static List<string> GetPlayingClipNames(Animation anim)
    {
        List<string> states = new List<string>();

        IEnumerator i = anim.GetEnumerator();
        while (i.MoveNext())
        {
            AnimationState state = (AnimationState)i.Current;
            if (anim.IsPlaying(state.name))
            {
                states.Add(state.name);
            }    
        }
        return states;
    }

    public static bool HasClip(Animation anim, string clipName)
	{
		AnimationState state = GetAnimClipAnimState(anim, clipName);
		return state == null ? false : true;
	}
	public static void SetPosition(Animation anim, float time)
	{
		AnimationState state = GetFirstAnimState(anim);
		state.time = time;
		anim.Sample();
	}
	public static void SetPosition(Animation anim, string clipName, float time)
	{
		AnimationState state = GetAnimClipAnimState(anim, clipName);
		if (state != null)
		{
			state.time = time;
			anim.Sample();
		}
	}
	public static float GetPosition(Animation anim , bool isNormal = false)
	{
		AnimationState state = GetFirstAnimState(anim);

        if (state == null)
            return 0;
        return (isNormal == false) ? state.time : state.normalizedTime;
	}
      
    public static float GetPosition(Animation anim, string clipName , bool isNormal = false)
	{
		AnimationState state = GetAnimClipAnimState(anim, clipName);
		if (state == null)
			return 0;
		else
            return (isNormal == false) ? state.time : state.normalizedTime;
    }
	public static float GetLength(Animation anim, string clipName)
	{
		AnimationState state = GetAnimClipAnimState(anim, clipName);
		if (state == null)
			return 0;
		else
			return state.clip.length;
	}

  
    public static float GetLength(Animation anim)
    {
        AnimationState state = GetFirstAnimState(anim);

        if (state == null)
            return 0;
        else
            return state.clip.length;
    }

    public static void SetPosition01(Animation anim, float normalizedTime)
	{
		AnimationState state = GetFirstAnimState(anim);
		state.time = normalizedTime * state.length;
		//anim.Sample();
		state.speed = 0;
		
	}
    public static void SetPosition01(Animation anim, string clipName, float normalizedTime)
    {
        AnimationState state = GetAnimClipAnimState(anim, clipName);

        if(state != null)
        {
            state.time = normalizedTime * state.length;
            state.speed = 0;
        }   
    }
    public static void SetPositionToBegin(Animation anim, bool stopAnim = true)
	{
		AnimationState state = GetFirstAnimState(anim);
		state.time = 0;
		PlayAnimBackward(anim, 1, false);
	}

    public static void SetPositionToBegin(Animation anim, string clipName, bool stopAnim = true)
    {
        AnimationState state = GetAnimClipAnimState(anim, clipName);

        if (state == null)
            state = GetFirstAnimState(anim);

        state.time = 0;
        PlayAnimBackward(anim, state , 1 , false);
    }

    public static void SetPositionToEnd(Animation anim, bool stopAnim = true)
	{
		AnimationState state = GetFirstAnimState(anim);
		state.time = state.length;
		PlayAnimForward(anim, 1, false);
	}

    public static void SetPositionToEnd(Animation anim, string clipName, bool stopAnim = true)
    {
        AnimationState state = GetAnimClipAnimState(anim, clipName);

        if (state == null)
            state = GetFirstAnimState(anim);

        state.time = state.length;
        PlayAnimForward(anim, state, 1, false);
    }

    public static void PlayAnimForward(Animation anim, float speed = 1, bool rewind = true)
	{
		AnimationState state = GetFirstAnimState(anim);
		PlayAnimForward(anim, state, speed, rewind);
	}

	public static void PlayAnimForward(Animation anim, string clipName, float speed = 1, bool rewind = true , bool stopAll = false)
	{
		AnimationState state = GetAnimClipAnimState(anim, clipName);

        if(state == null)
           state = GetFirstAnimState(anim);

        PlayAnimForward(anim, state, speed, rewind , stopAll);
	}
	public static void PlayAnimForward(Animation anim, AnimationState state, float speed = 1, bool rewind = true , bool stopAll = false)
	{
		if (state == null)
			return;
		if (rewind)
			state.time = 0;
		state.speed = speed;
		anim.clip = state.clip;

        if(stopAll == false)
        {
            anim.Play();
        } 
        else 
        {
            anim.Play(PlayMode.StopAll);
        }   
	}

	public static void PlayAnimBackward(Animation anim, float speed = 1, bool rewind = true)
	{
		AnimationState state = GetFirstAnimState(anim);
		PlayAnimBackward(anim, state, speed, rewind);
	}

	public static void PlayAnimBackward(Animation anim, string clipName, float speed = 1, bool rewind = true)
	{
		AnimationState state = GetAnimClipAnimState(anim, clipName);
		PlayAnimBackward(anim, state, speed, rewind);
	}

	public static void PlayAnimBackward(Animation anim, AnimationState state, float speed = 1, bool rewind = true)
	{
		if (state == null)
			return;
		if (rewind)
			state.time = state.length;
		state.speed = -1f * speed;
		anim.Play();
	}

	public static void SetAnimSpeed(Animation anim, float speed)
	{
		AnimationState state = GetFirstAnimState(anim);
        state.speed = speed;
    }

    public static void SetAnimSpeed(Animation anim, string clipName, float speed)
    {
        AnimationState state = GetAnimClipAnimState(anim,clipName);
        if(state != null)
             state.speed = speed;
    }


    public static AnimationState GetAnimClipAnimState(Animation anim, string clipName)
	{
		IEnumerator i = anim.GetEnumerator();
		while(i.MoveNext())
		{
			AnimationState state = (AnimationState)i.Current;
			if (CRC32.GetHashForAnsi(state.clip.name) == CRC32.GetHashForAnsi(clipName))
				return state;
		}
		return null;
	}
	public static AnimationState GetFirstAnimState(Animation anim)
	{
		IEnumerator i = anim.GetEnumerator();
		i.MoveNext();
		return (AnimationState)i.Current;
	}

    public static void TimeScaleZeroPlay(MonoBehaviour mb, Animation animation, System.Action onComplete)
    {
        mb.StartCoroutine( AnimUtil.CtimeScaleZeroPlay(animation , onComplete));  
    } 

    public static IEnumerator CtimeScaleZeroPlay(Animation animation, System.Action onComplete)
    {
        AnimationState _currState = GetFirstAnimState(animation);

        bool isPlaying = true;

        float _progressTime = 0f;
        float _timeAtLastFrame = 0f;
        float _timeAtCurrentFrame = 0f;
        float deltaTime = 0f;

        animation.Play();

        _timeAtLastFrame = Time.realtimeSinceStartup;

        while (isPlaying)
        {
            _timeAtCurrentFrame = Time.realtimeSinceStartup;
            deltaTime = _timeAtCurrentFrame - _timeAtLastFrame;
            _timeAtLastFrame = _timeAtCurrentFrame;

            _progressTime += deltaTime;
            _currState.normalizedTime = _progressTime / _currState.length;

            animation.Sample();

            //Debug.Log(_progressTime);

            if (_progressTime >= _currState.length)
            {
                //Debug.Log(&quot;Bam! Done animating&quot;);
                if (_currState.wrapMode != WrapMode.Loop)
                {
                    //Debug.Log(&quot;Animation is not a loop anim, kill it.&quot;);
                    //_currState.enabled = false;
                    isPlaying = false;
                }
                else
                {
                    //Debug.Log(&quot;Loop anim, continue.&quot;);
                    _progressTime = 0.0f;
                }
            }

            yield return new WaitForEndOfFrame();
        }

        yield return null;

        if (onComplete != null)
        {
            Debug.Log("Complete");
            onComplete();
        }
    }
}
