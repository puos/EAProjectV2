using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Debug = EAFrameWork.Debug;

public class EA_CItem : EA_CObjectBase
{
    ItemObjInfo m_ItemInfo = new ItemObjInfo();
    
    protected EAItem m_pLinkItem = null;

    public EA_CItem()
    {
    }

    public override eObjectKind GetKind() { return eObjectKind.CK_ITEM; }

	public bool	SetItemInfo( ItemObjInfo iteminfo )
    {
        m_ItemInfo.Copy(iteminfo);
        return true;
    }

    public virtual bool Use()
    {
        if (m_pLinkItem != null)
        {
            m_pLinkItem.Use();
        }

        return true;
    } 


    public ItemObjInfo GetItemInfo()
    {
        return m_ItemInfo; 
    }

	public override bool ResetInfo( eObjectState eChangeState )
    {
        m_ObjInfo.m_eObjState = eChangeState;
		SetObjInfo(  m_ObjInfo  );
        SetItemInfo( m_ItemInfo );
        return true;
    }

    public void SetLinkItem(EAItem pItem)
    {
        if (m_pLinkItem != pItem && pItem != null)
        {   m_pLinkItem = pItem;
            m_pLinkItem.SetItemBase(this);
        }
    }
    
    public EAItem GetLinkItem()  {   return m_pLinkItem; }

    public override bool SetObjInfo(ObjectInfo ObjInfo)
    {
        m_ObjInfo.Copy(ObjInfo);

        switch (m_ObjInfo.m_eObjState)
        {

            case eObjectState.CS_DEAD:
                {
                    if (m_pLinkItem != null)
                    {   m_pLinkItem.DeSpawnAction();
                        m_pLinkItem.SetItemBase(null);
                    }

                    m_pLinkItem = null;
                }
                break;
        }

        base.SetObjInfo(ObjInfo);

        switch (m_ObjInfo.m_eObjState)
        {
            case eObjectState.CS_SETENTITY:
                {
                    if (m_pLinkItem != null)
                    {   m_pLinkItem.SpawnAction();
                    }
                }
                break;
        }


        return true;
    } 
}