using System.Collections.Generic;
using UnityEngine;
using Debug = EAFrameWork.Debug;

[ExecuteInEditMode]
public abstract class SoundCue : MonoBehaviour
{
    public enum eMethod
    {
        PlayOne,
        Random,
        PlayAll
    }

    [Header("Setting Sound Source")]
    public List<AudioClip> audioClip = new List<AudioClip>();
   
    [HideInInspector] public int playMin = 0;
    [HideInInspector] public int playMax = 0;

    [Header("Setting Sound Play")]
    [Range(-3.0f, 3.0f)]
    public float pitch = 1.0f;
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
    [HideInInspector]  public eMethod method = eMethod.PlayOne;


    public void PlaySound()
    {
        switch (method)
        {
            case eMethod.PlayOne:
                PlayOnce();
                break;

            case eMethod.Random:
                PlayRandom();
                break;

            case eMethod.PlayAll:
                PlayAll();
                break;
            default:
                break;
        }
    }

    void PlayOnce()
    {
        if (audioClip.Count > playMin)
        {
            Debug.Log("playSound sfx index : " + playMin);

            if (audioClip[playMin] != null)
            {
                if (SoundManager.instance != null)
                {
                    SoundManager.instance.PlayOneShot(audioClip[playMin], volume , SOUND_TYPE.SFX);
                }
            }
        }
    }

    void PlayRandom()
    {
        int randomValue = Random.Range(playMin, playMax + 1);

        if (audioClip.Count > randomValue)
        {
            Debug.Log("playSound sfx index : " + randomValue);

            if (audioClip[randomValue] != null)
            {
                if (SoundManager.instance != null)
                {
                    SoundManager.instance.PlayOneShot(audioClip[randomValue], volume, SOUND_TYPE.SFX);
                }
            }
        }
    }

    void PlayAll()
    {
        for (int i = playMin; i <= playMax; i++)
        {
            if (audioClip.Count > i)
            {
                Debug.Log("playSound index : " + i);

                if (audioClip[i] != null)
                {
                    if (SoundManager.instance != null)
                    {
                        SoundManager.instance.PlayOneShot(audioClip[i], volume, SOUND_TYPE.SFX);
                    }
                }
            }
        }
    }
} 
