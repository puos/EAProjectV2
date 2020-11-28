
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Debug = EAFrameWork.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif



public static class AssetBundleConfig
{
    public static string m_LocalPath = Application.persistentDataPath + @"/"; //번들파일을 받을 파일 패스
    public static string m_streamingAssetsPath = Application.streamingAssetsPath + @"/"; // StreamingAssets 패스 (ios 내부 번들 저장위치)

    public static string m_abExtName = @".ppab";
    public static string m_abExtZipName = @".ppzp";


    // 번들 다운로드 테스트 위치
    public static string m_uploadRootUrl;
    public static string m_downloadRootUrl;

    public static  string passward = "$%^@";

    public static string m_projectPath = Application.dataPath + @"/";

    public static string m_UploadID;
    public static string m_UploadPW;

	public static string m_bundle_outputPath = @"PatchData";

	static AssetBundleMasterFileInfo _masterFileInfo = null;

    static AssetBundleConfig()
    {
        m_UploadID = @"GamePatcher";
        m_UploadPW = @"1234";

        // 번들 다운로드 테스트 위치
        m_uploadRootUrl = @"ftp://192.168.0.121/WorksDrive/PinpongAssetBundle/";
        //m_downloadRootUrl = @"http://localhost/Bundle/";  
        m_downloadRootUrl = @"http://pingpong-game-storage.kr.object.ncloudstorage.com/Dev/";

        //안드로이드 일떄 패스를 바꿔준다
        if (CoreApplication.IsMobileAndroid)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using(AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaObject fileObject = currentActivity.Call<AndroidJavaObject>("getExternalFilesDir", (object[])null))
                    {
                        if(fileObject != null)
                        {
                            m_LocalPath = fileObject.Call<string>("getPath") + @"/";
                        }
                    }
                } 
            } 
        }
        else if (CoreApplication.IsMobileIPhone)
        {
            m_LocalPath = Application.temporaryCachePath + @"/"; //번들파일을 받을 파일 패스
        }
    }

	public static AssetBundleMasterFileInfo GetMasterFileInfo()
	{
        if (_masterFileInfo == null || _masterFileInfo.assetBundleMasterFileDic.Count == 0)
		{
            //마스터 파일에 관한 정보가 없으면 로컬에 있는 마스터 파일을 불러온다.
            List<string> stringLineArray = FileManager.LoadTextLineList(AssetBundleConfig.m_LocalPath + @"BundleMasterFileInfo.bundlelist");

            _masterFileInfo = new AssetBundleMasterFileInfo();
                        
            if (stringLineArray == null || stringLineArray.Count <= 0)
            {
                //마스터파일 오류시에 삭제
                if(File.Exists(AssetBundleConfig.m_LocalPath + @"BundleMasterFileInfo.bundlelist"))
                {
                    File.Delete(AssetBundleConfig.m_LocalPath + @"BundleMasterFileInfo.bundlelist");
                }
            }
            else
            {
                //정상적으로 마스터 파일을 불러왔을경우

                //첫줄 번들버전정보
                _masterFileInfo.bundleVersion = stringLineArray[0];

                //마스터 파일 정보 로드 
                _masterFileInfo.assetBundleMasterFileDic = AssetBundleMasterFileInfo.LoadAssetBundleMasterFileDic(stringLineArray);
            }

            if (CoreApplication.IsMobileIPhone)
            {
                // IOS 경우에는 StreamAssets에 있는 마스터 파일 로드
                stringLineArray = FileManager.LoadTextLineList(AssetBundleConfig.m_streamingAssetsPath + @"BundleMasterFileInfo.bundlelist");

                if (_masterFileInfo.assetBundleMasterFileDic.Count > 0)
                {
                    //다운 받았던 번들정보가 있으므로 다운받은 마스터 버전 을 사용
                }
                else
                {
                    //다운받은 번들정가 없으므로 로컬에 있는 번들버전을 사용
                    _masterFileInfo.bundleVersion = stringLineArray[0];
                }

                Char[] delimiters = { '\t', '\t', '\t', '\t', '\t' };
                string key = string.Empty;
                for (int index = 1; index < stringLineArray.Count; index++)
                {
                    //순서 : 파일경로, 파일이름, CRC, 파일용량
                    string[] items = stringLineArray[index].Split(delimiters);

                    //키값을 생성한다 (소문자 경로)
                    key = string.Format(@"{0}/{1}", items[0], items[2]).ToLower();

                    if (_masterFileInfo.assetBundleMasterFileDic.ContainsKey(key))
                    {
                        if (items.Length == 5)
                        {
                            //이미 키가 있을 경우 비교를 한후 CRC가 다르면 m_streamingAssets = false 같으면 m_streamingAssets = true
                            if (_masterFileInfo.assetBundleMasterFileDic[key].m_crc == Int32.Parse(items[3]))
                            {
                                _masterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = true;

                                //IOS 경우 어떤 걸 사용하지 여부 판단
                                if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                {
                                    EAAssetBundleLoadModule.m_streamAseetBundle[_masterFileInfo.assetBundleMasterFileDic[key].m_fileName] = true;
                                }
                                else
                                {
                                    EAAssetBundleLoadModule.m_streamAseetBundle.Add(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName, true);
                                }
                            }
                            else
                            {
                                _masterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = false;

                                //IOS 경우 어떤 걸 사용하지 여부 판단
                                if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                {
                                    EAAssetBundleLoadModule.m_streamAseetBundle[_masterFileInfo.assetBundleMasterFileDic[key].m_fileName] = false;
                                }
                                else
                                {
                                    EAAssetBundleLoadModule.m_streamAseetBundle.Add(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName, false);
                                }
                            }
                        }
                        else if (items.Length == 6)
                        {
                            //내부 번들이 해쉬값을 가지고 있음으로 해쉬 비교를 해준다.
                            if ( !string.IsNullOrEmpty(items[5]) && 
                                 !string.IsNullOrEmpty(_masterFileInfo.assetBundleMasterFileDic[key].m_hash))
                            {
                                //둘다 해쉬값이 있으므로 해쉬비교 
                                //이미 키가 있을 경우 비교를 한후 해쉬값이 다르면 m_streamingAssets = false 같으면 m_streamingAssets = true
                                if ( _masterFileInfo.assetBundleMasterFileDic[key].m_hash.Equals(items[5]))
                                {
                                    _masterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = true;

                                    //IOS 경우 어떤 걸 사용하지 여부 판단
                                    if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle[_masterFileInfo.assetBundleMasterFileDic[key].m_fileName] = true;
                                    }
                                    else
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle.Add(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName, true);
                                    }
                                }
                                else
                                {
                                    _masterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = false;

                                    //IOS 경우 어떤 걸 사용하지 여부 판단
                                    if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle[_masterFileInfo.assetBundleMasterFileDic[key].m_fileName] = false;
                                    }
                                    else
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle.Add(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName, false);
                                    }
                                }
                            }
                            else
                            {
                                //이미 키가 있을 경우 비교를 한후 CRC가 다르면 m_streamingAssets = false 같으면 m_streamingAssets = true
                                if (_masterFileInfo.assetBundleMasterFileDic[key].m_crc == Int32.Parse(items[3]))
                                {
                                    _masterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = true;

                                    //IOS 경우 어떤 걸 사용하지 여부 판단
                                    if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle[_masterFileInfo.assetBundleMasterFileDic[key].m_fileName] = true;
                                    }
                                    else
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle.Add(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName, true);
                                    }
                                }
                                else
                                {
                                    _masterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = false;

                                    //IOS 경우 어떤 걸 사용하지 여부 판단
                                    if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle[_masterFileInfo.assetBundleMasterFileDic[key].m_fileName] = false;
                                    }
                                    else
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle.Add(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName, false);
                                    }
                                }
                            }
                            
                        }
                        
                    }
                    else
                    {
                        //키값이 없을 경우 그냥 추가
                        //내부의 것이니 m_streamingAssets 을 true로 한다.

                        if (items.Length == 5)
                        {
                            _masterFileInfo.assetBundleMasterFileDic.Add(key,
                                new AssetBundleMasterFile(items[0], items[1], items[2], Int32.Parse(items[3]), Int32.Parse(items[4]), string.Empty, true));                            
                        }
                        else if (items.Length == 6)
                        {
                            _masterFileInfo.assetBundleMasterFileDic.Add(key,
                                new AssetBundleMasterFile(items[0], items[1], items[2], Int32.Parse(items[3]), Int32.Parse(items[4]), items[5], true));
                        }

                        //IOS 경우 어떤 걸 사용하지 여부 판단
                        if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                        {
                            EAAssetBundleLoadModule.m_streamAseetBundle[_masterFileInfo.assetBundleMasterFileDic[key].m_fileName] = true;
                        }
                        else
                        {
                            EAAssetBundleLoadModule.m_streamAseetBundle.Add(_masterFileInfo.assetBundleMasterFileDic[key].m_fileName, true);
                        }
                    }
                }
            }
		}

		return _masterFileInfo;
	}

	public static string BUNDLE_VERSION
	{
		get
		{
			return GetMasterFileInfo().bundleVersion;
		}
	}
	public const string INITIAL_BUNDLE_VERSION = @"0";

}

public class AssetBundleMasterFile
{
    public string m_bundlePath; //번들 경로
    public string m_bundleName; // 번들파일이름
    public string m_fileName; // 번들파일의실제이름 
    public int m_crc; // CRC
    public int m_size; // 파일용량
    public string m_hash; //에셋파일해쉬
    public bool m_downloadCompleted = false; //다운로드 완료 상태
    public bool m_downloading = false; // 다운로드중인 상태
    public bool m_streamingAssets = false; // 아이폰에서 사용되는 로컬 번들 파일

    public AssetBundleMasterFile(string bundlePath, string filenName, string bundleName, int crc, int size, string hash, bool streamingAssets = false)
    {
        m_bundlePath = bundlePath;
        m_bundleName = bundleName;
        m_fileName = filenName;
        m_crc = crc;
        m_size = size;
        m_hash = hash;
        m_streamingAssets = streamingAssets;
    }

    public string GetAssetBundleMaterFileString()
    {
        return string.Format(@"{0}\t{1}\t{2}\t{3}\t{4}",this.m_bundlePath, this.m_fileName, this.m_bundleName, this.m_crc, this.m_size);
    }

    public string GetBundleLocalSaveFilePath()
    {
		return string.Format(@"{0}Assets/{1}", AssetBundleConfig.m_LocalPath, m_fileName);
    }
    public string GetLoadBundleFilePath()
    {

        if (CoreApplication.IsMobileIPhone)
        {
            if (m_streamingAssets)
            {
                return string.Format(@"{0}{1}", AssetBundleConfig.m_streamingAssetsPath, m_fileName);
            }
        }

        return string.Format(@"{0}Assets/{1}", AssetBundleConfig.m_LocalPath, m_fileName);
    }


    public string GetBundleLocalSaveRootDirectory()
    {
        return string.Format(@"{0}Assets", AssetBundleConfig.m_LocalPath);
    }

    public string GetBundleDownloadUrl()
    {
        return string.Format(@"{0}[{1}_{2}]/{3}", AssetBundleConfig.m_downloadRootUrl, EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.bundleVersion, CoreApplication.GetPlatformName(), m_fileName.Replace(AssetBundleConfig.m_abExtName, AssetBundleConfig.m_abExtZipName));
    }
}

public class AssetBundleMasterFileInfo
{
	public string bundleVersion = @"0";
    public Dictionary<string, AssetBundleMasterFile> assetBundleMasterFileDic = new Dictionary<string, AssetBundleMasterFile>();

	public AssetBundleMasterFileInfo()
	{

	}

	public AssetBundleMasterFileInfo(AssetBundleMasterFileInfo info)
	{
		bundleVersion = info.bundleVersion;
        assetBundleMasterFileDic = info.assetBundleMasterFileDic;
	}

    public static Dictionary<string, AssetBundleMasterFile>  LoadAssetBundleMasterFileDic(List<string> stringArray)
    {
        Dictionary<string, AssetBundleMasterFile> dic = new Dictionary<string,AssetBundleMasterFile>();

        if (stringArray == null || stringArray.Count <= 0)
        {
            //마스터파일이 불량일경우에는 지워준다.
            if (File.Exists(AssetBundleConfig.m_LocalPath + @"BundleMasterFileInfo.bundlelist"))
            {
                File.Delete(AssetBundleConfig.m_LocalPath + @"BundleMasterFileInfo.bundlelist");
            }
        }
        else
        {
            //정상적으로 마스터 파일을 불러왔을경우

            Char[] delimiters = { '\t', '\t', '\t', '\t', '\t' };
            string key = string.Empty;
            for (int index = 1; index < stringArray.Count; index++)
            {
                //순서 : 파일경로, 파일이름, CRC, 파일용량
                string[] items = stringArray[index].Split(delimiters);

                //11
                key = string.Format(@"{0}/{1}", items[0], items[2]).ToLower();

                if (items.Length == 5)
                {
                    dic.Add(key, new AssetBundleMasterFile(items[0], items[1], items[2], Int32.Parse(items[3]), Int32.Parse(items[4]), string.Empty));
                }
                else if (items.Length == 6)
                {
                    dic.Add(key, new AssetBundleMasterFile(items[0], items[1], items[2], Int32.Parse(items[3]), Int32.Parse(items[4]), items[5]));
                }
            }
        }

        return dic;
    }

}

[Serializable]
public class AssetBundleResourceInfo
{
    public string m_szResourceName = @"";

	public string m_szResourcePath = @"";

	public long m_lResourceSize = 0;

	public DateTime m_dateResource;

	public long m_lMetaResourceSize = 0;

	public DateTime m_dateMetaResource;

	public string m_AssetBundleKey = @"";

	public int m_nProperty = 0;

    [NonSerialized]
	public UnityEngine.Object assetObj = null;

    public void Clear()
    {
    
    }

	public void UnLoad()
	{
	
	}

	public int GetProperty()
	{
		return m_nProperty;
	}

    public static bool IsEqual(AssetBundleResourceInfo resourceInfo)
    {
		string szResource = resourceInfo.GetResourceFullPath();

        if (File.Exists(szResource) == false)
            return false;

        FileInfo fileInfo = new FileInfo(szResource);

        if (resourceInfo.m_lResourceSize != fileInfo.Length)
            return false;

        if (resourceInfo.m_dateResource != fileInfo.LastWriteTime)
            return false;

        if (File.Exists(szResource + @".meta") == true)
        {
            FileInfo metaFileInfo = new FileInfo(szResource + @".meta");

            if (resourceInfo.m_lMetaResourceSize != metaFileInfo.Length)
                return false;

            if (resourceInfo.m_dateMetaResource != metaFileInfo.LastWriteTime)
                return false;
        }

        return true;
    }

    public static string GetRemoveExtPath(string szResourceFullPath)
    {
        string ext = Path.GetExtension(szResourceFullPath);
        if (ext == null || ext.Length < 1)
            return szResourceFullPath;

        return szResourceFullPath.Remove(szResourceFullPath.LastIndexOf(ext));
    }

	public string GetResourceFullPath()
	{
		return m_szResourcePath + m_szResourceName;
	}
}

[Serializable]
public class AssetBundleInfo
{
	public int m_nPriority = 0;

	public string m_nVersion = @"0";

	public string m_szAssetBundleName = @"";

	public string m_szAssetBundlePath = @"";

	public string m_szAssetRealBundleName = @"";

	public long m_dateUpdate;

	public int m_size;

	public int m_crc;

    public Dictionary<string, AssetBundleResourceInfo> m_ResourceList = new Dictionary<string, AssetBundleResourceInfo>();

	public static string[] m_IgnoreExtension = { @".dll", @".zip" , @".ds_store"};
    [NonSerialized]    
    public AssetBundle m_AssetBundle = null;

	public bool m_bPreLoad = false; // 미리 로딩하는 경우는 번들을 반환하지 않는다.

	public bool m_downloadCompleted = false;
    public string FormatBundleName(bool bCompressed = false)
    {
		//--System.DateTime _DateUpdate = ClockManager.TimestampToLocalDateTime(m_dateUpdate);
		return FormatBundleName(CoreApplication.GetPlatformName(), bCompressed);
    }

	// 에셋 번들 저장 위치 (압축 여부)
	public string GetBundleLocalPath(bool bCompressed = false)
    {
        return GetBundleLocalPath(CoreApplication.GetPlatformName(), bCompressed);
    }

 
    public string GetBundleDownloadUrl()
    {
        return GetBundleDownloadUrl(CoreApplication.GetPlatformName(), true);
    }

	public string GetBundleMidPath(bool bVersionAdd = false)
	{
		if (bVersionAdd == true)
            return @"[" + Convert.ToString(m_nVersion) + @"_" + CoreApplication.GetPlatformName() + @"]"; /*+ m_szAssetBundlePath;*/
		else
		    return m_szAssetBundlePath;
	}

	public string FormatBundleName(string szPlatformName, bool bCompressed = false)
	{
		if (bCompressed == false)
		return m_szAssetBundleName + @"_" + szPlatformName + AssetBundleConfig.m_abExtName;
		else
		return m_szAssetBundleName + @"_" + szPlatformName + AssetBundleConfig.m_abExtZipName;
	}

	public string GetBundleLocalPath(string szPlatformName, bool bCompressed = false)
    {
		return AssetBundleConfig.m_LocalPath + GetBundleMidPath() + FormatBundleName(szPlatformName, bCompressed);
    }

	public string GetBundleBuildOutputPath(string serviceType, string szPlatformName, bool bCompressed = false)
	{
		return AssetBundleConfig.m_LocalPath + AssetBundleConfig.m_bundle_outputPath + @"/" + serviceType + FormatBundleName(szPlatformName, bCompressed).ToLower();
	}

    public string GetIOSLocalBundleBuildOutputPath(string serviceType, string szPlatformName, bool bCompressed = false)
    {
        return AssetBundleConfig.m_streamingAssetsPath + FormatBundleName(szPlatformName, bCompressed).ToLower();
    }

	public void GenerateBundleName(string szResourceFullPath, string szAssetBundlePath, int nIndividual)
	{
		
#if UNITY_EDITOR

		string sGUID = AssetDatabase.AssetPathToGUID(szAssetBundlePath);

		if (nIndividual == 1)
		{
			sGUID = AssetDatabase.AssetPathToGUID(szResourceFullPath);
		}

		m_szAssetBundleName = m_szAssetRealBundleName + @"_" + sGUID;

#endif
	}

	public string GetBundleUploadUrl(string szPlatformName, bool bCompressed = false)
    {
		return AssetBundleConfig.m_uploadRootUrl + GetBundleMidPath(true) + FormatBundleName(szPlatformName, bCompressed).ToLower();
    }

	public string GetBundleDownloadUrl(string szPlatformName, bool bCompressed = false)
    {
		string strDownloadPath = AssetBundleConfig.m_downloadRootUrl + GetBundleMidPath(true) + FormatBundleName(szPlatformName, bCompressed);
		strDownloadPath = strDownloadPath.Replace(@" ", @"%20"); // 공백 문자 처리 
		return strDownloadPath;
    }

	public string GetAssetBundleKey()
	{
		return GetBundleMidPath() + m_szAssetRealBundleName;
	}

	
    public void Clear()
    {
		 UnLoad();
		
		 m_ResourceList.Clear();
    }

	
	public void UnLoad()
	{
		foreach (var item in m_ResourceList)
		{
			item.Value.Clear();
		}
	}
	

    public static bool HasIgnoreExtension(string path)
    {
        string ext = Path.GetExtension(path);
        foreach (var item in m_IgnoreExtension)
        {
            if (item == ext)
                return true;
        }

        return false;
    }
}

[Serializable]
public class EAAssetBundleInfoMgr
{
    public string bundleVersion;
    
	public Dictionary<string, AssetBundleResourceInfo> m_ResourceList = new Dictionary<string, AssetBundleResourceInfo>(); // 번들로 묶이는 리소스들
	public Dictionary<string, AssetBundleInfo> m_AssetBundleList = new Dictionary<string, AssetBundleInfo>(); //에셋번들 목록
	public Dictionary<string, List<string>> m_Dependencies = new Dictionary<string, List<string>>(); // 종속성 목록

    public EAAssetBundleInfoMgr()
    {
        m_ResourceList = new Dictionary<string, AssetBundleResourceInfo>(); // 번들로 묶이는 리소스들
        m_AssetBundleList = new Dictionary<string, AssetBundleInfo>(); //에셋번들 목록
        m_Dependencies = new Dictionary<string, List<string>>(); // 종속성 목록
    }

	public static string FormatMasterFileName(string nVersion, string szDescName)
    {
		return @"ppablist_" + szDescName + @"_" + nVersion + @"_" + CoreApplication.GetPlatformName() + @".bundlelist";
    }

    public static string FormatBundleFileListName(string nVersion, string szDescName)
    {
        return @"ppablist_" + szDescName + @"_" + nVersion + @".ppablist";
    }

	public static string GetAssetBundleListRemoteUploadPath(string nVersion, string szDescName)
    {
		return AssetBundleConfig.m_uploadRootUrl + FormatMasterFileName(nVersion, szDescName);
    }

	public static string GetMasterFileRemoteURL(string nVersion, string szDescName)
    {
		return AssetBundleConfig.m_downloadRootUrl + FormatMasterFileName(nVersion, szDescName);
    }

	public static string GetMasterFilePathInProject(string nVersion, string szDescName)
    {
		return AssetBundleConfig.m_projectPath + FormatMasterFileName(nVersion, szDescName);
    }

    public static string GetBundleFileListPathInProject(string nVersion, string szDescName)
    {
        return AssetBundleConfig.m_projectPath + FormatBundleFileListName(nVersion, szDescName);
    }

	public static string GetMasterFileLocalPath(string nVersion, string szDescName)
	{
		return AssetBundleConfig.m_LocalPath + FormatMasterFileName(nVersion, szDescName);
	}

    public bool UpdateResource(string szResourceFullPath)
    {
        if (File.Exists(szResourceFullPath) == false)
            return false;

		string szlowerResourcePath = AssetBundleResourceInfo.GetRemoveExtPath(szResourceFullPath).ToLower();

		if (m_ResourceList.ContainsKey(szlowerResourcePath) == false)
            return false;

		AssetBundleResourceInfo resource = m_ResourceList[szlowerResourcePath];
		AssetBundleInfo bundleInfo = m_AssetBundleList[resource.m_AssetBundleKey];

		if (bundleInfo == null)
        {
			Debug.LogError("assetbundle info is null : @" + resource.m_AssetBundleKey);
        }

        FileInfo fileInfo = new FileInfo(szResourceFullPath);
        resource.m_dateResource = fileInfo.LastWriteTime;
        resource.m_lResourceSize = fileInfo.Length;

        FileInfo metaFileInfo = new FileInfo(szResourceFullPath + @".meta");
        resource.m_dateMetaResource = metaFileInfo.LastWriteTime;
        resource.m_lMetaResourceSize = metaFileInfo.Length;

        return true;
    }
	
	public bool AddResource(string szResourceFullPath, int nProperty = 0)
    {
        if (File.Exists(szResourceFullPath) == false)
            return false;

        if (AssetBundleInfo.HasIgnoreExtension(szResourceFullPath) == true)
            return false;

		string szlowerResourcePath = AssetBundleResourceInfo.GetRemoveExtPath(szResourceFullPath).ToLower();

		if (m_ResourceList.ContainsKey(szlowerResourcePath) == true)
            return false;

        AssetBundleResourceInfo resource = new AssetBundleResourceInfo();
        resource.m_szResourcePath = szResourceFullPath.Remove(szResourceFullPath.LastIndexOf(@"/"));
        resource.m_szResourceName = szResourceFullPath.Substring(szResourceFullPath.LastIndexOf(@"/"));
		        

        if(!CoreApplication.IsMobile)
        {
            FileInfo fileInfo = new FileInfo(szResourceFullPath);
            resource.m_dateResource = fileInfo.LastWriteTime;
            resource.m_lResourceSize = fileInfo.Length;

            FileInfo metaFileInfo = new FileInfo(szResourceFullPath + @".meta");
            resource.m_dateMetaResource = metaFileInfo.LastWriteTime;
            resource.m_lMetaResourceSize = metaFileInfo.Length;
        }
        

		string szAssetBundlePath = resource.m_szResourcePath.Remove(resource.m_szResourcePath.LastIndexOf(@"/"));
		string szAssetBundleName = resource.m_szResourcePath.Substring(resource.m_szResourcePath.LastIndexOf(@"/"));

        // 개별 빌드시에 오동작 체크 
        if (nProperty == 1)
        {
            szAssetBundlePath = AssetBundleResourceInfo.GetRemoveExtPath(szResourceFullPath);
            szAssetBundleName = szAssetBundlePath.Substring(szAssetBundlePath.LastIndexOf(@"/"));
        }

        string szAssetBundleFullPath = szAssetBundlePath + szAssetBundleName;

		resource.m_AssetBundleKey = szAssetBundleFullPath;
		resource.m_nProperty = nProperty;  // 특성을 저장

        AssetBundleInfo assetBundleInfo = null;
        
		if (m_AssetBundleList.ContainsKey(szAssetBundleFullPath) == true)
        {
            assetBundleInfo = m_AssetBundleList[szAssetBundleFullPath];
        }
        else
        {
            assetBundleInfo = new AssetBundleInfo();
			
			assetBundleInfo.m_nVersion = @"0";

			assetBundleInfo.m_szAssetBundlePath = szAssetBundlePath;

			assetBundleInfo.m_szAssetRealBundleName = szAssetBundleName;

			assetBundleInfo.GenerateBundleName(szResourceFullPath, szAssetBundleFullPath , nProperty);

			m_AssetBundleList.Add(szAssetBundleFullPath, assetBundleInfo);
        }

		assetBundleInfo.m_ResourceList.Add(szlowerResourcePath, resource);
       
		m_ResourceList.Add(szlowerResourcePath, resource);

		Debug.Log("Add Resources - @" + szlowerResourcePath);

		return true;
    }

	public bool AddDependenciesFromBundleName(string masterName, string[] bundleNames, string szOsType)
	{
		AssetBundleInfo master_info = GetAssetBundleInfoFromBundleName( masterName, szOsType);

		if (master_info != null)
		  ClearDependencies(master_info.GetAssetBundleKey());


        for(int i = 0; i < bundleNames.Length; ++i)
        {
            string s = bundleNames[i];

            AssetBundleInfo slave_info = GetAssetBundleInfoFromBundleName(s, szOsType);

            if (master_info != null && slave_info != null)
            {
                AddDependencies(master_info.GetAssetBundleKey(), slave_info.GetAssetBundleKey());
            }
        }  
				
		return true;
	}

	// 생성된 번들 이름으로 assetbundleinfo를 구한다. 
	public AssetBundleInfo GetAssetBundleInfoFromBundleName(string strBundleName, string szOsType)
	{
		foreach (var item in m_AssetBundleList)
		{
			string assetBundleName = item.Value.FormatBundleName(szOsType);
			assetBundleName = assetBundleName.Replace(@"/", @"");

			if (strBundleName.ToLower() == assetBundleName.ToLower())
			{
				return item.Value;
			}
		}

		return null;
	}

	public AssetBundleInfo GetAssetBundleInfo(string szAssetBundleKey)
	{
		if (m_AssetBundleList.ContainsKey(szAssetBundleKey) == true)
		{
			 return m_AssetBundleList[szAssetBundleKey];
		}

		return null;
	}
	
	// 종속성을 저장
	public bool AddDependencies(string szAssetBundleKey , string szDependentAssetBundleKey)
	{
		if (m_Dependencies.ContainsKey(szAssetBundleKey) == false)
		{
			m_Dependencies.Add(szAssetBundleKey, new List<string>());
		}

		List<string> listPath = m_Dependencies[szAssetBundleKey];

		listPath.Add(szDependentAssetBundleKey);

		List<string> reLoadlistPath = listPath.Distinct();

		m_Dependencies[szAssetBundleKey] = reLoadlistPath;

		return true;
	}

	public bool ClearDependencies(string szAssetBundleKey)
	{
		if (m_Dependencies.ContainsKey(szAssetBundleKey) == true)
		{
			List<string> listDependencies = m_Dependencies[szAssetBundleKey];

			if(listDependencies != null)
			{   //있으면 clear
				listDependencies.Clear();				
            } 
			else
			{
				//없으면 remove 한다
				m_Dependencies.Remove(szAssetBundleKey);
			}  
		}

		return true;
	}

    public AssetBundleResourceInfo GetResource(string szResourceFullPath)
    {
		string szlowerResourcePath = AssetBundleResourceInfo.GetRemoveExtPath(szResourceFullPath).ToLower();

		if (m_ResourceList.ContainsKey(szlowerResourcePath) == false)
            return null;

		return m_ResourceList[szlowerResourcePath];
    }
	
    public bool RemoveResource(string szResourceFullPath)
    {
		string szlowerResourcePath = AssetBundleResourceInfo.GetRemoveExtPath(szResourceFullPath).ToLower();

		if (m_ResourceList.ContainsKey(szlowerResourcePath) == false)
            return false;

		AssetBundleResourceInfo resource            = m_ResourceList[szlowerResourcePath];
		AssetBundleInfo assetbundleinfo             = m_AssetBundleList[resource.m_AssetBundleKey];

		if (assetbundleinfo != null)
        {
			assetbundleinfo.m_ResourceList.Remove(szlowerResourcePath);
		
			if (assetbundleinfo.m_ResourceList.Count == 0)
            {
				m_AssetBundleList.Remove(assetbundleinfo.GetBundleMidPath() + assetbundleinfo.m_szAssetRealBundleName);
            }
        }
        else
        {
            Debug.LogError("assetbundle info is null");
        }

        resource.Clear();
		m_ResourceList.Remove(szlowerResourcePath);
        
        return true;
    }

	// 강제로 전체 빌드를 시킨다. (종속성 문제로 인해서)
    public List<AssetBundleInfo> GetRebuildList()
    {
        List<AssetBundleInfo> rebuildList = new List<AssetBundleInfo>();

        foreach (var item in m_AssetBundleList)
        {
			rebuildList.Add(item.Value);
        }

        return rebuildList;
    }

	public void RefreshEachBundlesVersionAndDate(List<AssetBundleInfo> rebuildedList, string serviceType, string szOsType, string nBundleVersion)
	{
		foreach (var item in rebuildedList)
		{
			FileInfo fileInfo = new FileInfo(item.GetBundleBuildOutputPath(serviceType, szOsType));

			if (fileInfo != null)
			{

                long _dateUpdateValue = TimeUtil.LocalDateTimeToTimestamp(fileInfo.LastWriteTime); item.m_size = (int)fileInfo.Length;

				using (FileStream stream = fileInfo.OpenRead())
				{
					using (Ionic.Crc.CrcCalculatorStream crcStream = new Ionic.Crc.CrcCalculatorStream(stream))
					{
						byte[] buffer = new byte[4096];
						while (crcStream.Read(buffer, 0, buffer.Length) > 0)
						{ }
						item.m_crc = crcStream.Crc;
					}
				}
				item.m_dateUpdate = _dateUpdateValue;
				item.m_nVersion = nBundleVersion;
			}
		}
	}

    public void Clear()
    {
		foreach (var item in m_ResourceList)
		{
			item.Value.Clear();
		}

		foreach (var item in m_AssetBundleList)
		{
			item.Value.Clear();
		}

       	m_ResourceList.Clear();
	}

	public void UnLoad()
	{
		foreach (var item in m_ResourceList)
		{
			item.Value.UnLoad();
		}

		foreach (var item in m_AssetBundleList)
		{
			item.Value.UnLoad();
		}
	}

}
