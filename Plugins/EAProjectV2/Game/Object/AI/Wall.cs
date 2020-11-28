using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = EAFrameWork.Debug;

public class Wall
{

    protected Vector3 m_vA = Vector3.zero,
            m_vB = Vector3.zero,
            m_vN = Vector3.zero;

    protected void CalculateNormal()
    {
        Vector3 normal = (m_vB - m_vA).normalized;

        m_vN = Vector3.Cross(Vector3.up , normal);
    }

    public Wall()
    {
    }

    public Wall(Vector3 A, Vector3 B)
    {
        m_vA = A;
        m_vB = B;
        CalculateNormal();
    }

    public Wall(Vector3 A, Vector3 B, Vector3 N)
    {
        m_vA = A;
        m_vB = B;
        m_vN = N;
    }

    public void OnDebugRender()
    {
        Render(false);
    }

    public void Render(bool RenderNormals)
    {
        Gizmos.DrawLine(m_vA, m_vB);

        //render the normals if rqd
        if (RenderNormals)
        {
            Vector3 v = Center();
            
            DebugExtension.DrawLineArrow(v,  v + m_vN * 5.0f);
        }
    }

    public Vector3 From()
    {
        return m_vA;
    }

    public void SetFrom(Vector3 v)
    {
        m_vA = v;
        CalculateNormal();
    }

    public Vector3 To()
    {
        return m_vB;
    }

    public void SetTo(Vector3 v)
    {
        m_vB = v;
        CalculateNormal();
    }

    public Vector3 Normal()
    {
        return m_vN;
    }

    public void SetNormal(Vector3 n)
    {
        m_vN = n;
    }

    public Vector3 Center()
    {
        return (m_vA + m_vB) * 0.5f;
    }
}
