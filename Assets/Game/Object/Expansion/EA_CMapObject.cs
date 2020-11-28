using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Debug = EAFrameWork.Debug;

public class EA_CMapObject : EA_CObjectBase
{
    public EA_CMapObject()
    {
        m_pLinkMapObject = null;
    }

    public override eObjectKind GetKind() { return eObjectKind.CK_MAP; }

    public void SetLinkMapObject(EAMapObject pMapObject)
    {
        if (m_pLinkMapObject != pMapObject && pMapObject != null)
        {
            m_pLinkMapObject = pMapObject;
            m_pLinkMapObject.SetMapBase(this);
        }
    }

    public bool SetMapInfo(MapObjInfo mapInfo)
    {
        m_MapInfo.Copy(mapInfo);
        return true;
    }

    public MapObjInfo GetMapInfo()
    {
        return m_MapInfo;
    }

    public override bool ResetInfo(eObjectState eChangeState)
    {
        m_ObjInfo.m_eObjState = eChangeState;
        SetObjInfo(m_ObjInfo);
        SetMapInfo(m_MapInfo);
        return true;
    }

    public override bool SetObjInfo(ObjectInfo ObjInfo)
    {
        m_ObjInfo.Copy(ObjInfo);

        switch (m_ObjInfo.m_eObjState)
        {

            case eObjectState.CS_DEAD:
                {
                    if (m_pLinkMapObject != null)
                    {
                        m_pLinkMapObject.DeSpawnAction();
                        m_pLinkMapObject.SetMapBase(null);
                    }

                    m_pLinkMapObject = null;
                }
                break;
        }

        base.SetObjInfo(ObjInfo);

        switch (m_ObjInfo.m_eObjState)
        {
            case eObjectState.CS_SETENTITY:
                {
                    if (m_pLinkMapObject != null)
                    {   m_pLinkMapObject.SpawnAction();
                    }
                }
                break;
        }


        return true;
    } 
     


    MapObjInfo m_MapInfo = new MapObjInfo();

    protected EAMapObject m_pLinkMapObject = null;
    
}
