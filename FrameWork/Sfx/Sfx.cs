using System.Collections.Generic;
using UnityEngine;
using System;
using Debug = EAFrameWork.Debug;

public interface SfxParamSlotListener
{
    void OnSfxParamSlotAssigned(int slotIdx, Sfx.SfxParamSlot slot);
}

public enum SfxEventType
{
    None,
    Impact,
    Particle,
    ActiveSfxGroup,
    DeActiveSfxGroup,
    PlaySound,
    Decay, // It means the main production is over. If there is no explicit Decay event, it is always created just before Destroy.
    Destroy,
    CutSceneStart,
    CutSceneEnd
}

/// <summary>
/// Sfx event callbacks.
/// </summary>
/// <param name="slotName">List of gameObjectSlots defined in SfxContainer</param>
public delegate void SfxEventCallback(Sfx sfx, SfxEventType eventType, string slotName);

/// <summary>
/// sfx param slot callback
/// </summary>
/// <param name="targetObj"></param>
public delegate void SfxParamSlotCallback(GameObject targetObj);

[ExecuteInEditMode]
public abstract class Sfx : MonoBehaviour
{
    [System.Serializable]
    public class SfxParamSlot
    {
        public string name;
        public SfxParamSlotCallback callback; // Callback to handle connection
        public GameObject innerObj;           // [Optional] Internal Object to connect
    };

    public SoundCue soundCue = null;

    public Animation m_anim = null;

    public List<ParticleSystem> m_particles = new List<ParticleSystem>();

    public enum SfxCategory
    {
        sfxWorld = 0,
        sfxUi = 1,
        sfxCommon = 2,
    }

    [SerializeField]
    SfxParamSlot[] m_paramSlots = null;

    public SfxCategory sfxCategory = SfxCategory.sfxCommon;

    private void Awake()
    {
        if (m_anim == null)
        {
            m_anim = gameObject.GetComponent<Animation>();
        }

        if (soundCue == null)
        {
            soundCue = gameObject.GetComponent<SoundCue>();
        }

        if (m_anim != null)
        {
            m_anim.playAutomatically = false;
        }

        for (int i = 0; i < m_particles.Count; ++i)
        {
            ParticleSystem.MainModule main = EAFrameUtil.Call<ParticleSystem.MainModule>(m_particles[i].main);
            main.playOnAwake = false;
        }  

        ParticleSystem selfparticle = gameObject.GetComponent<ParticleSystem>();

        if (selfparticle != null)
        {
            int idx = m_particles.FindIndex(x => x.GetInstanceID() == selfparticle.GetInstanceID());

            if (idx == -1)
            {
                ParticleSystem.MainModule main = EAFrameUtil.Call<ParticleSystem.MainModule>(selfparticle.main);
                main.playOnAwake = false;

                m_particles.Add(selfparticle);
            }
        }
    }


    /// <summary>
    /// Sets the param value.
    /// </summary>
    /// <param name="slotIdx"></param>
    /// <param name="cb"></param>
    public void SetParam(int slotIdx, SfxParamSlotCallback cb)
    {
        if (slotIdx >= 0 && m_paramSlots != null && slotIdx < m_paramSlots.Length)
        {
            SfxParamSlot slot = m_paramSlots[slotIdx];
            slot.callback = cb;
            if (slot.innerObj != null)
                cb(slot.innerObj);

            _OnParamSlotAssigned(slotIdx, slot);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slotName"></param>
    /// <param name="cb"></param>
    public void SetParam(string slotName, SfxParamSlotCallback cb)
    {
        int slotIdx = FindParamSlotIdxByName(slotName);
        if (slotIdx != -1)
        {
            SfxParamSlot slot = m_paramSlots[slotIdx];
            slot.callback = cb;
            if (slot.innerObj != null)
                cb(slot.innerObj);

            _OnParamSlotAssigned(slotIdx, slot);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private int FindParamSlotIdxByName(string name)
    {
        for (int i = 0; i < m_paramSlots.Length; i++)
            if (m_paramSlots[i].name == name)
                return i;
        return -1;
    }

    void _OnParamSlotAssigned(int slotIdx, SfxParamSlot slot)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private float getAnimCurrTime()
    {
        foreach (AnimationState state in m_anim)
            return state.time;

        return 0;
    }

    /// <summary>
    /// Compare current category
    /// </summary>
    /// <param name="_sfxCategory"></param>
    /// <returns></returns>
    private bool IsCategory(SfxCategory _sfxCategory)
    {
        return (sfxCategory == _sfxCategory) ? true : false;
    }

    /// <summary>
    ///  play sound using sfx
    /// </summary>
    /// <param name="index"> 0: method , 1: min , 2: max </param>
    public void AnimEvent_PlaySound(string index)
    {
        Debug.Assert(soundCue != null, "sfx - soundCue is null :" + gameObject.name);

        if (soundCue != null)
        {
            string szSeparateExt = ";";
            string[] v = index.Split(szSeparateExt.ToCharArray());

            Debug.Assert(v.Length >= 2, "PlaySound is invalid : " + gameObject.name + " arg : " + index);

            if (v.Length == 2)
            {
                soundCue.method = (SoundCue.eMethod)System.Convert.ToInt32(v[0]);
                soundCue.playMin = soundCue.playMax = System.Convert.ToInt32(v[1]);
            }
            else if (v.Length > 2)
            {
                soundCue.method = (SoundCue.eMethod)System.Convert.ToInt32(v[0]);
                soundCue.playMin = System.Convert.ToInt32(v[1]);
                soundCue.playMax = System.Convert.ToInt32(v[2]);
            }

            soundCue.PlaySound();

            SendEventToOwner(SfxEventType.PlaySound, index);
        }
    }

    [System.NonSerialized]
    public SfxEventCallback eventCallback;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="iter"></param>
    public void AnimEvent_Impact(int iter)
    {
        SendEventToOwner(SfxEventType.Impact, iter);
    }

    public void AnimEvent_Decay()
    {
        SendEventToOwner(SfxEventType.Decay, -1);

        Decay();
    }

    /// <summary>
    /// Plays back the Animation Clip with the specified name.
    /// </summary>
    /// <param name="clipName"></param>
    public void AnimEvent_PlayAnim(string clipName)
    {
        if (m_anim == null)
        {
            return;
        }

        AnimUtil.PlayAnimForward(m_anim, clipName);
    }

    /// <summary>
    ///  Activate particle
    /// </summary>
    /// <param name="particleName"></param>
    void AnimEvent_PlayParticle(string particleName)
    {
        string szSeparateExt = ";";

        string[] v = particleName.Split(szSeparateExt.ToCharArray());

        for(int i = 0; i < v.Length; ++i)
        {
            ParticleSystem ps = null;

            ps = GetParticles(v[i]);

            if (ps != null)
            {
                SendEventToOwner(SfxEventType.Particle, v[i]);

                ps.gameObject.SetActive(true);

                ps.Play();
            }

        }
    }

    ParticleSystem GetParticles(string name)
    {
        ParticleSystem ps = null;

        for (int i = 0; i < m_particles.Count; ++i)
        {
            if (m_particles[i].name.Equals(name))
            {
                ps = m_particles[i];
            }
        }
        return ps;
    }

    void AnimEvent_PlaySfxGroup(string active_group)
    {
        string szSeparateExt = ";";

        string[] v = active_group.Split(szSeparateExt.ToCharArray());

        Debug.Assert(v.Length >= 2, "AnimEvent_ActiveSfxGroup is invalid : " + active_group);

        bool active = false;

        active = (v.Length < 2) ? true : (v[1].Equals("true") ? true : false);

        SetParam(v[0], delegate (GameObject go)
        {
            if (go != null)
            {
                Debug.Log("Active Sfx Group : " + active_group + " frameCount : " + Time.frameCount);

                if (active)
                {
                    SendEventToOwner(SfxEventType.ActiveSfxGroup, v[0]);
                }
                else
                {
                    SendEventToOwner(SfxEventType.DeActiveSfxGroup, v[0]);
                }
            }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    public void Decay()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="param"></param>
    void SendEventToOwner(SfxEventType type, int param)
    {
        if (eventCallback != null)
        {
            eventCallback(this, type, param.ToString());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="param"></param>
    void SendEventToOwner(SfxEventType type, string param)
    {
        if (eventCallback != null)
        {
            eventCallback(this, type, param);
        }
    }

    /// <summary>
    /// sfx 를 start 함 
    /// </summary>
    public void StartSfx(string clipName = null, bool stopAll = false, float speed = 1.0f)
    {
        if (m_anim != null)
        {
            AnimUtil.SetPositionToBegin(m_anim);

            if (string.IsNullOrEmpty(clipName))
            {
                AnimUtil.PlayAnimForward(m_anim, speed);
            }
            else
            {
                AnimUtil.PlayAnimForward(m_anim, clipName, speed, true, stopAll);
            }
        }
        else
        {
            for (int i = 0; i < m_particles.Count; ++i)
            {
                ParticleSystem x = m_particles[i];
                x.Play();
            }

            if(soundCue != null)
            {
                soundCue.method = SoundCue.eMethod.PlayAll;
                soundCue.playMin = 0;
                soundCue.playMax = Math.Max(soundCue.audioClip.Count , 0);
                soundCue.PlaySound();
            } 
        }   
    }

    /// <summary>
    /// 
    /// </summary>
    public void StopSfx(bool stopAni = false)
    {
        if (m_anim != null)
        {
            List<string> clips = AnimUtil.GetPlayingClipNames(m_anim);

            if (clips.Count > 0)
            {
                clips.ForEach(x =>
                {
                    AnimUtil.SetPositionToEnd(m_anim, x);
                });
            }
            else
            {
                AnimUtil.SetPositionToEnd(m_anim);
            }

            if (stopAni == true)
            {
                m_anim.Stop();
            }
        }
        else
        {
            for (int i = 0; i < m_particles.Count; ++i)
            {
                ParticleSystem x = m_particles[i];
                x.Stop();
            }
        }  
        
        eventCallback = null;
    }

    /// <summary>
    /// sfx cutscene set
    /// </summary>
    /// <param name="animationName"></param>
    void OnCutScene(string animationName)
    {
        SendEventToOwner(SfxEventType.CutSceneStart, animationName);
    }

    void OnCutSceneEnd(string animationName)
    {
        SendEventToOwner(SfxEventType.CutSceneEnd, animationName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animationName"></param>
    void ChangeAnimationState(string animationName)
    {
        if (m_anim == null)
            return;

        AnimationState state = AnimUtil.GetAnimClipAnimState(m_anim, animationName);

        if (state != null)
        {
            m_anim.clip = state.clip;
            AnimUtil.SetPositionToBegin(m_anim, animationName);
        }
    }

    public bool IsAlive()
    {
        if (m_anim != null)
        {
            return m_anim.isPlaying;
        }

        if (m_particles.Count > 0)
        {
            bool check = false;

            for(int i = 0; i < m_particles.Count; ++i)
            {
                ParticleSystem x = m_particles[i];
                check = (x.IsAlive() == true) ? true : check;
            }

            return check;
        }

        return true;
    }
}
