using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

public enum AnimationEventType
{
    None,
    Impact,
    Sfx,
    PlaySound,
}

/// <summary>
/// animation management class
/// </summary>
public abstract class ActorAnimBase : MonoBehaviour
{
    public interface IPlayAnimParam
    {
        void Init();
    }

    /// <summary>
    ///  animation event callback
    /// </summary>
    /// <param name="slotName"></param>
    public delegate void AnimEventCallback(ActorAnimBase anim, AnimationEventType eventType, string slotName);


    /// <summary>
    /// animation param slot callback
    /// </summary>
    /// <param name="targetObj"></param>
    public delegate void AnimParamSlotCallback(GameObject targetObj);

    /// <summary>
    /// animation interaction data
    /// </summary>
    [Serializable]
    public class AnimParamSlot
    {
        public string name;
        public AnimParamSlotCallback callback;
        public GameObject innerObj;
    };

    [Serializable]
    public class AnimStateBase
    {
        public string key;
        
        [SerializeField]
        public AnimParamSlot[] m_paramSlots;

        public void SetParam(string slotName, AnimParamSlotCallback cb)
        {
            int slotIdx = FindParamSlotIdxByName(slotName);
            if (slotIdx != -1)
            {
                AnimParamSlot slot = m_paramSlots[slotIdx];
                slot.callback = cb;
                if (slot.innerObj != null)
                    cb(slot.innerObj);
            }
        }

        public void SetInnerObject(string slotName, GameObject obj)
        {
            int slotIdx = FindParamSlotIdxByName(slotName);
            if (slotIdx != -1)
            {
                AnimParamSlot slot = m_paramSlots[slotIdx];
                slot.innerObj = obj;
            }
        }

        int FindParamSlotIdxByName(string name)
        {
            for (int i = 0; i < m_paramSlots.Length; i++)
                if (m_paramSlots[i].name.Equals(name))
                    return i;
            return -1;
        }

        public virtual void Init()
        {           
        }
    }

    protected Queue<AnimStateBase> _animStateQueue = new Queue<AnimStateBase>();

    Dictionary<int, AnimStateBase> dic_animStates = new Dictionary<int, AnimStateBase>();

    /// <summary>
    ///  animation state find
    /// </summary>
    /// <param name="_key"></param>
    /// <returns></returns>
    public AnimStateBase GetAnimState(string _key)
    {
        int key = CRC32.GetHashForAnsi(_key);

        AnimStateBase state = null;

        dic_animStates.TryGetValue(key, out state);

        return state;
    }

    void Update()
    {
        ProcessActionState();
    }

    /// <summary>
    /// 
    /// </summary>
    private void ProcessActionState()
    {
        if (_animStateQueue.Count > 0)
        {
            AnimStateBase state = _animStateQueue.Peek();

            ChangeAnim(state);

            _animStateQueue.Dequeue();
        }
    }

    /// <summary>
    /// change animation state
    /// </summary>
    /// <param name="animParam"></param>
    protected abstract void ChangeAnim(AnimStateBase state);

    /// <summary>
    /// stop animation
    /// </summary>
    public abstract void ClearAnimation(string key = "", bool rebind = true);

    /// <summary>
    /// Change animations and events
    /// </summary>
    protected void ResetAnimState(AnimStateBase[] animStates)
    {
        dic_animStates.Clear();

        for (int i = 0; i < animStates.Length; ++i)
        {
            animStates[i].Init();

            int key = CRC32.GetHashForAnsi(animStates[i].key);

            AnimStateBase state = null;

            if (!dic_animStates.TryGetValue(key, out state))
            {
                dic_animStates.Add(key, animStates[i]);
            }
            else
            {
                dic_animStates[key] = animStates[i];
            }
        }
    }

    /// <summary>
    /// animState push
    /// </summary>
    public void PushAnimation(string key)
    {
        AnimStateBase state = GetAnimState(key);

        if (state != null)
        {
            _animStateQueue.Enqueue(state);
        }
    }

    [System.NonSerialized]
    public AnimEventCallback eventCallback;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="iter"></param>
    public void AnimationEvent_Impact(string iter)
    {
        SendEventToOwner(AnimationEventType.Impact, iter);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="param"></param>
    void SendEventToOwner(AnimationEventType type, string param)
    {
        if (eventCallback != null)
        {
            eventCallback(this, type, param);
        }
    }
}
