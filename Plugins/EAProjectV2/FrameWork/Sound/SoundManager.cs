using System;
using UnityEngine;
using Debug = EAFrameWork.Debug;

// All sound play must be played through a manager. Otherwise, volume control will be lost
public class SoundManager : Singleton<SoundManager>
{
    public const string bgmVolume    = "BgmVolume";
    public const string sfxVolume    = "SfxVolume";
    public const float bgmMaxVolume  = 10.0f;
    public const float sfxMaxVolume  = 10.0f;

    AudioSource _mainAudio;
    AudioSource _subAudio;
    float mainAudioOriVolume;
    AudioClip _defaultClip;

    AudioLowPassFilter lowPassFilter = null;

    float lowPassDefault = 22000.0f;
    float lowPassLowValue = 7000.0f;
    float lowPassLowTime = 2.3f;

    BGMGroup bGMGroup = null;

    // BGM , SFX volume
    public Action<float, float> onChangeVolume;
    
    public override GameObject GetSingletonParent()
    {
        return EAMainFrame.instance.gameObject;
    }

    protected override void Awake()
    {
        base.Awake();

        Init();

        EAMainFrame.instance.OnMainFrameFacilityCreated(MainFrameAddFlags.SoundManager);
    }

    // Once the volume has been modified it can be called to call back
    public void OnAddListener_OnChangeVolume(Action<float, float> action)
    {
        onChangeVolume -= action;
        onChangeVolume += action;
    }

    public void RemoveListener_OnChangeVolume(Action<float, float> action)
    {
        onChangeVolume -= action;
    }

    public void RemoveAllListener_OnChangeVolume(Action<float, float> action)
    {
        onChangeVolume = null;
    }

    void OnChangeVolume(int value)
    {
        NotifyChangeVolume();
    }

    void NotifyChangeVolume()
    {
        if (onChangeVolume != null)
        {
            onChangeVolume(GetVolume(SOUND_TYPE.BGM), GetVolume(SOUND_TYPE.SFX));
        }

        if (_mainAudio != null)
        {
            _mainAudio.volume = GetBGMVolumeNormalized() * mainAudioOriVolume;
        }
    }

    public float GetBGMVolumeNormalized()
    {
        return OptionManager.instance.GetValueInRatio("BgmVolume", 0, bgmMaxVolume);
    }

    public float GetSFXVolumeNormalized()
    {
        return OptionManager.instance.GetValueInRatio("SfxVolume", 0, sfxMaxVolume);
    }

    // Returns the volume that matches the sound type.
    public float GetVolume(SOUND_TYPE type)
    {
        float result = 1;

        if (type == SOUND_TYPE.BGM)
        {
            result = GetBGMVolumeNormalized();
        }
        else if (type == SOUND_TYPE.SFX)
        {
            result = GetSFXVolumeNormalized();
        }

        return result;
    }

    // initialization
    void Init()
    {
        if (null == _mainAudio)
        {
            _mainAudio = gameObject.AddComponent<AudioSource>();
            _subAudio  = gameObject.AddComponent<AudioSource>();
            mainAudioOriVolume = _mainAudio.volume;
            _mainAudio.volume  = GetVolume(SOUND_TYPE.BGM) * _mainAudio.volume;
            _mainAudio.loop = true;
            _mainAudio.playOnAwake = false;
            _subAudio.playOnAwake = false;
        }

        if (null == _defaultClip)
        {
            _defaultClip = ResourceManager.instance.Load<AudioClip>("Sound/se/Btn_Default");
        }

        Debug.Assert(_mainAudio != null, "main audio is null");
    }

    public void LoadBGM()
    {
        if (bGMGroup == null)
        {
            GameObject obj = ResourceManager.instance.Load<GameObject>("Sound/BGM/BGMGroup");

            if (obj != null)
            {
                GameObject bgm = Instantiate(obj) as GameObject;

                bgm.name = "BGMGroup";
                bgm.transform.SetParent(gameObject.transform);
                bgm.transform.localScale = Vector3.one;
                bgm.transform.localPosition = Vector3.zero;

                bGMGroup = bgm.GetComponent<BGMGroup>();
            }
        }
    }

    public void Play(AudioSource source, float desiredVolume, SOUND_TYPE type)
    {
        SetVolume(source, desiredVolume, type);
        source.Play();
    }

    public void PlayOneShot(AudioClip clip, float volume , SOUND_TYPE type)
    {
        if (clip == null)
        {
            clip = _defaultClip;
        }

        if (null != _subAudio && clip != null)
        {
            PlayOneShot(_subAudio, clip, volume, type);
        }
    }

    public void PlayDelay(AudioSource source, float delay, float volume, SOUND_TYPE type)
    {
        SetVolume(source, volume, type);
        source.PlayDelayed(delay);
    }

    public void PlayOneShot(AudioSource source, AudioClip clip, float volume, SOUND_TYPE type)
    {
        SetVolume(source, volume, type);
        source.PlayOneShot(clip, GetVolume(type));
    }

    public void UnpauseAudio(AudioSource source, float desiredVolume, SOUND_TYPE type)
    {
        SetVolume(source, desiredVolume, type);
        source.UnPause();
    }

    public void SetVolume(AudioSource source, float desiredVolume, SOUND_TYPE type)
    {
        source.volume = GetVolume(type) * desiredVolume;
    }

    /// <summary>
    /// play bgm
    /// </summary>
    /// <param name="name"></param>
    /// <param name="useLowPassFilter"></param>
    public void PlayBGM(string name, bool useLowPassFilter = false)
    {
        if (bGMGroup != null)
        {
            BGMGroup.BGMSlot slot = bGMGroup.GetBGM(name);

            if(slot != null && slot.audioClip != null)
            {
                AudioClip clip = slot.audioClip;

                if (_mainAudio.clip == null ||
                    (_mainAudio.clip != null && !_mainAudio.clip.name.Equals(clip.name)) ||
                    !_mainAudio.isPlaying)
                {
                    _mainAudio.clip = clip;
                    _mainAudio.loop = slot.loop;

                    if(lowPassFilter != null)
                    {
                        lowPassFilter.cutoffFrequency = (useLowPassFilter == true) ? lowPassLowValue : lowPassDefault;
                    }  
                  
                    Play(_mainAudio, slot.volume, SOUND_TYPE.BGM);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uselowPassFilter"></param>
    public void SetLowPassFilter(bool uselowPassFilter, float desiredVolume = 1.0f)
    {
        if (lowPassFilter != null)
        {
            float frequency = (uselowPassFilter == true) ? lowPassLowValue : lowPassDefault;

            float numFrom = lowPassFilter.cutoffFrequency;
            float numTo = frequency;

            SetVolume(_mainAudio, desiredVolume, SOUND_TYPE.BGM);

            //Debug.Log("SetLowPassFilter test : uselowPassFilter : " + uselowPassFilter + " time " + Time.time);

            if (uselowPassFilter == false)
            {

                var tweener = EANumberTween.Start(numFrom, numTo, 0, lowPassLowTime);

                tweener.onUpdate = delegate (EANumberTween.Event e)
                {
                    lowPassFilter.cutoffFrequency = e.number;
                };

                tweener.onComplete = delegate (EANumberTween.Event e)
                {
                    lowPassFilter.cutoffFrequency = frequency;
                    //Debug.Log("SetLowPassFilter test ok : uselowPassFilter : " + uselowPassFilter + " time " + Time.time);
                };
            }
            else
            {
                lowPassFilter.cutoffFrequency = frequency;
                //Debug.Log("SetLowPassFilter test ok : uselowPassFilter : " + uselowPassFilter + " time " + Time.time);
            }
        }
    }

    public void CreateLowPassFilter()
    {
        if (lowPassFilter == null)
        {
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            lowPassFilter.cutoffFrequency = 5000.0f;

            //Debug.Log("SoundManager CreateLowPassFilter frameCount : " + Time.frameCount);
        }
    }

    public void StopBGM()
    {
        if (_mainAudio != null)
        {
            _mainAudio.Stop();
            _mainAudio.clip = null;
        }
    }

    public void StopSfxSound()
    {
        if (_subAudio != null)
        {
            _subAudio.Stop();
            _subAudio.clip = null;
        }
    }
}
