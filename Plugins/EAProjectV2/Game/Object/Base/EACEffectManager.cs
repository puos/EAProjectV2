using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = EAFrameWork.Debug;
using GDEffectID = System.UInt32;

public class EACEffectManager : EAGenericSingleton<EACEffectManager>
{
    Dictionary<GDEffectID, EA_CEffectNode> m_mapEffectList = new Dictionary<GDEffectID, EA_CEffectNode>();

    //--------------------------------------------------------------------------
    //	effect index generation
    EAIDGenerator m_pIDGenerator = new EAIDGenerator(50000);
    //--------------------------------------------------------------------------

    public EACEffectManager()
    {
    }

    public void Destroy()
    {
        foreach (KeyValuePair<GDEffectID, EA_CEffectNode> pair in m_mapEffectList)
        {
            pair.Value.ResetInfo(eEffectState.ES_UnLoad);
        }

        m_mapEffectList.Clear();

        m_pIDGenerator.ReGenerate();
    }
    
    public void StartChangeMap()
    {
        Destroy();

        Debug.Log("StartChangeMap - EACEffectManager");
    }

   
    public void EndChangeMap()
    {
        Destroy();
        
        CEffectResourcePoolingManager.instance.Destroy();

        Debug.Log("EndChangeMap - EACEffectManager");
    }

    public EA_CEffectNode CreateEffect(EA_EffectBaseInfo info)
    {
        info.m_eEffectState = eEffectState.ES_Load;

        //	Temporarily specify ObjId (sometimes temporary use by external system)
        bool bCreateTempId = false;

        if (CObjGlobal.InvalidEffectID == info.m_GDEffectId)
        {
            info.m_GDEffectId = (GDEffectID)m_pIDGenerator.GenerateID();
            bCreateTempId = true;
        }

        EA_CEffectNode pEffectNode = null;

        if (bCreateTempId == false)
        {
            pEffectNode = GetEffectGroup(info.m_GDEffectId);
        }

        if (pEffectNode == null)
        {
            pEffectNode = new EA_CEffectNode();

            if (pEffectNode != null)
            {
                m_mapEffectList.Add(info.m_GDEffectId, pEffectNode);
            }
        }

        //	Enter and apply the generated information
        Debug.Assert(info != null, "No Effect Info");

        if (pEffectNode != null)
        {
            pEffectNode.SetObjInfo(info);
        }
        
        return pEffectNode;
    }

    public void ResourceLoad(string szPath,string key, int initCreateCount = 1)
    {
        CEffectResourcePoolingManager.instance.CreatePool(key , szPath, initCreateCount);
    }

    public void ResourceUnLoad(string key)
    {
        CEffectResourcePoolingManager.instance.DeletePool(key);
    }
        
    public EA_CEffectNode GetEffectGroup(GDEffectID _id)
    {
        EA_CEffectNode pEffectNode = null;

        m_mapEffectList.TryGetValue(_id, out pEffectNode);

        return pEffectNode;
    }

    public void RemoveEffect(GDEffectID _id)
    {
        EA_CEffectNode pEffectNodeGroup = GetEffectGroup(_id);

        if (pEffectNodeGroup != null)
        {  
            pEffectNodeGroup.ResetInfo(eEffectState.ES_UnLoad);
            m_mapEffectList.Remove((uint)_id);
            m_pIDGenerator.FreeID(_id);
        }
    }

    // puos 20141019 Delete an effect associated with a specific actor
    public void DeleteRelatedEffectActor(uint ActorId , bool bOnlyNotLoop = false)
    {
        List<uint> uEffectIDList = new List<uint>();

        // Find a specific actor id
        foreach (KeyValuePair<GDEffectID, EA_CEffectNode> pair in m_mapEffectList)
        {
            // Get the effect id corresponding to the actor
            if (pair.Value.GetEffectBaseInfo().m_AttachObjectId == ActorId)
            {
                if (bOnlyNotLoop == false)
                {
                    uEffectIDList.Add(pair.Value.GetEffectBaseInfo().m_GDEffectId); 
                }
                else
                {
                    // loop passes.
                    if (pair.Value.GetEffectBaseInfo().m_eLifeType == eEffectLifeType.eLimitTime)
                    {
                        uEffectIDList.Add(pair.Value.GetEffectBaseInfo().m_GDEffectId); 
                    }   
                }
            }
        }

        // Delete the effect list.
        for (int i = 0; i < uEffectIDList.Count; ++i)
        {
            RemoveEffect(uEffectIDList[i]);
        }
    }
}
