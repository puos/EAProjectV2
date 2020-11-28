using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Debug = EAFrameWork.Debug;

[System.Serializable]
public class DownLoadAction
{
    public uint idx;
    public UnityWebRequest webRequest = null;
    public Func<UnityWebRequest, bool> action = null;


    public DownLoadAction(UnityWebRequest webRequest, Func<UnityWebRequest, bool> action)
    {
        this.webRequest = webRequest;
        this.action     = action;
    }

    public DownLoadAction(uint idx)
    {
        this.idx = idx;
    }

    public void SetDownLoadAction(UnityWebRequest webRequest, Func<UnityWebRequest, bool> action)
    {
        this.webRequest = webRequest;
        this.action     = action;
    }
}


public class EAAssetBundleDownloadModule : MonoBehaviour
{
	public enum State
	{
		None,
		DownloadMasterFile,  // Downloading master file
        CalcDownloadList,    // Calculating list of files to download
        ShowConfirmMsg,      // Would you like to download it? The confirmation window is floating.
        DownloadEachFile,    // 번들파일들 다운로드 중.
		End,                 // 종료됨
		ConfirmRetry,
		WaitRetry,
	}

	State _state;
	State _stateToRetry;

	// 연속으로 발생한 네트웍 에러 카운터
	int _continuousNetworkErrorCnt = 0;

    enum eAssetBundleType
    {
        eAssetBundleNextScene = 0,
        eAssetBundleFunc      = 1,
    }
    	
	// 다운로드 받으려는 목록
    List<AssetBundleMasterFile> _downloadList = new List<AssetBundleMasterFile>();

	// 다운받으려는 총용량(압축용량이 아니고 순수한 BundleData크기)
	long _downloadSize = 0;

    // 동시에 다운받을려는 갯수
    int _maxDownloadFileCnt = 2;
    int _curCoroutineCnt = 0;

	public static string DOWNLOADING_BUNDLE_VERSION = @"0";

    string _currentFile = string.Empty;
    float _currentProgress = 0f; // Progress per file.
    int _savedBundleFileCnt = 0;
	string _errorDesc = null;
	float _retryWaitTimer = 0;
    byte[] _downloadMaterfileByteArray = null;
    
    public delegate void OnDownLoadComplete(string error);
    protected OnDownLoadComplete _onDownloadComplete = null;

    public delegate void OnDownLoadUpdate(string file_name, int totalCount, int downloadCount);
    protected OnDownLoadUpdate _onDownloadUpdate = null;

    private List<DownLoadAction> actionList = new List<DownLoadAction>();
       
    public void ManifestLoadTest()
    {

    }

    public void StartDownLoad(OnDownLoadComplete onDownloadComplete , OnDownLoadUpdate onDownloadUpdate = null)
    {
        _onDownloadUpdate   = onDownloadUpdate; 
		_onDownloadComplete = onDownloadComplete;

		_ChangeState(State.DownloadMasterFile);
    }

    /// <summary>
    /// Check the list of bundles recorded on the master to find out which files need patches. 
    /// </summary>
    IEnumerator _BuildDownloadList()
	{
		_downloadList.Clear();
		int counter = 0;
		float prevTime = Time.realtimeSinceStartup;
		_downloadSize = 0;

		string retError;
        AssetBundleMasterFileInfo compareMaterFileInfo = AssetBundleConfig.GetMasterFileInfo(); // Android is used as is

        if (CoreApplication.IsMobileIPhone)
        {
            //IOS uses internal master files.
            List<string> stringLineArray = FileManager.LoadTextLineList(AssetBundleConfig.m_streamingAssetsPath + @"BundleMasterFileInfo.bundlelist");
            compareMaterFileInfo.assetBundleMasterFileDic = AssetBundleMasterFileInfo.LoadAssetBundleMasterFileDic(stringLineArray);
        }

		foreach (string key in EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic.Keys)
		{
			Debug.Log("Downloader._BuildDownloadList() check.. " + counter);
			counter++;

            if (compareMaterFileInfo.assetBundleMasterFileDic.Count <= 0 )
            {
                // Check everything because there is no master file
                if (!_CheckBundleFileConsistency(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key], true, out retError))
                {
                    Debug.Log("Downloader._BuildDownloadList() - AddFile: " + EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].GetBundleLocalSaveFilePath());
                    _downloadList.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key]);
                    _downloadSize += EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_size;
                }
            }
            else
            {
                //Since there is a master file, the standard file is checked.
                if (compareMaterFileInfo.assetBundleMasterFileDic.ContainsKey(key))
                {
                    //Judge whether there is hash information in the saved and received master file
                    if ( !string.IsNullOrEmpty(compareMaterFileInfo.assetBundleMasterFileDic[key].m_hash) && 
                         !string.IsNullOrEmpty(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_hash))
                    {
                        //Both compare to the hash if there is one.
                        if (compareMaterFileInfo.assetBundleMasterFileDic[key].m_hash.Equals(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_hash))
                        {
                            //Pass if the hashes are equal

                            if(CoreApplication.IsMobileIPhone)
                            {
                                string path = string.Format("{0}{1}", AssetBundleConfig.m_LocalPath, compareMaterFileInfo.assetBundleMasterFileDic[key].m_fileName);

                                // If you have an iPhone, check the file and remove it if it exists
                                if(File.Exists(path))
                                {
                                    File.Delete(path);
                                }
                            }
                        }
                        else
                        {
                            //If the hash is different, the file is compared to the hash value and added to the download list
                            if (!_CheckBundleFileConsistency(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key], true, out retError))
                            {
                                Debug.Log("Downloader._BuildDownloadList() - AddFile: " + EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].GetBundleLocalSaveFilePath());
                                _downloadList.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key]);
                                _downloadSize += EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_size;
                            }
                        }
                    }
                    else
                    {
                        //If a file has no hash information, it is compared with crc.
                        if (compareMaterFileInfo.assetBundleMasterFileDic[key].m_crc == EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_crc)
                        {
                            //Pass if CRC values are equal
                            if (CoreApplication.IsMobileIPhone)
                            {
                                string path = string.Format("{0}{1}", AssetBundleConfig.m_LocalPath, compareMaterFileInfo.assetBundleMasterFileDic[key].m_fileName);

                                //If you have an iPhone, check the file and remove it if it exists.
                                if (File.Exists(path))
                                {
                                    File.Delete(path);
                                }
                            }
                        }
                        else
                        {
                            //If the CRC values ​​are different, add the download list after comparing the file with the CRC.
                            if (!_CheckBundleFileConsistency(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key], true, out retError))
                            {
                                Debug.Log("Downloader._BuildDownloadList() - AddFile: " + EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].GetBundleLocalSaveFilePath());
                                _downloadList.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key]);
                                _downloadSize += EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_size;
                            }
                        }
                    }
                    
                }
                else
                {
                    //If the master file is not in the list, add it unconditionally.
                    _downloadList.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key]);
                    _downloadSize += EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_size;
                }
            }

			
			if (Time.realtimeSinceStartup - prevTime > 0.05f)
			{
				prevTime = Time.realtimeSinceStartup;
				yield return null;
			}
		}

		if (_downloadList.Count == 0)
			_ChangeState(State.End);
		else
			_ChangeState(State.ShowConfirmMsg);
	}

    private void OnComplete(string error)
    {
        _stateToRetry = State.None;
        _retryWaitTimer = 0;

        if (error==null)
		{
            //다운받은 파일에대한 정보를 저장한다.
            SaveMasterFile(_downloadMaterfileByteArray);

            //IOS경우 로컬에 있는 것과 비교해서 로컬에서 불러올것과 아닌것을 구분한다.
            if (CoreApplication.IsMobileIPhone)
            {
                // IOS 경우에는 StreamAssets에 있는 마스터 파일을 불러온다.
                List<string> stringLineArray = FileManager.LoadTextLineList(AssetBundleConfig.m_streamingAssetsPath + @"BundleMasterFileInfo.bundlelist");

                Char[] delimiters = { '\t', '\t', '\t', '\t', '\t' };
                string key = string.Empty;
                for (int index = 1; index < stringLineArray.Count; index++)
                {
                    //순서 : 파일경로, 파일이름, CRC, 파일용량, 해쉬값
                    string[] items = stringLineArray[index].Split(delimiters);

                    //키값을 생성한다 (소문자 경로)
                    key = string.Format(@"{0}/{1}", items[0], items[2]).ToLower();

                    if (EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic.ContainsKey(key))
                    {
                        //해쉬비교를 먼저한다.

                        if (items.Length == 5)
                        {
                            //내부번들이 해쉬코드를 안가지고 있음으로 
                            //이미 키가 있을 경우 비교를 한후 CRC가 다르면 m_streamingAssets = false 같으면 m_streamingAssets = true
                            if (EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_crc == Int32.Parse(items[3]))
                            {
                                EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = true;

                                //IOS 경우 어떤 걸 사용하지 여부 판단
                                if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                {
                                    EAAssetBundleLoadModule.m_streamAseetBundle[EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName] = true;
                                }
                                else
                                {
                                    EAAssetBundleLoadModule.m_streamAseetBundle.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName, true);
                                }

                            }
                            else
                            {
                                EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = false;

                                //IOS 경우 어떤 걸 사용하지 여부 판단
                                if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                {
                                    EAAssetBundleLoadModule.m_streamAseetBundle[EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName] = false;
                                }
                                else
                                {
                                    EAAssetBundleLoadModule.m_streamAseetBundle.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName, false);
                                }
                            }
                        }
                        else if(items.Length == 6)
                        {
                            //내부 번들이 해쉬값을 가지고 있음으로 해쉬 비교를 해준다.
                            if ( !String.IsNullOrEmpty(items[5]) && 
                                 !String.IsNullOrEmpty(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_hash))
                            {
                                //둘다 해쉬값이 있으므로 해쉬비교 
                                if (EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_hash.Equals(items[5]))
                                {
                                    EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = true;

                                    //IOS 경우 어떤 걸 사용하지 여부 판단
                                    if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle[EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName] = true;
                                    }
                                    else
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName, true);
                                    }

                                }
                                else
                                {
                                    EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = false;

                                    //IOS 경우 어떤 걸 사용하지 여부 판단
                                    if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle[EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName] = false;
                                    }
                                    else
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName, false);
                                    }
                                }
                            }
                            else
                            {
                                //한개라도 해쉬값을 가진게 없음으로 CRC 비교
                                if (EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_crc == Int32.Parse(items[3]))
                                {
                                    EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = true;

                                    //IOS 경우 어떤 걸 사용하지 여부 판단
                                    if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle[EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName] = true;
                                    }
                                    else
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName, true);
                                    }

                                }
                                else
                                {
                                    EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_streamingAssets = false;

                                    //IOS 경우 어떤 걸 사용하지 여부 판단
                                    if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle[EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName] = false;
                                    }
                                    else
                                    {
                                        EAAssetBundleLoadModule.m_streamAseetBundle.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName, false);
                                    }
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        //키값이 없을 경우 그냥 추가한다.
                        //내부의 것이니 m_streamingAssets 을 true로 한다.
                        if (items.Length == 5)
                        {
                            EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic.Add(key,
                                new AssetBundleMasterFile(items[0], items[1], items[2], Int32.Parse(items[3]), Int32.Parse(items[4]), items[5], true));
                        }
                        else if (items.Length == 6)
                        {
                            EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic.Add(key,
                                new AssetBundleMasterFile(items[0], items[1], items[2], Int32.Parse(items[3]), Int32.Parse(items[4]), string.Empty, true));
                        }

                        //IOS 경우 어떤 걸 사용하지 여부 판단
                        if (EAAssetBundleLoadModule.m_streamAseetBundle.ContainsKey(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName))
                        {
                            EAAssetBundleLoadModule.m_streamAseetBundle[EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName] = true;
                        }
                        else
                        {
                            EAAssetBundleLoadModule.m_streamAseetBundle.Add(EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic[key].m_fileName, true);
                        }
                    }
                }
            }

			_onDownloadComplete(null);
		}
		else
		{
			_onDownloadComplete(error);
		}
    }

	static void _SaveBundleFile(AssetBundleMasterFile info, byte[] bytes)
	{
        Debug.Log("SaveBundleFile : " + info.GetBundleLocalSaveFilePath());

        if (!Directory.Exists(AssetBundleConfig.m_LocalPath))
			Directory.CreateDirectory(AssetBundleConfig.m_LocalPath);

        //if (File.Exists(info.GetBundleLocalSaveFilePath()))
        //{
        //    File.Delete(info.GetBundleLocalSaveFilePath());
        //}

        using (MemoryStream inStream = new MemoryStream(bytes, 0, bytes.Length))
		{
			inStream.Seek(0, SeekOrigin.Begin);
			using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(inStream))
			{
                zip.Password = AssetBundleConfig.passward;
                zip.ExtractExistingFile = Ionic.Zip.ExtractExistingFileAction.OverwriteSilently;
                zip.ExtractAll(AssetBundleConfig.m_LocalPath + @"Assets", Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
			}
		}
               
		string retError;

        if (!_CheckBundleFileConsistency(info, false, out retError))
        {
            string msg = "patch file save error :" + @"(" + retError + @").";

            throw new Exception(msg);
        }
			

		info.m_downloadCompleted = true;
        Debug.Log("파일 저장됨 SaveFile : " + info.GetBundleLocalSaveFilePath());

	}

    static bool _CheckBundleFileConsistency(AssetBundleMasterFile info, bool calcBundleCRC, out string retError)
	{
		retError = null;

        string path = string.Format(@"{0}/{1}", AssetBundleConfig.m_LocalPath + @"Assets", info.m_fileName);

        if (!File.Exists(path))
        {
            retError = Translate("patch file nothing");
            return false;
        }
        else
        {
            if(calcBundleCRC)
            {
                FileInfo fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                {
                    retError = Translate("patch file nothing");
                    return false;
                }

                using (FileStream inStream = fileInfo.OpenRead())
                {
                    byte[] bytes = new byte[fileInfo.Length];

					int currRead;
					int totalRead = 0;
					while ((currRead = inStream.Read(bytes, totalRead, (int)fileInfo.Length - totalRead)) > 0)
						totalRead += currRead;

                    if (info.m_size != fileInfo.Length)
                    {
                        retError = Translate("patch file different size");
                        return false;
                    }

                    if (calcBundleCRC == true && string.IsNullOrEmpty(info.m_hash) == true)
                    {
                        // 해쉬값이 없을 경우에만 예전처럼 CRC 값을 비교해서 사용한다.
                        int crcCompute = CRC32.GetHash(bytes);
                        if (info.m_crc != crcCompute)
                        {
                            retError = Translate("patch file hash different");
                            return false;
                        }
                    }
                }
            }
        }

		return true;
	}

    void Update()
    {
		if (_retryWaitTimer > 0)
		{
			_retryWaitTimer = Mathf.Max(0, _retryWaitTimer - Time.unscaledDeltaTime);
			if (_retryWaitTimer == 0)
			{
				Debug.Log("ChangeState by RetryWaitTimer :" + _stateToRetry);
                
                _curCoroutineCnt -= 1;

                if (_curCoroutineCnt < 0)
                {
                    _curCoroutineCnt = 0;
                }
				_ChangeState(_stateToRetry);
			}
		}

        lock(actionList)
        {
            List<DownLoadAction> select_actions = new List<DownLoadAction>();

            for (int i = 0;i < actionList.Count; ++i)
            {
                if(actionList[i].action != null)
                {
                    if (actionList[i].action(actionList[i].webRequest))
                    {
                        actionList[i].webRequest = null;
                        actionList[i].action     = null;

                        select_actions.Add(actionList[i]);
                        //pool.FreeDownLoadAction(actionList[i]);
                    }
                }
            }

            if(select_actions.Count > 0)
            {
                actionList.RemoveAll(x => select_actions.Contains(x));
            } 
        }   
    }

    public static void SaveMasterFile(byte[] writeData)
	{
        using (FileStream writeFile = new FileStream(AssetBundleConfig.m_LocalPath + @"BundleMasterFileInfo.bundlelist", FileMode.Create))
        {
            print("SaveMasterFile - " + ((writeData == null) ? -1 : writeData.Length));

            if(writeData != null)
            {
                writeFile.Write(writeData, 0, writeData.Length);
                writeFile.Close();
            }     
        }
	}

	public static string GetTrustedLocalBundleVersion()
	{
		string bundleVersion = AssetBundleConfig.GetMasterFileInfo().bundleVersion;

        return bundleVersion;
	}

	void _ChangeState(State newState)
	{
		if (_state == newState)
		{
			Debug.Log("same State:" + newState);
			return;
		}
		Debug.Log("Change State:" + newState);

        // DownloadFiles만 같은 State로 전환가능. 나머진 불가.
        if (_state != State.DownloadEachFile && _state == newState)
        {
            Debug.Assert(false, "Invalid State Transition: " + _state + "-->" + newState);
            return;
        }

        _state = newState;


        switch (_state)
        {
            case State.DownloadMasterFile:
                _StartDownloadMasterFile();
                break;
            case State.CalcDownloadList:
                StartCoroutine(_BuildDownloadList());
                break;

            case State.ShowConfirmMsg:
                _ShowDownloadConfirmMsg();
                break;

            case State.DownloadEachFile:
                _StartDownloadBundleFile();
                break;

            case State.End:
                OnComplete(_errorDesc);
                break;

            case State.ConfirmRetry:
                _ShowRetryConfirmMsg();
                break;

            case State.WaitRetry:
                Debug.Log("WaitRetry. ContinuousNetworkErrorCnt:" + _continuousNetworkErrorCnt);
                _retryWaitTimer = 1;
                break;
        }
	}

	void _ShowDownloadConfirmMsg()
	{
		// 서배국 : 기획요청에 의해 번들 다운로드 팝업창 제거
		_ChangeState(State.DownloadEachFile);

		//string msg = string.Empty;

		//if (PPApplication.IsMobileAndroid)
		//{
		//	long size = PPAndroidPlugin.Instance.getTotalExternalMemorySize();

		//	if (size <= _downloadSize + 50 * 1024 * 1024) // DownloadSize이외에 약간 더 확보한다(일단 50M정도)
		//	{
		//		msg = string.Format(Translate("Sys_Remain_Volume_error"), NumberWithByteSuffix(_downloadSize), NumberWithByteSuffix(size));

		//		Popup.OneButton one_Popup = new Popup.OneButton
		//		{
		//			Title = "UI_Noti",      //제목
		//			Message = msg,           //내용
		//			FirstBtn_Txt = "UI_Confirm",        //ok     버튼의 글자(확인)
		//		};

		//		//  [12/17/2018 puos] 게임 종료 버튼 추가 
		//		one_Popup.Popup(delegate (bool ok)
		//		{
		//			GameManager.instance.QuitApplication();
		//		});

		//		PopupManager.Instance.Push(one_Popup);
		//	}
		//	else
		//	{

		//	}
		//}


		//int compressedDownloadSize = (int)(_downloadSize * 0.486f); // 실측한 압축률
		//msg = string.Format(Translate("Sys_popup_Bundle_verCheck"), NumberWithByteSuffix(compressedDownloadSize));

		//Popup.OneButton _popup = new Popup.OneButton
		//{
		//	Title = "UI_Noti",//"추가게임데이터_다운로드_경고",  
		//	Message = msg,
		//	FirstBtn_Txt = "UI_Confirm",
		//};

		////  [12/17/2018 puos] 게임 종료 버튼 추가 
		//_popup.Popup(delegate (bool ok)
		//{
		//	_ChangeState(State.DownloadEachFile);
		//});

		//PopupManager.Instance.Push(_popup);
	}

	void _ShowRetryConfirmMsg()
	{
		string errorMsg = Translate("EC_NOT_CONNECT_1000") + System.Environment.NewLine + _errorDesc;

        Debug.Log(errorMsg);

  //      Popup.OneButton _popup = new Popup.OneButton
		//{
  //          Title   = "UI_Noti",
  //          Message = errorMsg,
  //          FirstBtn_Txt  = "UI_Confirm",
  //      };

        //  [12/17/2018 puos] 게임 종료 버튼 추가 
        //_popup.Popup(delegate (bool ok)
        {
            _errorDesc = null;
            _continuousNetworkErrorCnt = 0;

            if(_downloadMaterfileByteArray == null)
            {
                _ChangeState(State.DownloadMasterFile);
            }
            else
            {
                _ChangeState(State.DownloadEachFile);
            } 
		}
        //);

        //PopupManager.Instance.Push(_popup);
    }

	int _OnNetworkError(string desc)
	{
		_continuousNetworkErrorCnt++;
		Debug.Log("_OnNetworkError : " + desc + " _continuousNetworkErrorCnt: " + _continuousNetworkErrorCnt);
		_errorDesc = desc;
				
		if (_continuousNetworkErrorCnt == 1)
		{
			// 연속 1회는 자동 재시도
			_errorDesc = null;
			_stateToRetry = _state;
			_ChangeState(State.WaitRetry);
            return 1;
		}
		else if (_continuousNetworkErrorCnt >= 2)
		{
            if(_continuousNetworkErrorCnt == 2)
            {
                _stateToRetry = _state;
                // 연속 2회는 재시도 확인창
                _ChangeState(State.ConfirmRetry);
            }
			
            return 2;
		}

        return 0;
	}

	void _StartDownloadBundleFile()
	{
        ///쓰레드를 생성해서 여러번 돌리도록 한다.
        ///다운로드 중인 파일과 완료된 파일을 제외한것을 _maxDownloadFileCnt개의 쓰레드를 통해서 돌린다.

        if (_savedBundleFileCnt >= _downloadList.Count)
        {
            _ChangeState(State.End);
            return;
        }

        if (_curCoroutineCnt >= _maxDownloadFileCnt)
        {
            return;
        }

        for (int count = _curCoroutineCnt; count < _maxDownloadFileCnt; count++ )
        {
            for (int i = 0; i < _downloadList.Count; i++)
            {
                if (_downloadList[i].m_downloadCompleted == false && _downloadList[i].m_downloading == false)
                {
                    StartCoroutine(_DownLoadBundle(_downloadList[i], i));
                    break;
                }
            }
        }   
	}

    UnityWebRequest CreateHttp(Uri uri , Func<UnityWebRequest,bool> action)
    {
        lock(actionList)
        {
            UnityWebRequest webRequest = new UnityWebRequest(uri);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            DownLoadAction downloadAction = new DownLoadAction(webRequest,action);

            actionList.Add(downloadAction);

            return webRequest;
        }
    }


    IEnumerator _DownLoadBundle(AssetBundleMasterFile info, int index)
    {
        _curCoroutineCnt += 1;
        info.m_downloading = true;

        _currentProgress = 0;
        string url = info.GetBundleDownloadUrl();
        Debug.Log("Download : " + url);
        _currentFile = url;

        UnityWebRequest http = CreateHttp(new Uri(url), (UnityWebRequest webReq) =>
        {
            if (!string.IsNullOrEmpty(webReq.error))
            {
                _curCoroutineCnt -= 1;
                info.m_downloading = false;
                int result = _OnNetworkError(webReq.error);

                if(result == 2)
                {
                    webReq.Dispose();
                    return true;
                }          
                else
                {
                    return false;
                }
            }

            Debug.Log("Downloading : " + info.m_bundleName + " downloadProgress : " + webReq.downloadProgress);

            if (webReq.downloadHandler.isDone == false || webReq.downloadProgress < 1.0f)
            {
                return false;
            }

            _SaveBundleFile(info, webReq.downloadHandler.data);

            Debug.Log("m_AssetBundleMgr - currentFile : " + _currentFile + " count : " + _savedBundleFileCnt  + "/" + _downloadList.Count);


            if (_onDownloadUpdate != null)
            {
                _onDownloadUpdate(_currentFile, _downloadList.Count, _savedBundleFileCnt);
            }

            _currentProgress = 1;
            _continuousNetworkErrorCnt = 0;
            _errorDesc = null;
            _currentFile = string.Empty;

            _savedBundleFileCnt++;

            _curCoroutineCnt -= 1;

            if (_curCoroutineCnt < 0)
            {
                _curCoroutineCnt = 0;
            }

            if (_savedBundleFileCnt >= _downloadList.Count)
            {
                _ChangeState(State.End);
            }
            else
            {
                _StartDownloadBundleFile();
            }

            webReq.Dispose();

            return true;
        });

        http.SendWebRequest();

        Debug.Log("http sendWebRequest : " + http + " url : " + url);

        yield return null;
    }
       
	void _StartDownloadMasterFile()
	{
		_currentProgress = 0.0f;

        _downloadMaterfileByteArray = null;

        string masterFileUrl = EAAssetBundleInfoMgr.GetMasterFileRemoteURL(DOWNLOADING_BUNDLE_VERSION, string.Empty);
		Debug.Log("MasterFileUrl: " + masterFileUrl);

	    UnityWebRequest http = CreateHttp(new Uri(masterFileUrl), (UnityWebRequest webReq) =>
        {
			if (!string.IsNullOrEmpty(webReq.error))
            {
				_OnNetworkError(@"StatusCode:" + webReq.error);

                return false;
			}

            if (webReq.downloadHandler.isDone == false || webReq.downloadProgress < 1.0f)
            {
                return false;
            }

            Debug.Log("Finished!");

            _currentProgress = 1;
			_continuousNetworkErrorCnt = 0;
            _currentFile = string.Empty;

			try
			{
				using (MemoryStream deserialize = new MemoryStream(webReq.downloadHandler.data))
				{
                    EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo = new AssetBundleMasterFileInfo();
                    StreamReader reader = new StreamReader(deserialize);
                    List<string> lines = new List<string>();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }

                    EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.bundleVersion = lines[0];

                    EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic = AssetBundleMasterFileInfo.LoadAssetBundleMasterFileDic(lines);

                    _downloadMaterfileByteArray = webReq.downloadHandler.data;
				}
			}
			catch (Exception e)
			{
				// master 파일데이터 깨짐?
				_OnNetworkError(e.Message);
				return false;
			}

            webReq.Dispose();

			_ChangeState(State.CalcDownloadList);

            Debug.Log("m_AssetBundleMgr count:" + EAAssetBundleLoadModule.m_AssetBundleMasterFileInfo.assetBundleMasterFileDic.Count);

            return true;
		});

        http.SendWebRequest();
    }

	public State GetState() { return _state; }
	public string GetCurrentFile() { return _currentFile; }
	public float GetCurrentProgress() { return _currentProgress;  }
	public int GetSavedCount() { return _savedBundleFileCnt;  }

	public int GetDownloadListCount() { return _downloadList.Count; }

    static string Translate(string key)
    {
        string msg = "";

        try
        {
            msg = "";
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        return msg;
    }

    public static string NumberWithByteSuffix(long n)
    {
        if (n < 1024)
            return string.Format("{0}Bytes", n);
        else if (n < 1024 * 1024)
            return string.Format("{0}KB", n / 1024);
        else if (n < 1024 * 1024 * 1024)
            return string.Format("{0}MB", n / (1024 * 1024));

        return string.Format("{0}GB", n / (1024 * 1024 * 1024));
    }

}
