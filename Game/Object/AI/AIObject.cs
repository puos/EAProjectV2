using UnityEngine;

public class AIObject
{
    //its location in the environment
    protected Vector3 m_vPos;

    //the length of this object's bounding radius
    protected float m_fBoundingRadius;

    public bool Tag { get; private set; }

    public void UnTagging()
    {
        Tag = false;
    }

    public void Tagging()
    {
        Tag = true;
    }

    public virtual float BRadius()
    {
        return m_fBoundingRadius;
    }

    public void SetBRadius(float r)
    {
        m_fBoundingRadius = r;
    }
    
    public virtual Vector3 VPos()
    {
        return m_vPos;
    }

    public virtual void SetVPos(Vector3 vPos)
    {
        m_vPos = vPos;
    }
}
