using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

/// <summary>
/// animation management class
/// </summary>
public abstract class ActorAnimLegacy : ActorAnimBase
{
    /// <summary>
    /// animation information
    /// </summary>
    [Serializable]
    public class PlayAnimParam : IPlayAnimParam
    {
        public string aniName;
        public WrapMode warpMode;
        public bool isForcePlay = true; 
        public float fadeLength = 0;
       
        public PlayAnimParam(string aniName, 
                             WrapMode warpMode, 
                             bool isForcePlay, 
                             bool isCrossFade = true, 
                             float fLength = 0.0f)
        {
            this.aniName = aniName;
            this.warpMode = warpMode;
            this.isForcePlay = isForcePlay;
            this.fadeLength = fLength;
        }

        public void Init()
        {            
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

    protected Animation m_anim = null;

    void Awake()
    {
        m_anim = GetComponent<Animation>();

        ResetAnimState(animStates);
    }

    /// <summary>
    /// change animation state
    /// </summary>
    /// <param name="animParam"></param>
    protected override void ChangeAnim(AnimStateBase state)
    {
        List<PlayAnimParam> animParam = ((AnimState)state).playAnimParams;

        string aniTemp = "";

        for (int i = 0; i < animParam.Count; i++)
        {
            PlayAnimParam param = (PlayAnimParam)animParam[i];

            string aniName = param.aniName;

            if (m_anim[aniName] != null)
            {
                m_anim[aniName].wrapMode = param.warpMode;

                if (param.isForcePlay)
                {
                    m_anim.CrossFade(aniName, param.fadeLength);
                }
                else
                {
                    m_anim.CrossFadeQueued(aniName, param.fadeLength);
                }

                aniTemp += "/" + aniName;
            }
        }

        Debug.Log("#Animation# Play Anim : " + aniTemp);
    }
}
