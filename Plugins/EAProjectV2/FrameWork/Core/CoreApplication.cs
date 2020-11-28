using UnityEngine;
using Debug = EAFrameWork.Debug;

public static class CoreApplication
{
	public static bool IsAndroid { get; private set; }
    public static bool IsIPhone { get; private set; }
	public static bool IsMobileAndroid { get; private set; }
	public static bool IsMobileIPhone { get; private set; }
	public static bool IsMobile { get; private set; }
	
    public static int GetOsVersion()
    {
        string osVersion = SystemInfo.operatingSystem;
		Debug.Log("SystemInfo.operatingSystem:" + osVersion);
				
		if (CoreApplication.IsMobileIPhone == true) 
        {
            osVersion = osVersion.Replace("iPhone OS ", "");
            return Mathf.FloorToInt(float.Parse(osVersion.Substring(0,1)));
        } 

        return 0;
    }

    public static bool IsIPhoneOS9()
    {
        if (CoreApplication.GetOsVersion() >= 9 && CoreApplication.IsMobileIPhone)
        {
            return true;
        }

        return false;
    }

	
	///////////////////////////////////////////////////////////////////////////////////////////////
	// Static initialize
	static CoreApplication()
	{

#if UNITY_ANDROID
		IsAndroid = true;
#else
		IsAndroid = false;
#endif

#if UNITY_IPHONE
		IsIPhone = true;
#else
        IsIPhone = false;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
		IsMobileAndroid = true;
#else
		IsMobileAndroid = false;
#endif

#if UNITY_IPHONE && !UNITY_EDITOR
		IsMobileIPhone = true;
#else
		IsMobileIPhone = false;
#endif

#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        IsMobile = true;
#else
		IsMobile = false;
#endif
    }

    public static string GetPlatformName()
    {

        if (CoreApplication.IsAndroid)
        {
            return @"android";
        }

        if (CoreApplication.IsIPhone)
        {
            return @"ios";
        }

        return @"";
    }

    public static string GetOSStoreName()
    {
        string store = "G";

#if UNITY_ANDROID && !UNITY_EDITOR
            store = "G";
#elif UNITY_IPHONE && !UNITY_EDITOR
            store = "A";
#elif UNITY_EDITOR
        store = "W";
#endif
        return store;
    }

    public static LANGUAGE_TYPE language;
}
