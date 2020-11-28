using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GDEffectID = System.UInt32;
using GDTblID    = System.UInt32;
using GDObjID    = System.UInt32;
using Debug = EAFrameWork.Debug;

public enum eEffectLifeType   // life type of node
{
    eLoop = 0,        // loop
    eLimitTime,       // limit
}



public enum eEffectAttachType     // attachment type of node
{
    eLinkBone = 0,         // link bone
    eLinkOffset,           // Link to Object / Avatar Offset
    eWorld,                // game world
}

public enum eEffectState
{
    ES_Start,
    ES_Load,
    ES_Stop,
    ES_ForceStop,
    ES_UnLoad,
};


public enum eEffectResourceType
{
     eParticle,
     eDecal,
     eMeshRender,
}

public class EA_EffectBaseInfo
{
    public GDEffectID            m_GDEffectId;
    public GDObjID               m_AttachObjectId;
    public string                m_AttachBoneName;
    public eEffectLifeType       m_eLifeType;
    public eEffectAttachType     m_eAttachType;
    public eEffectState          m_eEffectState;
    public float                 m_fStartTime;
    public float                 m_fLifeTime;
    public string                m_EffectTableIndex;
    public eEffectResourceType   m_EffectResourceType;
    public bool                  m_bForceAxis;
    public float                 m_fForceYpos;
    public string                m_strObjectId;
   
    public float[] m_EmitPos   = new float[] { CObjGlobal.fInvalidPos, CObjGlobal.fInvalidPos, CObjGlobal.fInvalidPos };	// Move to the location other than 0,0,0
    public float[] m_EmitAngle = new float[] { CObjGlobal.fInvalidAngle, CObjGlobal.fInvalidAngle, CObjGlobal.fInvalidAngle };	//	If it's not 0,0,0, look in that direction

    public EA_EffectBaseInfo()
    {
        m_GDEffectId = CObjGlobal.InvalidEffectID;
        m_AttachObjectId = CObjGlobal.InvalidObjID;
        m_AttachBoneName = "";
        m_eLifeType  = eEffectLifeType.eLoop;
        m_eAttachType = eEffectAttachType.eWorld;
        m_eEffectState = eEffectState.ES_UnLoad;
        m_fStartTime   = 0;
        m_fLifeTime    = 0;
        m_EffectTableIndex = "";
        m_EffectResourceType = eEffectResourceType.eParticle;
        m_bForceAxis         = false;
        m_fForceYpos         = 0.0f;
        m_strObjectId        = "";
    }

    public void Copy(EA_EffectBaseInfo ib)
    {
        m_GDEffectId = ib.m_GDEffectId;
        m_AttachObjectId = ib.m_AttachObjectId;
        m_AttachBoneName = ib.m_AttachBoneName;
        m_eLifeType = ib.m_eLifeType;
        m_eAttachType = ib.m_eAttachType;
        m_eEffectState = ib.m_eEffectState;
        m_fStartTime = ib.m_fStartTime;
        m_fLifeTime = ib.m_fLifeTime;
        m_EffectTableIndex = ib.m_EffectTableIndex;

        m_EmitPos[0] = ib.m_EmitPos[0];
        m_EmitPos[1] = ib.m_EmitPos[1];
        m_EmitPos[2] = ib.m_EmitPos[2];

        m_EmitAngle[0] = ib.m_EmitAngle[0];
        m_EmitAngle[1] = ib.m_EmitAngle[1];
        m_EmitAngle[2] = ib.m_EmitAngle[2];

        m_EffectResourceType = ib.m_EffectResourceType;
        m_bForceAxis         = ib.m_bForceAxis;
        m_fForceYpos         = ib.m_fForceYpos;
        m_strObjectId        = ib.m_strObjectId;
     }
}