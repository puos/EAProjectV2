using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

[RequireComponent(typeof(CapsuleCollider))]
public class ActorMover : MonoBehaviour
{
    public AIAgent aiAgent = null;

    SteeringBehaviour steeringBehaviour = null;
    Smoother m_pHeadingSmoother         = null;

    public enum MoveCompleteState
    {
        Reached,
        Landed,
    }

    public delegate void OnMoveComplete(AIAgent aiAgent, SteeringBehaviour steeringBehaviour, MoveCompleteState state);

    public OnMoveComplete onMoveComplete = null;
   
    bool grounded = false;

    //this vector represents the average of the vehicle's heading
    //vector smoothed over the last few frames
    Vector3 m_vSmoothedHeading;

    bool bSmoothingOn = false;
    bool useRotation = true;
    bool useWrapAround = true;

    Vector3 gravityaccel_old;

    public bool Grounded
    {
        get
        {
            return grounded;
        }
    }

    public void SetPos(Vector3 vPos)
    {
        aiAgent.SetVPos(vPos);
    }

    public void SetVelocity(Vector3 velocity)
    {
        aiAgent.SetVelocity(velocity);
    }

    public void SetMaxSpeed(float max_speed)
    {
        aiAgent.SetMaxSpeed(max_speed);
    } 

    public void SetConstVelocity(Vector3 velocity)
    {
        aiAgent.SetConstVelocity(velocity);
    } 
    
    public void AddVelocity(Vector3 velocity)
    {
        aiAgent.SetAddVelocity(velocity);
    }
    
    public void UseRotation(bool useRotation)
    {
        this.useRotation = useRotation;
    }

    public void UseWrapAround(bool useWrapAround)
    {
        this.useWrapAround = useWrapAround;
    }

    public float GetMaxSpeed()
    {
        return aiAgent.MaxSpeed();
    } 

    public bool isSmoothingOn()
    {
        return bSmoothingOn;
    }

    public void SmoothingOn()
    {
        bSmoothingOn = true;
    }

    public void SmoothingOff()
    {
        bSmoothingOn = false;
    }

    public void ToggleSmoothing()
    {
        bSmoothingOn = !bSmoothingOn;
    }

    public void Create()
    {
        aiAgent = new AIAgent(); 
        steeringBehaviour   = new SteeringBehaviour(aiAgent);
        m_pHeadingSmoother  = new Smoother(Prm.NumSamplesForSmoothing, Vector3.zero);
    }

    public SteeringBehaviour Steering()
    {
        return steeringBehaviour;
    }

    public void SetAgent(Vector3 pos, float fRadius, Vector3 velocity, float fMaxSpeed, Vector3 heading,
           float mass, float turn_rate, float fMaxForce , uint objectId, string aigroup = "basic")
    {
        aiAgent.SetAgent(pos, fRadius, velocity, fMaxSpeed, Vector3.forward, mass , turn_rate , fMaxForce , objectId, aigroup);
    }

    public void SetWanderDistance(float wanderDistance)
    {
        steeringBehaviour.SetWanderDistance(wanderDistance);
    }

    public void SetWayPointSeekDistance(float wayPointSeekDistance)
    {
        steeringBehaviour.SetWaypointSeeKDist(wayPointSeekDistance);
    }

    public void SetPath(List<Vector3> paths,bool isLoop = false)
    {
        steeringBehaviour.SetPath(paths , isLoop);
    }

    public void SetLandHeight(float height)
    {
        steeringBehaviour.SetLandHeight(height);
    } 

    public void AIUpdate()
    {
        //update the time elapsed

        //keep a record of its old position so we can update its cell later
        //in this method
        Vector3 OldVelocity = aiAgent.Velocity();

        Vector3 SteeringForce = Vector3.zero;

        //calculate the combined force from each steering behavior in the 
        //vehicle's list
        SteeringForce = steeringBehaviour.Calculate();
                 
        //Acceleration = Force/Mass
        Vector3 acceleration = SteeringForce / aiAgent.Mass();
        Vector3 gravityaccel = steeringBehaviour.GravityAccel();

        grounded = (gravityaccel.magnitude > 0) ? true : false;

        aiAgent.Update(acceleration, gravityaccel, useWrapAround, Time.deltaTime);

        Vector3 vPos = aiAgent.VPos();

        vPos.y = steeringBehaviour.GravityTruncate(vPos.y);

        aiAgent.SetVPos(vPos);

        //EnforceNonPenetrationConstraint(this, World()->Agents());

        //update the vehicle's current cell if space partitioning is turned on
        if (isSmoothingOn())
        {
            m_vSmoothedHeading = m_pHeadingSmoother.Update(aiAgent.Heading());
        }

        //moveState
        if(steeringBehaviour.IsSteering())
        {
            float epsillon = 0.01f;

            if (MathUtil.Equal(aiAgent.Velocity(), Vector3.zero , epsillon ) && 
                OldVelocity.magnitude >= epsillon)
            {
                if(onMoveComplete != null)
                {
                    onMoveComplete(aiAgent, steeringBehaviour , MoveCompleteState.Reached);
                } 
            }
            else if(gravityaccel.magnitude <= 0 && 
                    gravityaccel_old.magnitude > 0)
            {
                if (onMoveComplete != null)
                {
                    onMoveComplete(aiAgent, steeringBehaviour, MoveCompleteState.Landed);
                }
            } 
        }

        gameObject.transform.position = aiAgent.VPos();

        if (useRotation)
        {
            Vector3 vDir = aiAgent.Heading();

            if (vDir.magnitude > 0)
            {
                gameObject.transform.rotation = Quaternion.LookRotation(vDir);
            }
        }
        
        Vector3 target = gameObject.transform.position + gameObject.transform.rotation * Vector3.forward * aiAgent.BRadius();

        gravityaccel_old = gravityaccel;

        DebugExtension.DebugCircle(target, 0.5f);
    }

    public void SetTarget(Vector3 vTarget , bool lookAt)
    {
        aiAgent.SetVTarget(vTarget);

        if(lookAt)
        {
           aiAgent.RotateHeadingToFacePosition(vTarget);
        }  
    }

    private void OnDestroy()
    {
        //Debug.Log("ActorMover Destroy :" + gameObject.name);
        GameWorld.instance.RemoveAgent(aiAgent);

        aiAgent = null;
        steeringBehaviour = null;
        m_pHeadingSmoother = null;
    }

    public void OnDebugRenderer()
    {
        
        aiAgent.OnDebugRender();
        

        Steering().OnDebugRender();
    }
    
}
