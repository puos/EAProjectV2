using UnityEngine;


public class EAMainframeUtil
{
	/// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
	static public bool CreateMainFrameTree()
	{
		EAMainFrame mainFrame = EAMainFrame.instance;
             
		EAFrameUtil.Call<ClockManager>(ClockManager.instance);
        EAFrameUtil.Call<SceneLoadingManager>(SceneLoadingManager.instance);
        EAFrameUtil.Call<CObjResourcePoolingManager>(CObjResourcePoolingManager.instance);
        EAFrameUtil.Call<CEffectResourcePoolingManager>(CEffectResourcePoolingManager.instance);
        EAFrameUtil.Call<UIManager>(UIManager.instance);
        EAFrameUtil.Call<EAAssetBundleLoadModule>(EAAssetBundleLoadModule.instance);
        EAFrameUtil.Call<OptionManager>(OptionManager.instance);
        EAFrameUtil.Call<SoundManager>(SoundManager.instance);

        mainFrame.gameObject.AddComponent<AudioListener>();

        mainFrame.TryPostInit();

		return true;
	}

   
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefabName"></param>
    /// <param name="flag"></param>
    /// <param name="parentGo"></param>
    /// <param name="name"></param>
    /// <returns></returns>
	static T _AddMainFrameFacilityPrefab<T>(string prefabName, MainFrameAddFlags flag, GameObject parentGo = null, string name = null) where T : Component
	{
		//_mainFrame.FacilityFlags |= flag;
		var prefab = ResourceManager.instance.Create(prefabName);
		var go = GameObject.Instantiate(prefab) as GameObject;
        go.transform.parent = (parentGo == null) ?  EAMainFrame.instance.transform : parentGo.transform;
		go.transform.localPosition = new Vector3(0, 0, 0);
		go.transform.localRotation = Quaternion.identity;
		return go.GetComponent<T>();
	}
 }
