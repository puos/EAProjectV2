using UnityEngine;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

public class MathUtil
{
	static Vector3[] headingToVectorTable = new Vector3[1025];

	public const float HALF_PI = Mathf.PI * 0.5f;
	public const float TWO_PI = Mathf.PI * 2f;

    static System.Random rand = new System.Random();

    static MathUtil()
	{
		for (int a = 0; a <= 1024; ++a)
		{
			float ang = a * 2 * Mathf.PI / 1024;
			Vector3 v;
			v.x = Mathf.Sin(ang);
			v.y = 0;
			v.z = Mathf.Cos(ang);
			headingToVectorTable[a] = v;
			headingToVectorTable[a].Normalize();
		}
	}

	static public float GetHeading360FromVector(Vector3 v)
	{
		return GetHeading360FromXZ(v.x, v.z);
	}

	static public float GetHeadingFromVector(Vector3 v)
	{
		return GetHeadingFromXZ(v.x, v.z);
	}

	static public float GetHeading360FromXZ(float dx, float dz)
	{
		return Mathf.Rad2Deg * GetHeadingFromXZ(dx, dz);
	}

	static public float GetHeadingFromXZ(float dx, float dz)
	{
		float h;

		if (dz != 0)
		{
			float d = dx / dz;
			if (d > 1)
			{
				h = HALF_PI - d / (d * d + 0.28f);
			}
			else if (d < -1)
			{
				h = -HALF_PI - d / (d * d + 0.28f);
			}
			else
			{
				h = d / (1f + 0.28f * d * d);
			}
			if (dz < 0)
			{
				if (dx > 0)
					h += Mathf.PI;
				else
					h -= Mathf.PI;
			}

		}
		else
		{
			if (dx > 0)
				h = HALF_PI;
			else
				h = -HALF_PI;
		}
		return h;
	}

    // The difference between from and to is modulated from -PI to + PI and returned.
    static public float GetAngleDiffInRadian(float from, float to)
	{
		float diff = (to-from) % TWO_PI;
		if(diff < -Mathf.PI)
			diff += TWO_PI;
		if(diff > Mathf.PI)
			diff -= TWO_PI;
		return diff;
	}

    // The difference between from and to is modulated from -180 to +180 and returned.
    static public float GetAngleDiffInDegree(float degreeFrom, float degreeTo)
	{
		float diff = (degreeTo - degreeFrom) % 360f;
		if (diff < -180f)
			diff += 360f;
		if (diff > 180f)
			diff -= 360f;
		return diff;
	}

	// 0~2PI로 modulation된 radian을 얻는다
	public static float NormalizeHeadingInRadian(float rad)
	{
		rad = rad % TWO_PI;
		if(rad < 0)
			rad=TWO_PI + rad;
		return rad;
	}

	// 0 ~ 360도로 modulation시킨다.
	public static float NormalizeHeadingInDegree(float degree)
	{
		degree = degree % 360f;
		if (degree < 0)
			degree = 360f + degree;
		return degree;
	}

	public static float ModulateDegree360(float degree)
	{
		degree = degree % 360f;
		if (degree < 0)
			degree = 360f + degree;
		return degree;
	}

    public static bool TwoCirclesOverlapped(Vector3 c1, float r1,
           Vector3 c2, float r2)
    {
        float DistBetweenCenters = Mathf.Sqrt((c1.x - c2.x) * (c1.x - c2.x)
                + (c1.z - c2.z) * (c1.z - c2.z));

        if ((DistBetweenCenters < (r1 + r2)) || (DistBetweenCenters < Mathf.Abs(r1 - r2)))
        {
            return true;
        }

        return false;
    }

    /**
    *  returns true if the point p is within the radius of the given circle
    */
    public static bool PointInCircle(Vector3 pos,
            float radius,
            Vector3 p)
    {
        Vector2 _p   = ToVec2(p);
        Vector2 _pos = ToVec2(pos);

        double DistFromCenterSquared = (p - pos).sqrMagnitude;

        if (DistFromCenterSquared < (radius * radius))
        {
            return true;
        }

        return false;
    }

    public static bool IntersectRayWithSphere(Ray ray, Vector3 center, float radius, out float dist)
	{
		Vector3 dst = ray.origin - center;
		float B = Vector3.Dot(dst, ray.direction);
		float C = Vector3.Dot(dst, dst) - radius * radius;
		float D = B * B - C;
		if (D > 0)
        { // The sphere completely contains ray.
            dist = -B - Mathf.Sqrt(D);
			return true;
		}
		dist = float.MaxValue;
		return false;
	}

    public static Vector3 NearestPointOnLine(Ray ray, Vector3 pnt)
    {
        var v = pnt - ray.origin;

        var d = Vector3.Dot(v, ray.direction);

        return ray.origin + ray.direction * d;
    }

    public static bool IntersectRayWithPoint(Ray ray, Vector3 pnt , float distance , float radius )
    {
        var v = pnt - ray.origin;

        var d = Vector3.Dot(v, ray.direction);

        bool check = false;

        if ( d <= distance && d >= 0)
        {
            Vector3 closetPoint = ray.origin + ray.direction * d;

            if((pnt - closetPoint).magnitude <= radius )
            {
                check = true;
            }  
        }

        return check;
    } 

    public static bool IntersectLineCircleXZ(Vector3 center, float radius, Vector3 lineFrom, Vector3 lineTo , out float dist)
	 {
		 KeyValuePair<bool,Vector2> result = IntersectLineCircle(ToVec2(center), radius, ToVec2(lineFrom), ToVec2(lineTo) , out dist);
         return result.Key;
	 }

     public static KeyValuePair<bool,Vector2> IntersectLineCircle(Vector2 center, float radius, Vector2 lineFrom, Vector2 lineTo, out float dist)
	 {
		 float h2;
		 Vector2 ac = center - lineFrom;
		 Vector2 ab = lineTo - lineFrom;
		 float ab2 = Vector2.Dot(ab, ab);
		 float acab = Vector2.Dot(ac, ab);
		 float t = acab / ab2;

		 if (t < 0)
			 t = 0;
		 else if (t > 1)
			 t = 1;

        Vector2 point = ((ab * t) + lineFrom);
        
         Vector2 h = point - center;
		 h2 = Vector2.Dot(h, h);

        dist = Mathf.Sqrt(h2);

        return new KeyValuePair<bool,Vector2>( h2 <= (radius * radius) , point);
	 }

    public static KeyValuePair<bool,Vector2> LineIntersection2D(Vector2 A,Vector2 B,Vector2 C,Vector2 D , out float dist)
    {
        // Line AB represented as a1x + b1y = c1  
        float a1 = B.y - A.y;
        float b1 = A.x - B.x;
        float c1 = a1 * (A.x) + b1 * (A.y);

        // Line CD represented as a2x + b2y = c2  
        float a2 = D.y - C.y;
        float b2 = C.x - D.x;
        float c2 = a2 * (C.x) + b2 * (C.y);

        float determinant = a1 * b2 - a2 * b1;

        if (determinant == 0)
        {
            dist = float.MaxValue;
            // The lines are parallel. This is simplified  
            // by returning a pair of FLT_MAX  
            return new KeyValuePair<bool, Vector2>(false , new Vector2(float.MaxValue , float.MaxValue));
        }
        else
        {
           
            float x = (b2 * c1 - b1 * c2) / determinant;
            float y = (a1 * c2 - a2 * c1) / determinant;

            Vector2 crossPoint = new Vector2(x, y);

            dist = (crossPoint - A).magnitude; 
            
            return new KeyValuePair<bool, Vector2>(true, crossPoint);
        }
    }

    public static bool LineIntersectionXZ(Vector3 A, Vector3 B, Vector3 C, Vector3 D, out float dist)
    {
        Vector2 a = ToVec2(A);
        Vector2 b = ToVec2(B);
        Vector2 c = ToVec2(C);
        Vector2 d = ToVec2(D);

        KeyValuePair<bool, Vector2> result = LineIntersection2D(a , b, c , d , out dist);

        return result.Key;
    }

    public static Vector3 RotateVectorXZ(Vector3 v, float radian)
	 {
		 float x = v.x * Mathf.Cos(radian) - v.z * Mathf.Sin(radian);
		 float z = v.x * Mathf.Sin(radian) + v.z * Mathf.Cos(radian);
		 return new Vector3(x, v.y, z);
	 }

	 public static Vector3 GetHeadingVector(float rad)
	 {
		 rad = NormalizeHeadingInRadian(rad);

		 return headingToVectorTable[(int)(rad / TWO_PI * 1024)];
	 }

	 public static bool IsEqualXZ(Vector3 a, Vector3 b)
	 {
		 if (a.x == b.x && a.z == b.z)
			 return true;

        return false;
	 }

	public static Vector2 ToVec2(Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

    public static Vector3 ToVec3(Vector2 v ,float y)
    {
        return new Vector3(v.x, y , v.y);
    }

    public static Vector2 GetDiffXZ(Vector3 to, Vector3 from)
	{
		return new Vector3(to.x - from.x, 0, to.z - from.z);
	}

    public static bool IsZero(Vector3 v)
    {
        return v.Equals(Vector3.zero);
    }

	public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 translate;
		translate.x = matrix.m03;
		translate.y = matrix.m13;
		translate.z = matrix.m23;
		return translate;
	}

	public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 forward;
		forward.x = matrix.m02;
		forward.y = matrix.m12;
		forward.z = matrix.m22;

		Vector3 upwards;
		upwards.x = matrix.m01;
		upwards.y = matrix.m11;
		upwards.z = matrix.m21;

		return Quaternion.LookRotation(forward, upwards);
	}
	public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 scale;
		scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
		scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
		scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
		return scale;
	}

    public static Vector3 Truncate(Vector3 v, float force)
    {
        if (v.magnitude > force)
        {
            v.Normalize();
            v *= force;
        }

        return v;
    }

    public static float GetSignedAngle(Vector3 a, Vector3 b)
	{
		float angle = Vector3.Angle(a, b);
		return angle * Mathf.Sign(Vector3.Cross(a, b).y);
	}

    public static float GetSignedAngle(Vector2 a, Vector2 b)
	{
		float angle = Vector2.Angle(a, b);
		return angle * Mathf.Sign(GetCrossProductScalar(a, b));
	}

    public static Vector2 vLT = Vector2.zero;
    public static Vector2 vRT = Vector2.zero;
    public static Vector2 vLB = Vector2.zero;
    public static Vector2 vRB = Vector2.zero;

    public static Vector2[] lines = new Vector2[4];
    
    ///////////////////////////////////////////////////////////////////////////////
    //treats a window as a toroid
    public static Vector3 WrapAround(ref Vector3 heading, Vector3 pos, float radius ,float MaxX, float MaxY)
    {
        float _MaxX = MaxX * 0.5f;
        float _MaxY = MaxY * 0.5f;
        float _MinX = -1.0f * _MaxX;
        float _MinY = -1.0f * _MaxY;

        Vector2 _pos = ToVec2(pos);
        Vector2 _heading = ToVec2(heading);

        vLT.x = _MinX;
        vLT.y = _MaxY;

        vRT.x = _MaxX;
        vRT.y = _MaxY;

        vLB.x = _MinX;
        vLB.y = _MinY;

        vRB.x = _MaxX;
        vRB.y = _MinY;

        
        float dist = float.MaxValue;

        lines[0] = vLT;
        lines[1] = vRT;
        lines[2] = vRB;
        lines[3] = vLB;

        for(int i = 0; i < lines.Length; ++i)
        {
            float d = float.MaxValue;

            Vector2 a = lines[i];
            Vector2 b = lines[(i + 1) % lines.Length];

            KeyValuePair<bool , Vector2> result = IntersectLineCircle(_pos, radius, a , b, out d);

            if (result.Key && (d <= dist))
            {
                Vector2 normal = Vector2.Perpendicular((a - b).normalized);
                _pos     = result.Value + normal * radius;
                _heading = (_heading + normal).normalized;
                dist = d;
            }
        }

        pos = ToVec3(_pos , pos.y );

        if(!_heading.Equals(Vector2.zero))
        {
            heading = ToVec3(_heading, heading.y);
            heading.Normalize();
        }    
        
        return pos;
    }

    private static Vector2 AccumulateNormal(Vector2 a, Vector2 b,Vector2 _heading)
    {
        Vector2 normal = Vector2.Perpendicular((a - b).normalized);
        _heading = (_heading + normal).normalized;
        return _heading;
    }

    public static float GetCrossProductScalar(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	public static bool InRange(float val, float from, float to)
	{
		return from <= val && val <= to;
	}

    public static float RandInRange(float min, float max)
    {
        if (min >= max)
            return min;

        return min + (float)rand.NextDouble() * (max - min);
    }

    //returns a random float in the range -1 < n < 1
    public static float RandomClamped()
    {
        return ((float)rand.Next(0, 200)) / 100.0f - 1.0f;
    }

    //returns a random integer between x and y
    public static int RandInt(int x, int y)
    {
        Debug.Assert(y >= x ,"<RandInt>: y is less than x");
        return rand.Next(int.MaxValue - x) % (y - x + 1) + x;
    }

    //returns a random double between zero and 1
    public static double RandFloat()
    {
        return rand.NextDouble();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static float Lerp(float start, float end, float value)
    {
        end -= start;
        return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
    }

    /// <summary>
    ///  easeout expo
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Vector3 Lerp(Vector3 start, Vector3 end, float value)
    {
        end -= start;
        return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="q1"></param>
    /// <param name="q2"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Quaternion Lerp(Quaternion q1, Quaternion q2, float value)
    {
        Quaternion c = Quaternion.identity;

        c.x = Lerp(q1.x, q2.x, value);
        c.y = Lerp(q1.y, q2.y, value);
        c.z = Lerp(q1.z, q2.z, value);
        c.w = Lerp(q1.w, q2.w, value);

        c.Normalize();

        return c;
    }

    /// <summary>
    ///  Check if the rotation value has changed. Tolerance epsilon
    /// </summary>
    /// <param name="q1"></param>
    /// <param name="q2"></param>
    /// <returns></returns>
    public static bool Equal(Quaternion q1, Quaternion q2)
    {
        float epsilon = 0.001f;
        return Mathf.Abs(q1.x - q2.x) < epsilon &&
               Mathf.Abs(q1.y - q2.y) < epsilon &&
               Mathf.Abs(q1.z - q2.z) < epsilon &&
               Mathf.Abs(q1.w - q2.w) < epsilon;
    }

    /// <summary>
    /// Check if the position value has changed. Tolerance epsilon
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public static bool Equal(Vector3 v1, Vector3 v2 , float epsilon = 0.01f)
    {
        Vector3 offset = v1 - v2;

        return (offset.magnitude < epsilon) ? true : false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="f1"></param>
    /// <param name="f2"></param>
    /// <returns></returns>
    public static bool Equal(float f1, float f2)
    {
        float epsilon = 0.01f;

        return (Mathf.Abs(f1 - f2) < epsilon) ? true : false;
    }

    public static Vector3 PointToWorldSpace(Vector3 point,
  Vector3 AgentHeading,
  Vector3 AgentSide,
  Vector3 AgentPosition)
    {
        Vector3 up = Vector3.Cross(AgentHeading, AgentSide);

        //create a transformation matrix
        Matrix4x4 matTransform = Matrix4x4.TRS(AgentPosition, Quaternion.LookRotation(AgentHeading, up), Vector3.one);

        Vector3 transPoint = matTransform.MultiplyPoint(point);

        return transPoint;
    }

    public static Vector3 VectorToWorldSpace(Vector3 vector,
 Vector3 AgentHeading,
 Vector3 AgentSide)
    {
        Vector3 up = Vector3.Cross(AgentHeading, AgentSide);

        //create a transformation matrix
        Matrix4x4 matTransform = Matrix4x4.TRS(Vector3.zero , Quaternion.LookRotation(AgentHeading, up), Vector3.one);

        Vector3 transPoint = matTransform.MultiplyVector(vector);

        return transPoint;
    }

    //--------------------- PointToLocalSpace --------------------------------
    //
    //------------------------------------------------------------------------
    public static Vector3 PointToLocalSpace(Vector3 point,
            Vector3 AgentHeading,
            Vector3 AgentSide,
            Vector3 AgentPosition)
    {

        Vector3 up = Vector3.Cross(AgentHeading, AgentSide);

        //create a transformation matrix
        Matrix4x4 matTransform = Matrix4x4.TRS(AgentPosition, Quaternion.LookRotation(AgentHeading, up), Vector3.one);

        Vector3 transPoint = matTransform.inverse.MultiplyPoint(point);

        return transPoint;
    }

    public class Frustum2D
	{
		Vector3 _pos;
		Vector3 _look;
		Vector3 _right;
		float _near;
		float _far;
		float _rightFactor;
		public Frustum2D(Vector3 pos, Vector3 lookN, float near, float far, float fovDegree)
		{
			_pos = pos;
			_look = lookN;
			_look.y = 0;
			_look.Normalize();

			_right = Vector3.Cross(_look, Vector3.up);
			_rightFactor = Mathf.Tan( fovDegree * Mathf.Deg2Rad * 0.5f );
			_near = near;
			_far = far;
		}

        // horzRadius : frustum side and radius to be calculated
        // vertRadius : near, far and radius to be calculated
        public bool HitTest(Vector3 p, float horzRadius, float vertRadius)
		{
			Vector3 OP = p - _pos;

			float f = Vector3.Dot(OP, _look); // f: forward방향으로의 거리

			if (f < _near - vertRadius || _far + vertRadius < f) return false;

			float r = Vector3.Dot(OP, _right);

			float rLimit = _rightFactor * f + horzRadius;
			if (r < -rLimit || rLimit < r) return false;
			
			return true;
		}
	}

	public class Frustum3D
	{
		Vector3 _pos;
		Vector3 _look;
		Vector3 _right;
		Vector3 _up;
		float _near;
		float _far;
		float _rightFactor;
		float _upFactor;
		public Frustum3D(Vector3 pos, Vector3 look, Vector3 right, Vector3 up, float near, float far, float fovDegree, float camAspect)
		{
			_pos = pos;
			_look = look;
			_up = up;
			_right = right;
			
			_upFactor = Mathf.Tan(fovDegree * Mathf.Deg2Rad * 0.5f);
			_rightFactor = _upFactor * camAspect;
			
			_near = near;
			_far = far;
		}

        // horzRadius : frustum side and radius to be calculated
        // vertRadius : near, far and radius to be calculated
        public bool HitTest(Vector3 p, float radius, bool includeNearFarTest = true)
		{
			Vector3 OP = p - _pos;

			float f = Vector3.Dot(OP, _look); // f: forward방향으로의 거리

			if (includeNearFarTest && (f < _near - radius || _far + radius < f)) return false;

			float r = Vector3.Dot(OP, _right);
			float rLimit = _rightFactor * f + radius;
			if (r < -rLimit || rLimit < r) return false;

			float u = Vector3.Dot(OP, _up);
			float uLimit = _upFactor * f + radius;
			if (u < -uLimit || uLimit < u) return false;

			return true;
		}
	}
}

public class InvertedAABBox
{
    private Vector3 m_vTopLeft;
    private Vector3 m_vBottomRight;
    private Vector3 m_vCenter;
    private Vector3 m_vExtenstion;

    public InvertedAABBox(Vector3 tl, Vector3 br)
    {
        m_vTopLeft = tl;
        m_vBottomRight = br;
        m_vCenter = (tl + br) * 0.5f;
        m_vExtenstion = m_vCenter - br;
    }

    //returns true if the bbox described by other intersects with this one
    public bool isOverlappedWith(InvertedAABBox other)
    {
        return !((other.Top() > this.Bottom())
                || (other.Bottom() < this.Top())
                || (other.Left() > this.Right())
                || (other.Right() < this.Left())
                || (other.Front() > this.Back())
                || (other.Back() < this.Front()));
    }

    public Vector3 TopLeft()
    {
        return m_vTopLeft;
    }

    public Vector3 BottomRight()
    {
        return m_vBottomRight;
    }

    public float Top()
    {
        return m_vTopLeft.y;
    }

    public float Left()
    {
        return m_vTopLeft.x;
    }

    public float Bottom()
    {
        return m_vBottomRight.y;
    }

    public float Right()
    {
        return m_vBottomRight.x;
    }

    public float Back()
    {
        return m_vBottomRight.z;
    }

    public float Front()
    {
        return m_vTopLeft.z;
    }

    public Vector3 Center()
    {
        return m_vCenter;
    }

    public void OnDebugRender()
    {
        Render(Color.blue, false);
    }

    void Render(Color color,bool RenderCenter)
    {
        float x = m_vExtenstion.x;
        float y = m_vExtenstion.y;
        float z = m_vExtenstion.z;

        Vector3 ruf = m_vCenter + new Vector3(x, y, z);
        Vector3 rub = m_vCenter + new Vector3(x, y, -z);
        Vector3 luf = m_vCenter + new Vector3(-x, y, z);
        Vector3 lub = m_vCenter + new Vector3(-x, y, -z);

        Vector3 rdf = m_vCenter + new Vector3(x, -y, z);
        Vector3 rdb = m_vCenter + new Vector3(x, -y, -z);
        Vector3 lfd = m_vCenter + new Vector3(-x, -y, z);
        Vector3 lbd = m_vCenter + new Vector3(-x, -y, -z);

        Color oldColor = Gizmos.color;
        Gizmos.color = color;

        Gizmos.DrawLine(ruf, luf);
        Gizmos.DrawLine(ruf, rub);
        Gizmos.DrawLine(luf, lub);
        Gizmos.DrawLine(rub, lub);

        Gizmos.DrawLine(ruf, rdf);
        Gizmos.DrawLine(rub, rdb);
        Gizmos.DrawLine(luf, lfd);
        Gizmos.DrawLine(lub, lbd);

        Gizmos.DrawLine(rdf, lfd);
        Gizmos.DrawLine(rdf, rdb);
        Gizmos.DrawLine(lfd, lbd);
        Gizmos.DrawLine(lbd, rdb);

        Gizmos.color = oldColor;

        if (RenderCenter)
        {
            Gizmos.DrawSphere(m_vCenter, 5);
        }
    }
}
