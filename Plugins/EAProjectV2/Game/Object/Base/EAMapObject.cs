using UnityEngine;
using System.Collections;
using Debug = EAFrameWork.Debug;

public class EAMapObject : MonoBehaviour
{
    protected virtual void OnCreate() { }

    protected virtual void OnInit() { }

    protected virtual void OnUpdate() { }

    protected virtual void OnClose() { }

    protected virtual void OnDecay() { }

    private void Awake()
    {
        OnCreate();
    }

    private void OnDestroy()
    {
        OnDecay();
    }

    public void SpawnAction()
    {
        OnInit();
    }

    public void DeSpawnAction()
    {
        OnClose();
    }

    EA_CMapObject m_pDiaObjectBase = null;

    public void SetMapBase(EA_CMapObject pDiaMapBase)
    {
        m_pDiaObjectBase = pDiaMapBase;
    }

    public EA_CMapObject GetMapBase()
    {
        return m_pDiaObjectBase;
    }

    public virtual void OnAction(params object[] parms)
    {
    }
}
