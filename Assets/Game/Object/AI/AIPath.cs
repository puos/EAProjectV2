using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = EAFrameWork.Debug;

public class AIPath
{
    private List<Vector3> m_WayPoints = new List<Vector3>();

    int currentIdx = 0;

    public float TwoPi = Mathf.PI * 2.0f;

    bool m_bLooped;

    public AIPath()
    {
        m_bLooped = false;
    }

    public AIPath(int NumWaypoints, float MinX, float MinY, float MaxX, float MaxY, float fHeight , bool looped)
    {
        m_bLooped = looped;
        CreateRandomPath(NumWaypoints, MinX, MinY, MaxX, MaxY , fHeight);
    }

    public virtual Vector3 CurrentWaypoint()
    {
        if(m_WayPoints.Count <= 0)
        {
            return Vector3.zero;
        }

        if(m_WayPoints.Count > currentIdx)
        {
            return m_WayPoints[currentIdx];
        }
        else
        {
            return m_WayPoints[m_WayPoints.Count - 1];
        }
    }

    public virtual bool Finished()
    {
        if ( (currentIdx >= (m_WayPoints.Count - 1)) && !m_bLooped)
        {
            return true;
        }

        return false;
    }

    public virtual void SetNextWaypoint()
    {
        Debug.Assert(m_WayPoints.Count > 0);

        int limit = (m_WayPoints.Count == 0) ? 1 : m_WayPoints.Count;

        if (m_bLooped)
        {
            currentIdx = (currentIdx + 1) % limit;
        }
        else
        {
            currentIdx = Math.Min(currentIdx + 1, limit - 1);
        } 
    }

    //creates a random path which is bound by rectangle described by
    //the min/max values
    public virtual List<Vector3> CreateRandomPath(int NumWaypoints, float MinX, float MinY, float MaxX, float MaxY , float fHeight)
    {
        m_WayPoints.Clear();

        float midX = (MaxX + MinX) / 2.0f;
        float midY = (MaxY + MinY) / 2.0f;

        float smaller = Math.Min(midX, midY);

        float spacing = TwoPi / (float)NumWaypoints;

        for (int i = 0; i < NumWaypoints; ++i)
        {
            float RadialDist = MathUtil.RandInRange(smaller * 0.2f, smaller);

            Vector2 temp = new Vector2(RadialDist, 0.0f);

            temp = Vec2DRotateAroundOrigin(temp, i * spacing);

            Vector3 temp2 = Vector3.zero;

            temp2.x = temp.x + midX;
            temp2.y = fHeight;
            temp2.z = temp.y + midY;

            m_WayPoints.Add(temp2);
        }

        return m_WayPoints;
    }

    public Vector2 Vec2DRotateAroundOrigin(Vector2 v, float ang)
    {
        Quaternion q = Quaternion.Euler(0, 0, ang);

        //now transform the object's vertices
        Vector2 t = q * v;

        return t;
    }

   
    public virtual void LoopOn()
    {
        m_bLooped = true;
    }

    public virtual void LoopOff()
    {
        m_bLooped = false;
    }

    //methods for setting the path with either another Path or a list of vectors
    public virtual void Set(List<Vector3> new_path)
    {
        m_WayPoints.Clear();

        for (int i = 0; i < new_path.Count; ++i)
        {
            m_WayPoints.Add(new_path[i]);
        } 

        currentIdx = 0;
    }

    public virtual void Set(AIPath path)
    {
        Set(path.GetPath());
    }

    public virtual void Clear()
    {
        m_WayPoints.Clear();
    }

    public virtual List<Vector3> GetPath()
    {
        return m_WayPoints;
    }

    //renders the path in orange
    public void OnDebugRender()
    {
        if(m_WayPoints.Count <= 0)
        {
            return;
        }  

        int idx = 0;

        Vector3 wp = m_WayPoints[idx];

        Gizmos.color = Color.red;

        while (idx < m_WayPoints.Count - 1)
        {
            idx += 1;

            Vector3 n = m_WayPoints[idx];

            DebugExtension.DrawLineArrow2(wp, n,Color.black);

            wp = n;
        }

        if (m_bLooped)
        {
            DebugExtension.DrawLineArrow2(wp, m_WayPoints[0] , Color.black);
        }

        Gizmos.DrawSphere(wp, 0.1f);

        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(m_WayPoints[currentIdx], 0.1f);
    }
}