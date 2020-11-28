using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Debug = EAFrameWork.Debug;


public class EAAssetBundleLoadModule : Singleton<EAAssetBundleLoadModule>
{
    public static EAAssetBundleInfoMgr m_AssetBundleMgr = new EAAssetBundleInfoMgr();
    
    public static AssetBundleMasterFileInfo m_AssetBundleMasterFileInfo = new AssetBundleMasterFileInfo();
    public static AssetBundleManifest m_AssetBundleManifest = null;
    public static Dictionary<string, AssetBundle> m_LoadAssetBundle = new Dictionary<string, AssetBundle>();

    // Bundle File Name , Whether it is an internal bundle
    public static Dictionary<string, bool> m_streamAseetBundle = new Dictionary<string, bool>(); // Use only for iOS builds

    public static string m_CurrentFile = string.Empty;
    public static float  m_CurrentProgress = 0.0f;
    public static uint m_nLoadedCount = 0;
    public static int  m_nTotalCount  = 0;
	bool m_useBundle = false;
     
    static string m_strResourcePath = @"Assets/AssetBundles/";
	//static string m_strLevelPath = @"Assets/Scenes/";

    public override GameObject GetSingletonParent()
    {
        return EAMainFrame.instance.gameObject;
    }

    protected override void Awake()
    {
        base.Awake();

        EAMainFrame.instance.OnMainFrameFacilityCreated(MainFrameAddFlags.AssetBundleLoadModule);
    }

    public void ResetContent()
	{
	}

	public bool GetUseBundle()
	{
		return m_useBundle;
	}

	public void UnLoadAll()
	{
		m_AssetBundleMgr.UnLoad();
	}

	List<AssetBundleInfo> _cachedBundles = new List<AssetBundleInfo>();
	List<AssetBundleResourceInfo> _cachedAssetObjs = new List<AssetBundleResourceInfo>();

	public void _RemoveCachedBundles()
	{
		foreach (AssetBundleResourceInfo obj in _cachedAssetObjs)
		{
			if (obj != null && obj.assetObj != null)
			{
                Debug.Log(" unload object : " + obj.GetResourceFullPath());
                obj.assetObj = null;
			}
		}

		foreach (AssetBundleInfo info in _cachedBundles)
		{
			if (info.m_bPreLoad == false && info.m_AssetBundle != null)
			{
                Debug.Log(" unload bundle : " + info.GetAssetBundleKey());
                info.m_AssetBundle.Unload(false);
			}  
		}
		_cachedBundles.Clear();
		_cachedAssetObjs.Clear();
	}

	public void OnSceneUnloaded()
	{
		_RemoveCachedBundles();
	}
	

	public void SetUseBundle(bool useBundle)
    {
        m_useBundle = useBundle;
    }

    public UnityEngine.Object Load(string name)
    {
        return Load<UnityEngine.Object>(name);
    }

	public static T LoadLocal<T>(string name) where T : UnityEngine.Object
    {
		string fullName = m_strResourcePath + name;

        T findObject = null;

     	DirectoryInfo path = new DirectoryInfo(Path.GetDirectoryName(fullName));

		if (Directory.Exists(path.FullName) == false)
			return findObject;

#if UNITY_EDITOR

	    string[] strFiles = getFindFileWildCard(fullName);

		string strTextExt = Path.GetExtension(fullName);

		string strFilePath = strFiles.Find(s => 
		{
			string sExt = Path.GetExtension(s);

			bool bForceExclude = (sExt.ToLower() == @".meta") || (sExt.ToLower() == @".anim");

			if (bForceExclude == true)
				return false; 

			if(typeof(T) == typeof(TextAsset))
			{
				if (strTextExt.ToLower() == @".bin" && sExt.ToLower() == @".bytes")  //.bin.bytes
					return true;
				else if (strTextExt.ToLower() == @"" && sExt.ToLower() == @".txt")   //.txt
					return true;
				else if (strTextExt.ToLower() == @"" && sExt.ToLower() == @".bytes" && s.Contains(@".bin") == false) //.bytes
					return true;
				else if (strTextExt.ToLower() == @"" && sExt.ToLower() == @".csv") //.csv
					return true;
				else
					return false; 
			}

			return true;

		});

		if (string.IsNullOrEmpty(strFilePath) == true)
		{
			strFilePath = fullName;
		}

		string strResourceName = Path.GetDirectoryName(fullName) + @"/" + Path.GetFileName(strFilePath);

		findObject = AssetDatabase.LoadAssetAtPath<T>(strResourceName);

#endif

		return findObject;

	}

	public static string[] getFindFileWildCard(string szPath)
	{
		string[] files;

        DirectoryInfo path = new DirectoryInfo(Path.GetDirectoryName(szPath));

		string szFileName = Path.GetFileName(szPath);

		// If there is no extension, add a wildcard.
		string szExt = Path.GetExtension(szFileName);

		if (szExt == null || szExt == @"" || szExt == @".bin")
		{
			szFileName += @".*";
		}

		if (Directory.Exists(path.FullName))
		{
			files = Directory.GetFiles(path.FullName, szFileName, SearchOption.TopDirectoryOnly);
			return files;
		}
		else
		{
			return null;
		}
	}

	public T Load<T>(string name) where T : UnityEngine.Object
    {
        if(m_AssetBundleManifest == null)
        {
            AssetBundleMasterFile manifestfile = m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[@"assets/assetbundlemanifest"];
            AssetBundle bundle = AssetBundle.LoadFromFile(manifestfile.GetLoadBundleFilePath());
            m_AssetBundleManifest = bundle.LoadAsset(manifestfile.m_bundleName) as AssetBundleManifest;
        }
        
        Debug.Log("Bundle Load Name: " + name);

        string fullName = m_strResourcePath + name;

		string strLowerPathName = fullName.ToLower();

        string assetbundleKey = strLowerPathName.Substring(0, fullName.LastIndexOf('/'));
        string bundleName = strLowerPathName.Replace(assetbundleKey + @"/", @"");

        AssetBundleMasterFile file = null;

        if (m_AssetBundleMasterFileInfo.assetBundleMasterFileDic.ContainsKey(strLowerPathName))
        {
            file = m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[strLowerPathName];
        }
        else if (m_AssetBundleMasterFileInfo.assetBundleMasterFileDic.ContainsKey(assetbundleKey))
        {
            file = m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[assetbundleKey];
        }
        else if (m_AssetBundleMasterFileInfo.assetBundleMasterFileDic.ContainsKey(string.Format(@"{0}/{1}", strLowerPathName, bundleName)))
        {
            file = m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[string.Format(@"{0}/{1}", strLowerPathName, bundleName)];
        }

        if (file == null)
        {
            return null;
        }

        //First check if it is in the loaded bundle list
        if (!m_LoadAssetBundle.ContainsKey(file.m_fileName))
        {

            //Load a bundle because it is not in the list of bundles
            AssetBundle bundle = AssetBundle.LoadFromFile(file.GetLoadBundleFilePath());

            if (bundle != null)
            {
                // Add to list
                m_LoadAssetBundle.Add(file.m_fileName.ToLower(), bundle);

                if (file.m_bundleName == @"DB")
                {
                    bundleName = bundleName + @".bytes";
                }

                string[] temp = m_AssetBundleManifest.GetAllDependencies(file.m_fileName.ToLower());

                //Load the relevant bundle.
                for (int i = 0; i < temp.Length; i++)
                {
                    DependenciesBundleLoad(temp[i]);
                }
                
                T obj = bundle.LoadAsset<T>(bundleName.ToLower());
    
                return obj;
            }
        }
        else
        {
            //Use a bundle already loaded
            if (m_LoadAssetBundle[file.m_fileName.ToLower()] != null)
            {
                if (file.m_bundleName == @"DB")
                {
                    bundleName = bundleName + @".bytes";
                }

                //디펜던시 확인
                string[] temp = m_AssetBundleManifest.GetAllDependencies(file.m_fileName.ToLower());

                //Load related bundles
                for (int i = 0; i < temp.Length; i++)
                {
                    DependenciesBundleLoad(temp[i]);
                }

                T obj = m_LoadAssetBundle[file.m_fileName.ToLower()].LoadAsset<T>(bundleName.ToLower());
                
                return obj;
            }
        }

		return null;
	}

    public void DependenciesBundleLoad(string AssetBundleKey)
    {
        //First check if it is in the loaded bundle list
        if (!m_LoadAssetBundle.ContainsKey(AssetBundleKey))
        {
            string path = string.Format(@"{0}Assets/{1}", AssetBundleConfig.m_LocalPath, AssetBundleKey);

            if(CoreApplication.IsMobileIPhone)
            {
                if (m_streamAseetBundle.ContainsKey(AssetBundleKey))
                {
                    if (m_streamAseetBundle[AssetBundleKey])
                    {
                        if (File.Exists(string.Format(@"{0}{1}", AssetBundleConfig.m_streamingAssetsPath, AssetBundleKey)))
                        {
                            path = string.Format(@"{0}{1}", AssetBundleConfig.m_streamingAssetsPath, AssetBundleKey);
                        }
                    }
                }
                else
                {
                    if (File.Exists(string.Format(@"{0}{1}", AssetBundleConfig.m_streamingAssetsPath, AssetBundleKey)))
                    {
                        path = string.Format(@"{0}{1}", AssetBundleConfig.m_streamingAssetsPath, AssetBundleKey);
                    }
                }
                
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(path);

            if (bundle != null)
            {
                // Add to list
                m_LoadAssetBundle.Add(AssetBundleKey, bundle);

                string[] temp = m_AssetBundleManifest.GetAllDependencies(AssetBundleKey);

                //Load related bundles
                for (int i = 0; i < temp.Length; i++)
                {
                    DependenciesBundleLoad(temp[i]);
                }
            }
        }
    }

}