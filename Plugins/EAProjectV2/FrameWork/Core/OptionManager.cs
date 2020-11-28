using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = EAFrameWork.Debug;

/// <summary>
/// Manage Option Values
/// </summary>
public class OptionManager : Singleton<OptionManager>
{
    [System.Serializable]
    public class OptionValue
    {
        int value;
        string value_string;

        public string key { get; private set; }

        public Action<int> onChange;
        public Action<string> onChange_string;

        public OptionValue(string key, int value)
        {
            this.key = key;
            this.value = value;
        }

        public OptionValue(string key, string value)
        {
            this.key = key;
            value_string = value;
        }

        public void Set(int value)
        {
            bool notifyChange = this.value != value;
            this.value = value;

            if (notifyChange &&
                onChange != null)
            {
                onChange(value);
            }
        }

        public void Set(string value)
        {
            bool notifyChange = this.value_string != value;
            this.value_string = value;

            if (notifyChange &&
                onChange_string != null)
            {
                onChange_string(value);
            }
        }

        public int GetValue()
        {
            return value;
        }

        public string GetValue_String()
        {
            return value_string;
        }
    }

    // Dictionary<int, int> valueRefTable;
    // If the key is enum, internal boxing occurs 
    Dictionary<int, OptionValue> optionValues = new Dictionary<int, OptionValue>();
    Dictionary<int, OptionValue> optionValues_string = new Dictionary<int, OptionValue>();

    const int optionDefaultValue = 1;

     public static string playerPrefKeyPrefix = "";

    public override GameObject GetSingletonParent()
    {
        return EAMainFrame.instance.gameObject;
    }

    protected override void Awake()
    {
        base.Awake();

        EAMainFrame.instance.OnMainFrameFacilityCreated(MainFrameAddFlags.OptionManager);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        SaveToDisc();
    }

    static public string MakePlayerPrefKey(string option)
    {
        return string.Concat(playerPrefKeyPrefix + "_Int_", option);
    }

    static public string MakePlayerPrefKeyString(string option)
    {
        return string.Concat(playerPrefKeyPrefix + "_String_", option);
    }

    // player pref save 
    public void SaveToDisc()
    {
        foreach (var keyPair in optionValues)
        {
            PlayerPrefs.SetInt(keyPair.Value.key, keyPair.Value.GetValue());
        }

        foreach (var keyPair in optionValues_string)
        {
            PlayerPrefs.SetString(keyPair.Value.key, keyPair.Value.GetValue_String());
        }

        PlayerPrefs.Save();

        Debug.Log("OptionManager - saveToDisc");
    }
       
    public void SetOptionValue(string option, int value)
    {
        var optionConverted = CRC32.GetHashForAnsi(option);

        OptionValue Value = null;

        if(optionValues.TryGetValue(optionConverted,out Value))
        {
            Value.Set(value);
        }
        else
        {
            optionValues.Add(optionConverted, new OptionValue(MakePlayerPrefKey(option), value));
        } 
    }

    public void SetOptionValueString(string option, string value)
    {
        var optionConverted = CRC32.GetHashForAnsi(option);

        OptionValue Value = null;

        if (optionValues_string.TryGetValue(optionConverted, out Value))
        {
            Value.Set(value);
        }
        else
        {
            optionValues_string.Add(optionConverted, new OptionValue(MakePlayerPrefKeyString(option), value));
        }
    }
    
    public void AddListener(string option, Action<int> listener)
    {
        var optionConverted = CRC32.GetHashForAnsi(option);

        OptionValue Value = null;

        if (optionValues.TryGetValue(optionConverted, out Value))
        {
            optionValues[optionConverted].onChange += listener;
        }
    }

    public void RemoveListener(string option, Action<int> listener)
    {
        var optionConverted = CRC32.GetHashForAnsi(option);

        OptionValue Value = null;

        if (optionValues.TryGetValue(optionConverted, out Value))
        {
            optionValues[optionConverted].onChange += listener;
        }
    }

    public void AddListener(string option, Action<string> listener)
    {
        var optionConverted = CRC32.GetHashForAnsi(option);

        OptionValue Value = null;

        if (optionValues_string.TryGetValue(optionConverted, out Value))
        {
            optionValues_string[optionConverted].onChange_string += listener;
        }
    }

    public void RemoveListener(string option, Action<string> listener)
    {
        var optionConverted = CRC32.GetHashForAnsi(option);

        OptionValue Value = null;

        if (optionValues_string.TryGetValue(optionConverted, out Value))
        {
            optionValues_string[optionConverted].onChange_string -= listener;
        }
    }

    public int Get(string option)
    {
        int optionConverted = 0;

        if(!IsOptionValue(optionValues, option, out optionConverted))
        {
            return int.MinValue;
        } 

        return optionValues[optionConverted].GetValue();
    }

    public string Get_String(string option)
    {
        int optionConverted = 0;

        if (!IsOptionValue(optionValues_string , option, out optionConverted))
        {
            return String.Empty;
        }

        return optionValues_string[optionConverted].GetValue_String();
    }

    bool IsOptionValue(Dictionary<int, OptionValue> options, string option, out int optionConverted)
    {
        optionConverted = CRC32.GetHashForAnsi(option);

        OptionValue Value = null;

        if (options.TryGetValue(optionConverted, out Value))
        {
            return true;
        }

        return false;
    }

    public bool IsOptionValue(string option)
    {
        int optionConverted = 0;

        return IsOptionValue(optionValues, option, out optionConverted);
    }

    public bool IsOptionValue_String(string option)
    {
        int optionConverted = 0;

        return IsOptionValue(optionValues_string, option, out optionConverted);
    }

    // os langauge - To the correct language for your project
    public LANGUAGE_TYPE ConvertSystemLanguage()
    {
        SystemLanguage sysLang = Application.systemLanguage;
        Debug.Log("System Language : " + sysLang.ToString());
        LANGUAGE_TYPE result = LANGUAGE_TYPE.ENGLISH;

        switch (sysLang)
        {
            case SystemLanguage.Chinese:
                result = LANGUAGE_TYPE.CHINESE_TRADITIONAL;
                break;
            case SystemLanguage.ChineseSimplified:
                result = LANGUAGE_TYPE.CHINESE_SIMPLIFIED;
                break;
            case SystemLanguage.ChineseTraditional:
                result = LANGUAGE_TYPE.CHINESE_TRADITIONAL;
                break;
            case SystemLanguage.English:
                result = LANGUAGE_TYPE.ENGLISH;
                break;
            case SystemLanguage.French:
                result = LANGUAGE_TYPE.FRANCE;
                break;
            case SystemLanguage.German:
                result = LANGUAGE_TYPE.GERMAN;
                break;
            case SystemLanguage.Indonesian:
                result = LANGUAGE_TYPE.INDONESIA;
                break;
            case SystemLanguage.Italian:
                result = LANGUAGE_TYPE.ITALY;
                break;
            case SystemLanguage.Japanese:
                result = LANGUAGE_TYPE.JAPAN;
                break;
            case SystemLanguage.Korean:
                result = LANGUAGE_TYPE.KOREAN;
                break;
            case SystemLanguage.Portuguese:
                result = LANGUAGE_TYPE.PORTUGAL_AND_BRAZIL;
                break;
            case SystemLanguage.Russian:
                result = LANGUAGE_TYPE.RUSSIA;
                break;
            case SystemLanguage.Spanish:
                result = LANGUAGE_TYPE.SPAIN;
                break;
            case SystemLanguage.Thai:
                result = LANGUAGE_TYPE.THAILAND;
                break;
            case SystemLanguage.Turkish:
                result = LANGUAGE_TYPE.TURKEY;
                break;
            case SystemLanguage.Vietnamese:
                result = LANGUAGE_TYPE.VIETNAM;
                break;
            case SystemLanguage.Unknown:
                result = LANGUAGE_TYPE.ENGLISH;
                break;
            default:
                result = LANGUAGE_TYPE.ENGLISH;
                break;
        }

        return result;
    }

    public float GetValueInRatio(string option, float min, float max)
    {
        int oriValue = Get(option);
        oriValue = Math.Max(oriValue, 0);
        float gap = max - min;
        return (oriValue - min) / gap;
    }

    public static void ClearPrefKey()
    {
        PlayerPrefs.DeleteAll();
    }
}