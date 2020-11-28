using UnityEngine;


public class AIAgent : AIObject
{
    Vector3 m_vVelocity; // Velocity vector

    Vector3 m_vConstVelocity;

    Vector3 m_vAddVelocity;

    Vector3 m_vSide;  // Vertical vector

    Vector3 m_vHeading;

    Vector3 m_vCrosshair;

    float m_fMass = 0f;

    float m_fMaxSpeed = 0f;

    float m_fMaxForce = 0f;

    float m_fMaxTurnRate = 0f;

    Vector3 m_vScale;

    float   m_fFriction    = 0.9f;

    float   m_fAddFriction = 0.99f;

    float m_fTimeElapsed;

    public uint objectId { get; private set; }

    AIGroup m_aiGroup = new AIGroup();

    public GameWorld World()
    {
        return GameWorld.instance;
    } 

    public Vector3 VTarget()
    {
        return m_vCrosshair;
    }

    public void SetAIGroup(AIGroup aiGroup)
    {
        m_aiGroup = aiGroup;
    }

    public AIGroup GetAIGroup()
    {
        return m_aiGroup;
    }

    public void SetVTarget(Vector3 vCrosshair)
    {
        m_vCrosshair = vCrosshair;
    }

    /**
     * @return time elapsed from last update
     */
    public float getTimeElapsed()
    {
        return m_fTimeElapsed;
    }

    public AIAgent(Vector3 position,float radius,Vector3 velocity,float max_speed, Vector3 heading,
           float mass, float turn_rate,float max_force , uint objectId ,string aigroup = "basic")
    {
        SetAgent(position,radius,velocity,max_speed,heading,mass,turn_rate,max_force , objectId, aigroup);
    }

    public AIAgent()
    {
        SetAgent(Vector3.zero , 0.0f ,  Vector3.zero ,  0 , Vector3.forward , 1 , Mathf.PI * 2.0f , 0 ,  0);
    }

    public void SetAgent(Vector3 position, float radius, Vector3 velocity, float max_speed, Vector3 heading,
           float mass, float turn_rate, float max_force,uint objectId, string aigroup = "basic")
    {
        
        SetVPos(position);

        m_fBoundingRadius = radius;

        SetVelocity(velocity);
        
        SetMass(mass);

        SetHeading(heading);
        
        SetMaxSpeed(max_speed);

        m_fMaxTurnRate = turn_rate;
        m_fMaxForce = max_force;

        SetObjectId(objectId);

        AddAgent(aigroup);
    }
  
    public virtual Vector3 Velocity()
    {
        return m_vVelocity;
    }

    public virtual void SetVelocity(Vector3 velocity)
    {
        m_vVelocity = velocity;
    }

    public virtual void SetConstVelocity(Vector3 velocity)
    {
        m_vConstVelocity = velocity;
    }

    public virtual void SetAddVelocity(Vector3 velocity)
    {
        m_vAddVelocity = velocity;
    }

    public virtual void SetMass(float mass)
    {
        m_fMass = mass;
    }

    public virtual float Mass()
    {
        return m_fMass;
    }

    public virtual Vector3 Side()
    {
        return m_vSide;
    }

    public virtual float MaxSpeed()
    {
        return m_fMaxSpeed;
    }

    public virtual void SetMaxSpeed(float new_speed)
    {
        m_fMaxSpeed = new_speed;
    }

    public virtual float MaxForce()
    {
        return m_fMaxForce;
    }

    public virtual void SetMaxForce(float mf)
    {
        m_fMaxForce = mf;
    }

    public virtual bool IsSpeedMaxedOut()
    {
        return m_fMaxSpeed * m_fMaxSpeed >= m_vVelocity.sqrMagnitude;
    }

    public virtual float Speed()
    {
        return m_vVelocity.magnitude;
    }

    public virtual float SpeedSq()
    {
        return m_vVelocity.sqrMagnitude;
    }

    public virtual Vector3 Heading()
    {
        return m_vHeading;
    }

    internal virtual double MaxTurnRate()
    {
        return m_fMaxTurnRate;
    }

    internal virtual void SetMaxTurnRate(float val)
    {
        m_fMaxTurnRate = val;
    }

    public void AddAgent(string aigroup)
    {
        World().AddAgent(aigroup, this);
    }

    public void SetObjectId(uint objectId)
    {
        this.objectId = objectId;
    }

    public virtual void Update(Vector3 acceleration, Vector3 gravityVelocity ,
        bool useWrapAround , float fElapsedTime )
    {
        //update velocity
        m_vVelocity += acceleration * fElapsedTime;

        m_fTimeElapsed = fElapsedTime;

        //make sure vehicle does not exceed maximum velocity
        MathUtil.Truncate(m_vVelocity, m_fMaxSpeed);

        Vector3 velocity = Vector3.zero;

        if (gravityVelocity.magnitude <= 0)
        {
            Vector3 friction = m_vVelocity * m_fFriction * fElapsedTime;
            m_vVelocity = m_vVelocity - friction;
            velocity    = m_vVelocity + m_vConstVelocity;
        }
        else
        {
            // Regardless of max speed or max accel, the acceleration of gravity is calculated separately.
            m_vVelocity      = m_vVelocity + gravityVelocity;
            velocity         = m_vVelocity + m_vConstVelocity;
        }

        //update the position
        m_vPos += (velocity + m_vAddVelocity) * fElapsedTime;

        Vector3 add_friction = m_vAddVelocity * m_fAddFriction * fElapsedTime;
        m_vAddVelocity = m_vAddVelocity - add_friction;


        //update the heading if the vehicle has a non zero velocity
        if (velocity.magnitude > 0)
        {
            SetHeading(velocity.normalized);
        } 
        
        Vector3 heading = Heading();

        if(GameWorld.instance.cxClient > 0 && GameWorld.instance.cyClient > 0)
        {
            if(useWrapAround)
            {
                //treat the screen as a toroid
                m_vPos = MathUtil.WrapAround(ref heading, m_vPos, m_fBoundingRadius, GameWorld.instance.cxClient, GameWorld.instance.cyClient);
            }   
        }     

        SetHeading(heading);
    }

    /// <summary>
    ///  given a target position, this method rotates the entity's heading and
    ///  side vectors by an amount not greater than m_dMaxTurnRate until it
    ///  directly faces the target.
    /// </summary>
    ///  <returns> true when the heading is facing in the desired direction </returns>
    public virtual bool RotateHeadingToFacePosition(Vector3 target)
    {
        Vector3 pos = m_vPos;
        Vector3 toTarget = (target - pos).normalized;

        //first determine the angle between the heading vector and the target
        float angle = Mathf.Acos(Vector3.Dot(m_vHeading,toTarget));

        if (float.IsNaN(angle))
        {
            angle = 0;
        }

        //return true if the player is facing the target
        if (angle < 0.00001)
        {
            return true;
        }

        //clamp the amount to turn to the max turn rate
        if (angle > m_fMaxTurnRate)
        {
            angle = m_fMaxTurnRate;
        }

        float sign = (MathUtil.GetSignedAngle(m_vHeading , toTarget) > 0) ? 1 : -1 ;

        //The next few lines use a rotation matrix to rotate the player's heading
        //vector accordingly
        Quaternion rotate = Quaternion.Euler(0, angle * sign * 180.0f / Mathf.PI  , 0);

        //notice how the direction of rotation has to be determined when creating
        //the rotation matrix

        m_vHeading  = rotate * m_vHeading;
        m_vVelocity = rotate * m_vVelocity;

        //finally recreate m_vSide
        SetHeading(m_vHeading);

        return false;
    }

    /// <summary>
    ///  first checks that the given heading is not a vector of zero length. If the
    ///  new heading is valid this function sets the entity's heading and side 
    ///  vectors accordingly
    /// </summary>
    public virtual void SetHeading(Vector3 new_heading)
    {
        if (new_heading.magnitude > 0)
        {
            new_heading.y = 0;
            m_vHeading = new_heading.normalized;

            //the side vector must always be perpendicular to the heading
            m_vSide = Vector3.Cross(Vector3.up, m_vHeading);
        } 

    }

    public void OnDebugRender()
    {
        if(m_vHeading.magnitude > 0)
        {
            DebugExtension.DrawArrow(m_vPos, m_vHeading * m_fBoundingRadius, Color.blue);
        }   
        
        if(m_vSide.magnitude > 0)
        {
            DebugExtension.DrawArrow(m_vPos, m_vSide * m_fBoundingRadius, Color.red);
        }   
        
        Gizmos.color = Color.black;

        DebugExtension.DrawCircle(m_vPos, m_fBoundingRadius);
    }

    public void OnDebugUpdateRender()
    {
        if (m_vHeading.magnitude > 0)
        {
            DebugExtension.DebugArrow(m_vPos, m_vHeading * m_fBoundingRadius, Color.blue);
        }

        if (m_vSide.magnitude > 0)
        {
            DebugExtension.DebugArrow(m_vPos, m_vSide * m_fBoundingRadius, Color.red);
        }

        Gizmos.color = Color.black;

        DebugExtension.DebugCircle(m_vPos, m_fBoundingRadius);
    }
}
