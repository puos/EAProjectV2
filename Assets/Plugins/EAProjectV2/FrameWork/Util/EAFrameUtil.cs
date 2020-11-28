using System;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;


public static class EAFrameUtil 
{
	static public void CopyTransform(Transform dst, Transform src)
	{
	   dst.localPosition = src.localPosition;
	   dst.localRotation = src.localRotation;
	   dst.localScale = src.localScale;
	}
	static public void ResetLocalTransform(GameObject go, bool resetScale = true)
	{
		ResetLocalTransform(go.transform, true);
	}
	static public void ResetLocalTransform(Transform t, bool resetScale = true)
	{
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		if(resetScale) t.localScale = Vector3.one;
	}

    /// <summary>
    /// Return the path of gameObject in scene in '/' delimited string format.
    /// If root is specified, the path from root is returned; if null, the path from the top gameObject is returned.
    /// </summary>
    static public string GetPathInScene(GameObject root, GameObject go, bool slashAtFirst = true)
	{
		if (go == null) return "";
		string path = go.name;
		Transform t = go.transform.parent;
		Transform tRoot = (root == null) ? null : root.transform;
		while (t != null)
		{
			path = t.gameObject.name + "/" + path;
			t = t.parent;
			if (t == tRoot)
				break;
		}
		if (slashAtFirst)
			return "/" + path;
		else
			return path;
	}
	static public string GetPathInScene(GameObject go, bool slashAtFirst = true)
	{
		return GetPathInScene(null, go, slashAtFirst);
	}

	static public string GetRelPathInScene(GameObject rootGo, GameObject go, bool includeRootGoName)
	{
		if (go == rootGo) return includeRootGoName ? rootGo.name : "";
		string path = go.name;
		Transform t = go.transform.parent;
		Transform tRoot = rootGo.transform;
		while (t != null && t != tRoot)
		{
			path = t.gameObject.name + "/" + path;
			t = t.parent;
			if (t == tRoot)
				break;
		}
		if (includeRootGoName)
			return rootGo.name + "/" + path;
		else
			return path;
	}
   	  
	static public string GetAssetPathFromFullPath(string fullPath)
	{
		int pos = fullPath.IndexOf("Assets/");
		if (pos == -1)
			pos = fullPath.IndexOf("Assets\\");
		if (pos == -1)
			return null;
		
		string assetPath = fullPath.Substring(pos);
		assetPath = assetPath.Replace('\\', '/');
		bool isDirectory = new DirectoryInfo(fullPath).Exists;
		if (isDirectory && assetPath.Length > 0 && assetPath[assetPath.Length - 1] != '/')
			assetPath += '/';
		return assetPath;
	}

	static public string GetLastSegmentFromPath(string path)
	{
		int pos = path.LastIndexOf('/');
		if(pos == -1)
			return path;
		if (pos == path.Length - 1) // Find again if the last character is '/'
        {
			int pos2 = path.LastIndexOf('/', pos - 1);
			if(pos2 == -1)
				return path.Substring(0, pos);
			else
				return path.Substring(pos2 + 1, pos - pos2 - 1);
		}
		return path.Substring(pos + 1);
	}
   
	public static bool HasBomUTF8(byte[] bytes)
	{
		if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
			return true;
		return false;
	}

    public static bool IsSameOrSubclass(System.Type baseType, System.Type testType)
	{
		return testType.IsSubclassOf(baseType)
			   || testType == baseType;
	}

    static public T LoadSettingsByName<T>(string path) where T : class
    {
       GameObject obj = ResourceManager.instance.Create(path) as GameObject;

       return obj == null ? null : obj.GetComponent<T>();
    }
  	
	public static string ToRGBHex(Color32 color)
	{
		return string.Format("{0:X2}{1:X2}{2:X2}", color.r, color.g, color.b);
	}

	public static GameObject AddChild(GameObject parent, string name = null)
	{
		GameObject go = AddChild(parent);
		          
		if (name != null)
			go.name = name;

		return go;
	}

    public static T AddChild<T>(GameObject parent, string name = null) where T : Component
	{
 		GameObject go = AddChild(parent, name);

       	return go.AddComponent<T>();
    }

    public static Component AddChild(GameObject parent, Type type , string name = null)
    {
        GameObject go = AddChild(parent, name);

        return go.AddComponent(type);
    }


    /// <summary>
    /// Add a child object to the specified parent and attaches the specified script to it.
    /// </summary>
    static public T AddChild<T>(GameObject parent) where T : Component
    {
        GameObject go = AddChild(parent);
        
        string s = typeof(T).ToString();
        if (s.StartsWith("UI")) s = s.Substring(2);
        else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
        go.name =  s;

        return go.AddComponent<T>();
    }

    static public GameObject AddChild(GameObject parent, GameObject prefab)
    {
        GameObject go = GameObject.Instantiate(prefab) as GameObject;

        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent.transform);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }

    static public GameObject AddChild(GameObject parent)
    {
         GameObject go = new GameObject();

         if (parent != null)
         {
             Transform t = go.transform;
             t.SetParent(parent.transform);
             t.localPosition = Vector3.zero;
             t.localRotation = Quaternion.identity;
             t.localScale = Vector3.one;
             go.layer = parent.layer;
         }
         return go;
     }

	public static Transform FindChildRecursively(Transform parent, string name)
	{
        if (CRC32.GetHashForAnsi(parent.name) == CRC32.GetHashForAnsi(name))
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform child = parent.GetChild(i);

            Transform result = FindChildRecursively(child, name);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

	public static T FindTypeInScene<T>(string name) where T : Component
	{
		UnityEngine.Object[] objs = GameObject.FindObjectsOfType(typeof(T));
		for (int i = 0; i < objs.Length; i++)
			if (objs[i].name.Equals(name))
				return objs[i] as T;
		return null;
	}

    /// <summary>
    /// Execute the action by traversing
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="action"></param>
    public static void DoRecursively(this Transform transform, Action<Transform> action)
    {
        action(transform);

        for (int i = 0; i < transform.childCount; i++)
        {
            DoRecursively(transform.GetChild(i), action);
        }
    }

    /// <summary>
    /// Execute the action by traversing
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="action"></param>
    public static void DoRecursively(this GameObject gameObject, Action<GameObject> action)
    {
        action(gameObject);

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            DoRecursively(gameObject.transform.GetChild(i).gameObject, action);
        }
    }

    public static bool IsAncestor(GameObject child, GameObject ancestorTest)
	{
		Transform t = child.transform.parent;
		while (t != null)
		{
			if (t.gameObject == ancestorTest)
				return true;
			t = t.parent;
		}
		return false;
	}
    
	public static void ChangeGameObjectLayersRecursively(Transform trans, int layer)
	 {
		 trans.gameObject.layer = layer;
		 for(int i = 0; i < trans.childCount; ++i)
		 {
            Transform child = trans.GetChild(i);
             child.gameObject.layer = layer;
			 ChangeGameObjectLayersRecursively(child, layer);
		 }
	 }


    /// <summary>
    /// Used when you want to call a certain property getter without using return value.
    /// 예) Bounds bounds = scrollView.bounds; // 'unused variable' Warning triggered.
    ///      EAFrameUtil.Call(scrollView.bounds);   // No warning is issued.
    /// </summary>
    public static T Call<T>(T t)
	{
		return t;
	}
    		
	public static void SplitLog(string msg, System.Action<string> logCb)
	{
        // If it's too long, it will be truncated in the Unity console so it will be split properly.
        const int MaxIter = 100;
		const int SegmentSize = 15000;
		if(msg.Length <= SegmentSize)
		{
			logCb(msg);
			return;
		}
		int prevPos = 0;
		int pos = 0;
		for (int i = 0; i < MaxIter && prevPos < msg.Length; i++)
		{
			pos += Mathf.Min(msg.Length - pos, SegmentSize);

            // If you break at the sequence character, you get an output error.
            while (pos < msg.Length && msg[pos] != ' ' && msg[pos] != '\"') 
               pos++;

			// print message
			logCb(msg.Substring(prevPos, pos - prevPos));
			prevPos = pos;
		}
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

	public static string Unescape(string s)
	{
		if (s == null)
			return string.Empty;
		return Regex.Unescape(s);
	}
	
	public static int CalcNameBytes(string s)
	{
		int bytes = 0;
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];

            // 3 bytes for Korean - Chinese characters, 2 bytes otherwise
            if (UnicodeRangeUtil.Contains(UnicodeRangeFilterType.KorJpnChi, c))
				bytes += 3;
			else
				bytes += 2;
		}
		return bytes;
	}

    /// <summary>
    /// set layer
    /// </summary>
    public static void setLayer(this GameObject self, int layer, bool includeChildren = true)
    {
        self.layer = layer;
        if (includeChildren)
        {
            var children = self.transform.GetComponentsInChildren<Transform>(true);
            for (var c = 0; c < children.Length; ++c)
            {
                children[c].gameObject.layer = layer;
            }
        }
    }

}

