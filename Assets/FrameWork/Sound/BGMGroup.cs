using UnityEngine;

[System.Serializable]
public abstract class BGMGroup : MonoBehaviour
{
    [System.Serializable]
    public class BGMSlot
    {
        public string name;
        public bool   loop;

        [Range(0,1)]
        public float  volume;

        public AudioClip audioClip;
    };
    
    [Header("BGM Source")]
    public BGMSlot[] audioClip;

    public BGMSlot GetBGM(string name)
    {
        for (int i = 0; i < audioClip.Length; i++)
        {
            if (audioClip[i].name.Equals(name))
            {
                return audioClip[i];
            } 
        }

        return null;
    }
}
