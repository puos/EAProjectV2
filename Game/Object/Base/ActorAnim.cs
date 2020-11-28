using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;


/// <summary>
/// animation management class
/// </summary>
public abstract class ActorAnim : ActorAnimBase
{
    /// <summary>
    /// animation information
    /// </summary>
    [Serializable]
    public class PlayAnimParam : IPlayAnimParam
    {
        public enum Type { Trigger , Integer }
        public string aniName;
        public Type type;
        public int value;

        
        [HideInInspector] public int paramId;

        public void Init()
        {
            this.paramId = Animator.StringToHash(aniName);
        }
    }

    [Serializable]
    public class AnimState : AnimStateBase
    {
        public List<PlayAnimParam> playAnimParams = new List<PlayAnimParam>();

        public override void Init()
        {
            for (int i = 0; i < playAnimParams.Count; ++i)
            {
                playAnimParams[i].Init();
            }
        }
    }

    [SerializeField]
    public AnimState[] animStates = null;

    Animator m_anim = null;

    void Awake()
    {
        m_anim = GetComponent<Animator>();

        ResetAnimState(animStates);
    }

    /// <summary>
    /// change animation state
    /// </summary>
    /// <param name="animParam"></param>
    protected override void ChangeAnim(AnimStateBase state)
    {
        if (m_anim == null)
            return;

        List<PlayAnimParam> animParam = ((AnimState)state).playAnimParams;

        string aniTemp = "";

        for (int i = 0; i < animParam.Count; i++)
        {
            string aniName = ((PlayAnimParam)animParam[i]).aniName;
            int value      = ((PlayAnimParam)animParam[i]).value;
            PlayAnimParam.Type type = ((PlayAnimParam)animParam[i]).type;

            if (type == PlayAnimParam.Type.Trigger)
            {
                m_anim.SetTrigger(((PlayAnimParam)animParam[i]).paramId);
                aniTemp += "/" + aniName;
                continue;
            }

            m_anim.SetInteger(((PlayAnimParam)animParam[i]).paramId, value);   
            aniTemp += "/" + aniName + ":" + value;
        }

        Debug.Log(gameObject.name + " # Play Anim # " + aniTemp);
    }

    public override void ClearAnimation(string key = "", bool rebind = true)
    {
        if (rebind == true)
        {
            if (m_anim != null)
            {
                m_anim.Rebind();
            }
        }

        int count = _animStateQueue.Count;

        for (int i = 0; i < count; ++i)
        {
            if (_animStateQueue.Count > 0)
            {
                AnimStateBase state = _animStateQueue.Peek();

                if (state.key.Equals(key) || string.IsNullOrEmpty(key))
                {
                    _animStateQueue.Dequeue();
                }
            }
        }
    }
}
