using UnityEngine;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

public class EAActor : MonoBehaviour
{
    public GameObject DummyWeaponSlot = null;
   
    EA_CCharBPlayer m_pDiaCharBase = null;
    protected EAWeapon m_pCurWeapon = null;

    public delegate void ActorInputEvent(params object[] command);
    public ActorInputEvent actorInputSendEvent;

    public Dictionary<int, Transform> transformList = new Dictionary<int, Transform>();
    public Renderer[] renderers = null;

    public string[] m_PartTblId = new string[(int)eCharParts.CP_MAX];

    protected virtual void OnCreate() { }

    protected virtual void OnInit() { }

    protected virtual void OnPosInit() { }

    protected virtual void OnUpdate() { }

    protected virtual void OnClose() { }

    protected virtual void OnDecay() { }

    private void Awake()
    {
        transformList.Clear();

        OnCreate();
    }

    private void OnDestroy()
    {
        OnDecay();
    }

    private void Start() { }


    /// <summary>
    /// update function
    /// </summary>
    private void Update()
    {
        OnUpdate();
    }

    public void SpawnAction()
    {
        OnInit();
    }

    public void PosInit()
    {
        OnPosInit();
    }
    
    public void DeSpawnAction()
    {
        OnClose();

        ReleaseParts();
    }

    /// <summary>
    /// Works after SetWeaponAttachment function
    /// </summary>
    /// <param name="weaponState"></param>
    public virtual void DoSwitchWeapons(uint weaponState)
    {
        RaiseWeapon();
    }

    public virtual void ShootAction()
    {
    }

    public virtual void UnShootAction()
    {
    }

    public virtual void OnAction(params object[] parms)
    {
    }

    /// <summary>
    /// Works before DoSwitchWeapon
    /// </summary>
    /// <param name="_WeaponType"></param>
    /// <param name="gameobject"></param>
    /// <returns></returns>
    public virtual bool SetWeaponAttachment(eWeaponType _WeaponType, GameObject gameobject)
    {
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="PartTblId"></param>
    public void FindParts(string[] PartTblId)
    {
        Transform mesh = GetTransform("mesh");

        for(int i = 0; i < PartTblId.Length; ++i)
        {
            if(m_PartTblId[i] != PartTblId[i])
            {
                ReleasePart(mesh, m_PartTblId[i]);

                ChangeParts(mesh, i, PartTblId[i]);

                m_PartTblId[i] = PartTblId[i];
            }
        }  
    }
       
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="idx"></param>
    /// <param name="parts"></param>
    public virtual void ChangeParts(Transform mesh, int idx , string parts)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    private void ReleaseParts()
    {
        Transform mesh = GetTransform("mesh");

        for (int i = 0; i < m_PartTblId.Length; ++i)
        {
            if (!string.IsNullOrEmpty(m_PartTblId[i]))
            {
                ReleasePart(mesh, m_PartTblId[i]);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="parts"></param>
    private void ReleasePart(Transform mesh,string parts)
    {
        Transform mesh_part = EAFrameUtil.FindChildRecursively(mesh, parts);

        if (mesh_part != null)
        {
            DestroyImmediate(mesh_part.gameObject);
        }
    }

    public void SetCharBase(EA_CCharBPlayer pDiaCharBase)
    { 
        m_pDiaCharBase = pDiaCharBase;
    }

    public EA_CCharBPlayer GetCharBase()
    {
        return m_pDiaCharBase;
    }

    //  [5/28/2014 puos] Instruction with input
    public void OnActorInputSendEvent(params object[] command)
    { 
        if(actorInputSendEvent != null)
        {  
            actorInputSendEvent(command);
        }
    }  

    /// <summary>
    /// 
    /// </summary>
    protected virtual void RaiseWeapon()
    {
        FindWeapon();

        if (m_pCurWeapon != null)
        {
            //  [4/9/2014 puos] Reset weapon info
            m_pCurWeapon.RaiseWeapon();
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    protected void FindWeapon()
    {
        EA_CCharBPlayer pActor = GetCharBase();

        EA_Equipment pEquipment = null;

        //2017 1126 Consider even without weapons
        m_pCurWeapon = null;

        if (pActor != null)
        {
            pEquipment = EA_ItemManager.instance.Equip_FindEqipment(pActor.GetObjID());
        }

        if (pEquipment != null)
        {
            EA_CItemUnit pItemUnit = pEquipment.GetCurrentItem();

            if (pItemUnit != null)
            {
                EA_CItem pItem = EACObjManager.instance.GetItemObject(pItemUnit.GetObjId());

                if (pItem != null)
                {
                    Debug.Assert(pItem.GetLinkItem() != null, "EAActor FindWeapon pItem.GetLinkItem() is null");

                    if (pItem.GetLinkItem() != null)
                        m_pCurWeapon = (EAWeapon)pItem.GetLinkItem() as EAWeapon;
                }
            }
        }

        //  [5/28/2018 puos] bug fixed
        if(m_pCurWeapon == null)
        {
            if (pActor != null)
            {
                pEquipment = EA_ItemManager.instance.Equip_FindEqipment(pActor.GetObjID());
            }

            if (pEquipment != null)
            {
                EA_CItemUnit pItemUnit = pEquipment.GetCurrentItem();

                if (pItemUnit != null)
                {
                    EA_CItem pItem = EACObjManager.instance.GetItemObject(pItemUnit.GetObjId());

                    if (pItem != null)
                    {
                        Debug.Assert(pItem.GetLinkItem() != null, "EAActor FindWeapon2 pItem.GetLinkItem() is null");

                        if(pItem.GetLinkItem() != null)
                           m_pCurWeapon = (EAWeapon)pItem.GetLinkItem() as EAWeapon;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parts_basic"></param>
    /// <returns></returns>
    private GameObject Attach(SkinnedMeshRenderer parts_basic)
    {
        Transform mesh = GetTransform("mesh");

        GameObject go = EAFrameUtil.AddChild(mesh.gameObject, parts_basic.gameObject);

        if (go != null)
        {
            go.SetActive(true);

            SkinnedMeshRenderer parts = go.GetComponent<SkinnedMeshRenderer>();

            if (parts != null)
            {
                EASKinInfo skinInfo = parts.GetComponent<EASKinInfo>();

                Debug.Assert(skinInfo != null, "SkinInfo not valid : " + parts_basic.gameObject.name);

                if (skinInfo != null)
                {
                    Transform[] bones = new Transform[skinInfo.BoneNames.Length];

                    for (int i = 0; i < skinInfo.BoneNames.Length; i++)
                    {
                        bones[i] = GetTransform(skinInfo.BoneNames[i]);
                    }

                    parts.rootBone = GetTransform(skinInfo.RootboneName);
                    parts.bones = bones;
                    Bounds b = parts.localBounds;
                    b.center = Vector3.zero;
                    parts.localBounds = b;
                }
            }
        }

        return go;
    }

    /// <summary>
    ///  skeleton setting
    /// </summary>
    public void SetSkeleton()
    {
        if(transformList.Count <= 0)
        {
            EACharacterInfo characterInfo = transform.GetComponent<EACharacterInfo>();

            if(characterInfo != null)
            {
                Transform[] transforms = characterInfo.Bones;
                string[] BoneNames     = characterInfo.BoneNames;

                for (int i = 0; i < transforms.Length; ++i)
                {
                    int key = CRC32.GetHashForAnsi(BoneNames[i]);
                    transformList.Add(key, transforms[i]);
                }
            }
            else
            {
                Debug.LogError("SetSkeleton - EACharacterInfo is null : " + gameObject.name);

                Transform[] transforms = transform.GetComponentsInChildren<Transform>();

                for (int i = 0; i < transforms.Length; ++i)
                {
                    int key = CRC32.GetHashForAnsi(transforms[i].name);

                    Transform value = null;

                    if (!transformList.TryGetValue(key, out value))
                    {
                        transformList.Add(key, transforms[i]);
                    }
                }
            }   
        }  

        Transform mesh = GetTransform("mesh");

        if (mesh != null)
        {
            mesh.transform.parent = null;
            GameObject.Destroy(mesh.gameObject);
        }

        GameObject _mesh = EAFrameUtil.AddChild(gameObject, "mesh");

        if (_mesh != null)
        {
            AddTransform("mesh", _mesh.transform);
        }
    }

    public void SetRenderer()
    {
        bool buseParts = false;

        for (int i = 0; i < m_PartTblId.Length; ++i)
        {
            if (!string.IsNullOrEmpty(m_PartTblId[i]))
            {
                buseParts = true;
            }  
        } 
        
        if(buseParts == false)
        {
            EACharacterInfo characterInfo = transform.GetComponent<EACharacterInfo>();

            if (characterInfo != null)
            {
                renderers = characterInfo.renderers;
            }
        }
        else
        {
            renderers = transform.GetComponentsInChildren<Renderer>();
        }     
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_key"></param>
    /// <returns></returns>
    public Transform GetTransform(string _key)
    {
        int key = CRC32.GetHashForAnsi(_key);

        Transform outValue = null;

        transformList.TryGetValue(key, out outValue);

        return outValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_key"></param>
    /// <param name="obj"></param>
    public void AddTransform(string _key, Transform obj)
    {
        int key = CRC32.GetHashForAnsi(_key);

        Transform outValue = null;

        if (!transformList.TryGetValue(key, out outValue))
        {
            transformList.Add(key, obj);
        }
        else
        {
            transformList[key] = obj;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strObjectName"></param>
    /// <returns></returns>
    public GameObject GetObjectInActor(string strObjectName)
    {
        Transform t = GetTransform(strObjectName);

        if(t != null)
        {
           return t.gameObject;
        } 

        return gameObject;
    }
    
    
    /// <summary>
    /// String value in OnAction; Parsed as character
    /// </summary>
    /// <param name="szValues"></param>
    /// <returns></returns>
    protected string[] GetSenderValues(string szValues)
    {
        string szSeparateExt = ";";
        string[] arValueParser = szValues.Split(szSeparateExt.ToCharArray());
        return arValueParser;
    }

    /// <summary>
    /// Display currently registered bone
    /// </summary>
    protected void DrawBone()
    {
        foreach (KeyValuePair<int,Transform> t in transformList)
        {
            float len = 0.05f;
            Vector3 loxalX = new Vector3(len, 0, 0);
            Vector3 loxalY = new Vector3(0, len, 0);
            Vector3 loxalZ = new Vector3(0, 0, len);

            loxalX = t.Value.rotation * loxalX;
            loxalY = t.Value.rotation * loxalY;
            loxalZ = t.Value.rotation * loxalZ;

            for(int i = 0; i < t.Value.childCount; ++i)
            {
                Debug.DrawLine(t.Value.position, t.Value.GetChild(i).position , Color.white);
            }
        }
    }
}
