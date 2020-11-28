using UnityEngine;


public class ObStacle : AIObject
{
    public ObStacle(float x,
            float y,
            float r)
    {
        m_vPos = MathUtil.ToVec3(new Vector2(x, y) , 0);

        SetBRadius(r);
    }

    public ObStacle(Vector2 pos, float radius)
    {
        m_vPos = MathUtil.ToVec3(pos, 0);

        SetBRadius(radius);
    }

    
     //this is defined as a pure virtual function in AIObject so
     //it must be implemented
    public void Update(double time_elapsed)
    {
    }


    public void OnDebugRender()
    {
        Gizmos.color = Color.black;

        DebugExtension.DrawCircle(VPos(), BRadius());
    }


    public string toString()
    {
      return Tag + "," + VPos() + "," + BRadius();
    }

}
