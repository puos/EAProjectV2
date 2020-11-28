using System.Collections.Generic;
using UnityEngine;
using System;
using Debug = EAFrameWork.Debug;

public class SteeringBehaviour 
{
    float wayPointSeekDist = 1.0f;

    AIAgent m_entity = null;

    public enum summing_method
    {
        weighted_average = 0,
        prioritized = 1,
        dithered = 2,
    }

    [Flags]
    private enum behaviour_type
    {
        none    = 1 << 0,
        seek    = 1 << 1,     
        flee    = 1 << 2,
        arrive  = 1 << 3,
        wander  = 1 << 4,
        cohesion = 1 << 5, 
        separation = 1 << 6,
        allignment = 1 << 7,
        obstacle_avoidance = 1 << 8,
        wall_avoidance = 1 << 9,
        follow_path = 1 << 10,
        pursuit = 1 << 11,
        evade   = 1 << 12,
        interpose = 1 << 13, 
        hide      = 1 << 14,
        flock     = 1 << 15,
        offset_pursuit = 1 << 16,
        gravity   = 1 << 17,
    }
   

    Vector3 steeringForce = Vector3.zero;

    //these can be used to keep track of friends, pursuers, or prey
    private AIAgent m_pTargetAgent1 = null;
    private AIAgent m_pTargetAgent2 = null;

    float m_fDBoxLength;
    
    //a vertex buffer to contain the feelers rqd for wall avoidance  
    private List<Vector3> m_Feelers = new List<Vector3>();

    float m_fWallDetectionFeelerLength = 0;

    Vector3 m_vWanderTarget;

    float m_fWanderJitter = 50.0f;
    float m_fWanderRadius = 10.0f;
    float m_fWanderDistance = 25.0f;

    //multipliers. These can be adjusted to effect strength of the  
    //appropriate behavior. Useful to get flocking the way you require
    //for example.
    private float m_fWeightSeparation = Prm.SeparationWeight;
    private float m_fWeightCohesion = Prm.CohesionWeight;
    private float m_fWeightAlignment = Prm.AlignmentWeight;
    private float m_fWeightWander = Prm.WanderWeight;
    private float m_fWeightObstacleAvoidance = Prm.ObstacleAvoidanceWeight;
    private float m_fWeightWallAvoidance;
    private float m_fWeightSeek = Prm.SeekWeight;
    private float m_fWeightFlee = Prm.FleeWeight;
    private float m_fWeightArrive = Prm.ArriveWeight;
    private float m_fWeightPursuit;
    private float m_fWeightOffsetPursuit;
    private float m_fWeightInterpose = 0;
    private float m_fWeightHide;
    private float m_fWeightEvade;
    private float m_fWeightFollowPath = Prm.FollowPathWeight;

    private float prObstacleAvoidance = 0;

    //how far the agent can 'see'
    private float m_fViewDistance = Prm.ViewDistance;
    //pointer to any current path
    private AIPath m_pPath = new AIPath();

    private float m_fWaypointSeekDistSq;

    //any offset used for formations or offset pursuit
    private Vector3 m_vOffset;

    behaviour_type m_iflag = behaviour_type.none;

    // Gravity constant
    private float m_fGravityConstant = 4.9f;

    // Landing height
    private float m_fheight = 0;

    private enum Deceleration
    {
        fast = 1,
        normal = 2,
        slow = 3,
    }

    Deceleration m_deceleration = Deceleration.normal;

    float minDetectionBoxLength = Prm.MinDetectionBoxLength;

    summing_method summingMethod = summing_method.weighted_average;

    public SteeringBehaviour(AIAgent _entity)
    {
        m_entity = _entity;

        m_fWaypointSeekDistSq = wayPointSeekDist * wayPointSeekDist;

        Debug.Assert(m_entity != null, "entity is null");
    }
        
    public void SetWanderDistance(float fWanderDistance)
    {
        m_fWanderDistance = fWanderDistance;
    } 

    public void SetWaypointSeeKDist(float _wayPointSeekDist)
    {
        wayPointSeekDist      = _wayPointSeekDist;
        m_fWaypointSeekDistSq = wayPointSeekDist * wayPointSeekDist;
    }

    public void SetLandHeight(float height)
    {
        m_fheight = height;
    }

    public float GetLandHeight()
    {
        return m_fheight;
    }


    public void SetPath(List<Vector3> paths,bool isLoop = false)
    {
        m_pPath.Set(paths);

        if(isLoop == true)
        {
            m_pPath.LoopOn();
        }  
        else
        {
            m_pPath.LoopOff();
        }  
    }

    bool On(behaviour_type bType) { return ((m_iflag & bType) == bType); }

    bool On(behaviour_type bType, behaviour_type iflag) { return ((iflag & bType) == bType); }

    void Off(behaviour_type bType)
    {
        if (On(bType))
        {
            m_iflag ^= bType;
        }
    }

    behaviour_type Off(behaviour_type bType , behaviour_type iflag)
    {
        if (On(bType , iflag))
        {
            iflag ^= bType;
        }

        return iflag;
    }


    public void BasicOn()
    {
        m_iflag |= behaviour_type.obstacle_avoidance;
        m_iflag |= behaviour_type.wall_avoidance;
    }


    public void CohesionOn()
    {
        m_iflag |= behaviour_type.cohesion;
    }

    public void FleeOn()
    {
        m_iflag |= behaviour_type.flee;
    }

    public void SeekOn()
    {
        m_iflag |= behaviour_type.seek;
    }

    public void ArriveOn()
    {
        m_iflag |= behaviour_type.arrive;
    }

    public void WanderOn()
    {
        m_iflag |= behaviour_type.wander;
    }

    public void AlignmentOn()
    {
        m_iflag |= behaviour_type.allignment;
    }

    public void FollowPathOn()
    {
        m_iflag |= behaviour_type.follow_path;
    }

    public void GravityOn()
    {
        m_iflag |= behaviour_type.gravity;
    }

    public void SetSummingMethod(summing_method sm)
    {
        summingMethod = sm;
    }

    public void CohesionOff()
    {
       Off(behaviour_type.cohesion);
    }

    public void ArriveOff()
    {
        Off(behaviour_type.arrive);
    }

    public void WanderOff()
    {
        Off(behaviour_type.wander);
    }

    public void AlignmentOff()
    {
        Off(behaviour_type.allignment);
    }

    public void FleeOff()
    {
        Off(behaviour_type.allignment);
    }

    public void FollowPathOff()
    {
        Off(behaviour_type.follow_path);
    }

    public bool IsSteering()
    {
        behaviour_type clongFlag = m_iflag;

        clongFlag = Off(behaviour_type.obstacle_avoidance, clongFlag);
        clongFlag = Off(behaviour_type.wall_avoidance, clongFlag);

        return ((int)clongFlag > 1) ? true : false;
    }


    bool AccumulateForce(ref Vector3 runningTot , Vector3 forceToAdd)
    {
        float magnitudeSoFar = runningTot.magnitude;

        float MagnitudeRemaining = m_entity.MaxForce() - magnitudeSoFar;

        //return false if there is no more force left to use
        if (MagnitudeRemaining <= 0.0)
        {
            return false;
        }

        //calculate the magnitude of the force we want to add
        float MagnitudeToAdd = forceToAdd.magnitude;

        //if the magnitude of the sum of ForceToAdd and the running total
        //does not exceed the maximum force available to this vehicle, just
        //add together. Otherwise add as much of the ForceToAdd vector is
        //possible without going over the max.
        if (MagnitudeToAdd < MagnitudeRemaining)
        {
            runningTot = forceToAdd;
        }
        else
        {
            runningTot = forceToAdd.normalized * MagnitudeRemaining;
            //add it to the steering force
        }

        return true;
    }

    /**
     *  Creates the antenna utilized by WallAvoidance
     */
    private void CreateFeelers()
    {
        m_Feelers.Clear();
        //feeler pointing straight in front
        m_Feelers.Add( m_entity.VPos() + m_entity.Heading() * m_fWallDetectionFeelerLength );

        //feeler to left
        Vector3 temp = m_entity.Heading();

        Quaternion rotate = Quaternion.Euler(0,  90.0f , 0);

        Vector3 temp2 = rotate * temp;

        m_Feelers.Add(m_entity.VPos() + temp2 * m_fWallDetectionFeelerLength / 2.0f);

        rotate = Quaternion.Euler(0, 45.0f, 0);

        temp2 = rotate * temp;

        //feeler to right
        m_Feelers.Add(m_entity.VPos() + temp2 * m_fWallDetectionFeelerLength / 2.0f);
    }

    /**
    * Given a target, this behavior returns a steering force which will
    *  direct the agent towards the target
    */
    private Vector3 Seek(Vector3 TargetPos)
    {
        Vector3 desiredVelocity = (TargetPos - m_entity.VPos()).normalized * m_entity.MaxSpeed();

        return desiredVelocity - m_entity.Velocity();
    }

    /**
    *   Move along the specified path                                            
    */
    private Vector3 FindRoute(Vector3 TargetPos)
    {
        Vector3 ToTarget = TargetPos - m_entity.VPos();

        //calculate the distance to the target
        float dist = ToTarget.magnitude;

        //because Deceleration is enumerated as an int, this value is required
        //to provide fine tweaking of the deceleration..
        float DecelerationTweaker = 0.3f;

        //calculate the speed required to reach the target given the desired
        //deceleration
        float speed = dist / (0.5f * DecelerationTweaker);

        //make sure the velocity does not exceed the max
        speed = Mathf.Min(speed, m_entity.MaxSpeed());

        //from here proceed just like Seek except we don't need to normalize 
        //the ToTarget vector because we have already gone to the trouble
        //of calculating its length: dist. 
        // speed  = distance / time    
        // arrival time = (arrival speed/arrival distance)

        Vector3 DesiredVelocity = ToTarget * (speed / dist);

        return DesiredVelocity - m_entity.Velocity();
    }

    private Vector3 Flee(Vector3 TargetPos)
    {
        Vector3 desiredVelocity = (m_entity.VPos() - TargetPos).normalized * m_entity.MaxSpeed();
                   
        return desiredVelocity - m_entity.Velocity();
    }

    /**
     * This behavior is similar to seek but it attempts to arrive at the
     *  target with a zero velocity
     */
    private Vector3 Arrive(Vector3 TargetPos, Deceleration deceleration)
    {
        Vector3 ToTarget = TargetPos - m_entity.VPos();

        //calculate the distance to the target
        float dist = ToTarget.magnitude;

        if (dist > 0)
        {
            //because Deceleration is enumerated as an int, this value is required
            //to provide fine tweaking of the deceleration..
            float DecelerationTweaker = 0.3f;

            //calculate the speed required to reach the target given the desired
            //deceleration
            float speed = dist / ((float)deceleration * DecelerationTweaker);

            //make sure the velocity does not exceed the max
            speed = Mathf.Min(speed, m_entity.MaxSpeed());

            //from here proceed just like Seek except we don't need to normalize 
            //the ToTarget vector because we have already gone to the trouble
            //of calculating its length: dist. 
            Vector3 DesiredVelocity = ToTarget * (speed / dist);

            return DesiredVelocity - m_entity.Velocity();
        }

        return Vector3.zero;
    }

    /**
    *  this behavior creates a force that steers the agent towards the 
    *  evader
    */
    private Vector3 Pursuit(AIAgent evader)
    {
        //if the evader is ahead and facing the agent then we can just seek
        //for the evader's current position.
        Vector3 ToEvader = evader.VPos() - m_entity.VPos();

        float RelativeHeading = Vector3.Dot(m_entity.Heading(), evader.Heading());

        if (Vector3.Dot(ToEvader , m_entity.Heading()) > 0 && (RelativeHeading < -0.95)) //acos(0.95)=18 degs
        {
            return Seek(evader.VPos());
        }

        //Not considered ahead so we predict where the evader will be.

        //the lookahead time is propotional to the distance between the evader
        //and the pursuer; and is inversely proportional to the sum of the
        //agent's velocities
        float LookAheadTime = ToEvader.magnitude / (m_entity.MaxSpeed() + evader.Speed());

        //now seek to the predicted future position of the evader
        return Seek(evader.VPos() + evader.Velocity() * LookAheadTime);
    }

    /**
   *  similar to pursuit except the agent Flees from the estimated future
   *  position of the pursuer
   */
    private Vector3 Evade(AIAgent pursuer)
    {
        // Not necessary to include the check for facing direction this time

        Vector3 ToPursuer = pursuer.VPos() - m_entity.VPos();

        //uncomment the following two lines to have Evade only consider pursuers 
        //within a 'threat range'
        float ThreatRange = 100.0f;
        if (ToPursuer.sqrMagnitude > ThreatRange * ThreatRange)
        {
            return Vector3.zero;
        }

        //the lookahead time is propotional to the distance between the pursuer
        //and the pursuer; and is inversely proportional to the sum of the
        //agents' velocities
        float LookAheadTime = ToPursuer.magnitude
                / (m_entity.MaxSpeed() + pursuer.Speed());

        //now flee away from predicted future position of the pursuer
        return Flee(pursuer.VPos() + pursuer.Velocity() * LookAheadTime);
    }


    /**
    * This behavior makes the agent wander about randomly
    */
    private Vector3 Wander()
    {
       
        //this behavior is dependent on the update rate, so this line must
        //be included when using time independent framerate.
        float JitterThisTimeSlice = m_fWanderJitter * m_entity.getTimeElapsed();

        //first, add a small random vector to the target's position
        m_vWanderTarget = m_vWanderTarget + new Vector3(MathUtil.RandomClamped() * JitterThisTimeSlice, 0.0f,
                MathUtil.RandomClamped() * JitterThisTimeSlice);

        //reproject this new vector back on to a unit circle
        m_vWanderTarget.Normalize();

        //increase the length of the vector to the same as the radius
        //of the wander circle
        m_vWanderTarget = m_vWanderTarget * m_fWanderRadius;

        //move the target into a position WanderDist in front of the agent
        Vector3 target = m_vWanderTarget + m_fWanderDistance * Vector3.forward;

        //project the target into world space
        target = MathUtil.PointToWorldSpace(target,
                m_entity.Heading(),
                m_entity.Side(),
                m_entity.VPos());

        //and steer towards it
        return target - m_entity.VPos();
    }
        
    private bool CheckNeighbors(AIAgent neighbor)
    {
        return (neighbor != m_entity) && neighbor.Tag && (neighbor != m_pTargetAgent1);
    }

    /**
    * this calculates a force repelling from the other neighbors
    */
    Vector3 Separation(List<AIAgent> neighbors)
    {
        Vector3 SteeringForce = Vector3.zero;

        for (int a = 0; a < neighbors.Count; ++a)
        {
            //make sure this agent isn't included in the calculations and that
            //the agent being examined is close enough. ***also make sure it doesn't
            //include the evade target ***
            if (CheckNeighbors(neighbors[a]))
            {
                Vector3 ToAgent = m_entity.VPos() - neighbors[a].VPos();

                //scale the force inversely proportional to the agents distance  
                //from its neighbor.
                SteeringForce += (ToAgent.normalized / ToAgent.magnitude);
            }
        }

        return SteeringForce;
    }

    /***
     * 
     * 
     *
     */
    public Vector3 FreeFall()
    {
        Vector3 SteeringForce = Vector3.zero;

        if(m_entity.VPos().y > m_fheight)
        {
            SteeringForce = m_fGravityConstant * Vector3.down;
        }
      
        return SteeringForce;
    }

    /*
    /**
     * returns a force that attempts to align this agents heading with that
     * of its neighbors
     */
    private Vector3 Alignment(List<AIAgent> neighbors)
    {
        //used to record the average heading of the neighbors
        Vector3 AverageHeading = Vector3.zero;

        //used to count the number of vehicles in the neighborhood
        int NeighborCount = 0;

        //iterate through all the tagged vehicles and sum their heading vectors  
        for (int a = 0; a < neighbors.Count; ++a)
        {
            //make sure *this* agent isn't included in the calculations and that
            //the agent being examined  is close enough ***also make sure it doesn't
            //include any evade target ***
            if (CheckNeighbors(neighbors[a]))
            {
                AverageHeading += neighbors[a].Heading();

                ++NeighborCount;
            }
        }

        //if the neighborhood contained one or more vehicles, average their
        //heading vectors.
        if (NeighborCount > 0)
        {
            AverageHeading = AverageHeading / NeighborCount;
        }

        return AverageHeading;
    }

    /**
    * This returns a steering force that will keep the agent away from any
    *  walls it may encounter
    */
    private Vector3 WallAvoidance(List<Wall> walls)
    {
        //the feelers are contained in a std::vector, m_Feelers
        CreateFeelers();

        float DistToThisIP = 0f;
        float DistToClosestIP = float.MaxValue;

        //this will hold an index into the vector of walls
        int ClosestWall = -1;

        Vector3 SteeringForce = Vector3.zero,
                point = Vector3.zero, //used for storing temporary info
                ClosestPoint = Vector3.zero;  //holds the closest intersection point

        //examine each feeler in turn
        for (int flr = 0; flr < m_Feelers.Count; ++flr)
        {
            //run through each wall checking for any intersection points
            for (int w = 0; w < walls.Count; ++w)
            {
                bool check = MathUtil.LineIntersectionXZ(m_entity.VPos(),
                        m_Feelers[flr],
                        walls[w].From(),
                        walls[w].To(), out DistToThisIP);

                if (check)
                {
                    //is this the closest found so far? If so keep a record
                    if (DistToThisIP < DistToClosestIP)
                    {
                        DistToClosestIP = DistToThisIP;

                        ClosestWall = w;

                        ClosestPoint = point;
                    }
                }
            }//next wall


            //if an intersection point has been detected, calculate a force  
            //that will direct the agent away
            if (ClosestWall >= 0)
            {
                //calculate by what distance the projected position of the agent
                //will overshoot the wall
                Vector3 OverShoot = m_Feelers[flr] - ClosestPoint;

                //create a force in the direction of the wall normal, with a 
                //magnitude of the overshoot
                SteeringForce = walls[ClosestWall].Normal() * OverShoot.magnitude;
            }

        }//next feeler

        return SteeringForce;
    }

    /**
     * returns a steering force that attempts to move the agent towards the
     * center of mass of the agents in its immediate area
     */
    private Vector3 Cohesion(List<AIAgent> neighbors)
    {
        //first find the center of mass of all the agents
        Vector3 centerOfMass = Vector3.zero;
        Vector3 steeringForce = Vector3.zero;

        int NeighborCount = 0;

        //iterate through the neighbors and sum up all the position vectors
        for (int a = 0; a < neighbors.Count; ++a)
        {
            //make sure *this* agent isn't included in the calculations and that
            //the agent being examined is close enough ***also make sure it doesn't
            //include the evade target ***
            if (CheckNeighbors(neighbors[a]))
            {
                centerOfMass += neighbors[a].VPos();

                ++NeighborCount;
            }
        }

        if (NeighborCount > 0)
        {
            //the center of mass is the average of the sum of positions
            centerOfMass = centerOfMass / (float)NeighborCount; 

            //now seek towards that position
            steeringForce = Seek(centerOfMass);
        }

        //the magnitude of cohesion is usually much larger than separation or
        //allignment so it usually helps to normalize it.
        return steeringForce.normalized;
    }
    
    /**
    *  Given a vector of obstacles, this method returns a steering force
    *  that will prevent the agent colliding with the closest obstacle
    */
    private Vector3 ObstacleAvoidance(List<AIObject> obstacles)
    {
        //*
        //the detection box length is proportional to the agent's velocity
        m_fDBoxLength = minDetectionBoxLength
                + (m_entity.Speed() / m_entity.MaxSpeed())
                * minDetectionBoxLength;

        //tag all obstacles within range of the box for processing
        m_entity.World().TagObstaclesWithinViewRange(m_entity, m_fDBoxLength);

        //this will keep track of the closest intersecting obstacle (CIB)
        AIObject ClosestIntersectingObstacle = null;

        //this will be used to track the distance to the CIB
        float DistToClosestIP = float.MaxValue;

        //this will record the transformed local coordinates of the CIB
        Vector3 LocalPosOfClosestObstacle = Vector3.zero;

        IEnumerator<AIObject> it = obstacles.GetEnumerator();

        for(int i = 0; i < obstacles.Count; ++i)
        {
            //if the obstacle has been tagged within range proceed
            AIObject curOb = obstacles[i];

            if (curOb.Tag)
            {
                //calculate this obstacle's position in local space
                Vector3 LocalPos = MathUtil.PointToLocalSpace(curOb.VPos(),
                        m_entity.Heading(),
                        m_entity.Side(),
                        m_entity.VPos());


                // entity Heading and LocalPos is 
                if (Vector3.Dot(LocalPos, Vector3.forward) > 0)
                {

                    //than its radius + half the width of the detection box then there
                    //is a potential intersection.
                    float ExpandedRadius = curOb.BRadius() + m_entity.BRadius();

                    if (Mathf.Abs(LocalPos.x) < ExpandedRadius)
                    {
                        //test to see if this is the closest so far. If it is keep a
                        //record of the obstacle and its local coordinates
                        if (LocalPos.magnitude < DistToClosestIP)
                        {
                            DistToClosestIP = LocalPos.magnitude;

                            ClosestIntersectingObstacle = curOb;

                            LocalPosOfClosestObstacle = LocalPos;
                        }
                    }
                }
            }
        } 
        
        //if we have found an intersecting obstacle, calculate a steering 
        //force away from it
        Vector3 steeringForce = Vector3.zero;

        if (ClosestIntersectingObstacle != null)
        {
            //should be
            //the closer the agent is to an object, the stronger the 
            //steering force should be
            float multiplier = 1.0f + (m_fDBoxLength - LocalPosOfClosestObstacle.z)
                    / m_fDBoxLength;

            //calculate the lateral force
            steeringForce.x = (ClosestIntersectingObstacle.BRadius()
                    - LocalPosOfClosestObstacle.x) * multiplier;

            //apply a braking force proportional to the obstacles distance from
            //the vehicle. 
            float BrakingWeight = 0.2f;

            steeringForce.z = (ClosestIntersectingObstacle.BRadius()
                   - LocalPosOfClosestObstacle.z)
                   * BrakingWeight;
        }

         //finally, convert the steering vector from local to world space
        return MathUtil.VectorToWorldSpace(steeringForce,
                m_entity.Heading(),
                m_entity.Side());

    }
    
    /**
    * Given two agents, this method returns a force that attempts to 
    * position the vehicle between them
    */
    private Vector3 Interpose(AIAgent AgentA, AIAgent AgentB)
    {
        //first we need to figure out where the two agents are going to be at 
        //time T in the future. This is approximated by determining the time
        //taken to reach the mid way point at the current time at at max speed.
        Vector3 MidPoint = (AgentA.VPos() + AgentB.VPos()) / 2.0f;

        float TimeToReachMidPoint = (MidPoint - m_entity.VPos()).sqrMagnitude / m_entity.MaxSpeed();

        //now we have T, we assume that agent A and agent B will continue on a
        //straight trajectory and extrapolate to get their future positions
        Vector3 aPos = AgentA.VPos() + (AgentA.Velocity() * TimeToReachMidPoint);
        Vector3 bPos = AgentB.VPos() + (AgentB.Velocity() * TimeToReachMidPoint);

        //calculate the mid point of these predicted positions
        MidPoint = (aPos + bPos) / 2.0f;

        //then steer to Arrive at it
        return Arrive(MidPoint, Deceleration.fast);
    }

    private Vector3 Hide(AIAgent hunter, List<AIObject> obstacles)
    {
        float DistToClosest = float.MaxValue;
        Vector3 BestHidingSpot = Vector3.zero;

        AIObject closest;

        for(int i = 0; i < obstacles.Count; ++i)
        {
            AIObject curOb = obstacles[i];

            Vector3 hidingSpot = GetHidingPosition(curOb.VPos(), curOb.BRadius(), hunter.VPos());

            float dist = (m_entity.VPos() - hidingSpot).sqrMagnitude;
            DistToClosest = dist;

            BestHidingSpot = hidingSpot;

            if (dist < DistToClosest)
            {

                closest = curOb;
            }
        }  

        //if no suitable obstacles found then Evade the hunter
        if (DistToClosest == float.MaxValue)
        {
            return Evade(hunter);
        }

        //else use Arrive on the hiding spot
        return Arrive(BestHidingSpot, Deceleration.fast);
    }

    /**
    *  Given the position of a hunter, and the position and radius of
    *  an obstacle, this method calculates a position DistanceFromBoundary 
    *  away from its bounding radius and directly opposite the hunter
    */
    private Vector3 GetHidingPosition(Vector3 posOb,
            float radiusOb, Vector3 posHunter)
    {
        //calculate how far away the agent is to be from the chosen obstacle's
        //bounding radius
        float DistanceFromBoundary = 30.0f;
        float DistAway = radiusOb + DistanceFromBoundary;

        //calculate the heading toward the object from the hunter
        Vector3 ToOb = (posOb - posHunter).normalized;

        //scale it to size and add to the obstacles position to get
        //the hiding spot.
        return (ToOb * DistAway) + posOb;
    }

    private Vector3 FollowPath()
    {
        //move to next target if close enough to current target (working in
        //distance squared space)
        Vector3 wayPoint = m_pPath.CurrentWaypoint();

        if ( (wayPoint - m_entity.VPos()).sqrMagnitude < m_fWaypointSeekDistSq)
        {
            m_pPath.SetNextWaypoint();
        }

        if (!m_pPath.Finished())
        {
            return FindRoute(m_pPath.CurrentWaypoint());
        }
        else
        {
            return Arrive(m_pPath.CurrentWaypoint(), Deceleration.normal);
        }
    }

    /////////////////////////////////////////////////////////////////////////////// CALCULATE METHODS 
    /**
     * calculates the accumulated steering force according to the method set
     *  in m_SummingMethod
     */
    public Vector3 Calculate()
    {
        //reset the steering force
        steeringForce = Vector3.zero;

        //tag neighbors if any of the following 3 group behaviors are switched on
        if (On(behaviour_type.separation) || On(behaviour_type.allignment) || On(behaviour_type.cohesion))
        {
            m_entity.World().TagAIAgentWithinViewRange(m_entity, m_fViewDistance);
        }


        switch (summingMethod)
        {

            case summing_method.weighted_average:

                steeringForce = CalculateWeightedSum();
                break;

            case summing_method.prioritized:

                steeringForce = CalculatePrioritized();
                break;

            case summing_method.dithered:

                steeringForce = CalculateDithered();
                break;

            default:
                steeringForce = Vector3.zero;
                break;

        }//end switch

        return steeringForce;
    }
    
    /**
     *  this simply sums up all the active behaviors X their weights and 
     *  truncates the result to the max available steering force before 
     *  returning
     */
    private Vector3 CalculateWeightedSum()
    {
        if (On(behaviour_type.wall_avoidance))
        {
            steeringForce += WallAvoidance(m_entity.World().Walls()) * m_fWeightWallAvoidance;
        }

        if (On(behaviour_type.obstacle_avoidance))
        {
            steeringForce += ObstacleAvoidance(m_entity.World().Obstacles()) * m_fWeightObstacleAvoidance;
        }

        if (On(behaviour_type.evade))
        {
            Debug.Assert(m_pTargetAgent1 != null, "Evade target not assigned");
            steeringForce += Evade(m_pTargetAgent1) * m_fWeightEvade;
        }


        //these next three can be combined for flocking behavior (wander is
        //also a good behavior to add into this mix)
       
        if (On(behaviour_type.separation))
        {
            steeringForce += Separation(m_entity.GetAIGroup().Agents()) * m_fWeightSeparation;
        }

        if (On(behaviour_type.allignment))
        {
            steeringForce += Alignment(m_entity.GetAIGroup().Agents()) * m_fWeightAlignment;
        }

        if (On(behaviour_type.cohesion))
        {
            steeringForce += Cohesion(m_entity.GetAIGroup().Agents()) * m_fWeightCohesion;
        }

        if (On(behaviour_type.wander))
        {
            steeringForce += Wander() * m_fWeightWander;
        }

        if (On(behaviour_type.seek))
        {
            steeringForce += Seek(m_entity.VTarget()) * m_fWeightSeek;
        }

        if (On(behaviour_type.flee))
        {
            steeringForce += Flee(m_entity.VTarget()) * m_fWeightFlee;
        }

        if (On(behaviour_type.arrive))
        {
            steeringForce += Arrive(m_entity.VTarget(), m_deceleration) * m_fWeightArrive;
        }

        if (On(behaviour_type.pursuit))
        {
            Debug.Assert(m_pTargetAgent1 != null, "pursuit target not assigned");

            steeringForce += Pursuit(m_pTargetAgent1) * m_fWeightPursuit;
        }

        if (On(behaviour_type.offset_pursuit))
        {
            Debug.Assert(m_pTargetAgent1 != null, "pursuit target not assigned");
            Debug.Assert(m_vOffset.magnitude > 0 ,"No offset assigned");

            //steeringForce += OffsetPursuit(m_pTargetAgent1, m_vOffset) * m_fWeightOffestPursuit;
        }

        if (On(behaviour_type.interpose))
        {
            Debug.Assert(m_pTargetAgent1 != null && m_pTargetAgent2 != null,"Interpose agents not assigned");

            steeringForce += Interpose(m_pTargetAgent1, m_pTargetAgent2) * m_fWeightInterpose;
        }

        if (On(behaviour_type.hide))
        {
            Debug.Assert(m_pTargetAgent1 != null,"Hide target not assigned");

            steeringForce += Hide(m_pTargetAgent1, m_entity.World().Obstacles());
        }

        if (On(behaviour_type.follow_path))
        {
            steeringForce += FollowPath() * m_fWeightFollowPath;
        }

        steeringForce = MathUtil.Truncate(steeringForce,m_entity.MaxForce());
        
        return steeringForce;
    }

    /**
     *  this method calls each active steering behavior in order of priority
     *  and acumulates their forces until the max steering force magnitude
     *  is reached, at which time the function returns the steering force 
     *  accumulated to that  point
     */
    private Vector3 CalculatePrioritized()
    {
        Vector3 force = Vector3.zero;

        if (On(behaviour_type.wall_avoidance))
        {
            force = WallAvoidance(m_entity.World().Walls()) * m_fWeightWallAvoidance;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }

        if (On(behaviour_type.obstacle_avoidance))
        {
            force = ObstacleAvoidance(m_entity.World().Obstacles()) * m_fWeightObstacleAvoidance;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }

        if (On(behaviour_type.evade))
        {
            Debug.Assert(m_pTargetAgent1 != null,"Evade target not assigned");

            force = Evade(m_pTargetAgent1) * m_fWeightEvade;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }


        if (On(behaviour_type.flee))
        {
            force = Flee(m_entity.VTarget()) * m_fWeightFlee;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }


        //these next three can be combined for flocking behavior (wander is
        //also a good behavior to add into this mix)
        if (On(behaviour_type.separation))
        {
            force = Separation(m_entity.GetAIGroup().Agents()) * m_fWeightSeparation;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }

        if (On(behaviour_type.allignment))
        {
            force = Alignment(m_entity.GetAIGroup().Agents()) * m_fWeightAlignment;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }

        if (On(behaviour_type.cohesion))
        {
            force = Cohesion(m_entity.GetAIGroup().Agents()) * m_fWeightCohesion;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }


        if (On(behaviour_type.seek))
        {
            force = Seek(m_entity.VTarget()) * m_fWeightSeek;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }


        if (On(behaviour_type.arrive))
        {
            force = Arrive(m_entity.VTarget(), m_deceleration) * m_fWeightArrive;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }

        if (On(behaviour_type.wander))
        {
            force = Wander() * m_fWeightWander;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }

        if (On(behaviour_type.pursuit))
        {
            Debug.Assert(m_pTargetAgent1 != null,"pursuit target not assigned");

            force = Pursuit(m_pTargetAgent1) * m_fWeightPursuit;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }

        if (On(behaviour_type.offset_pursuit))
        {
            //Debug.Assert(m_pTargetAgent1 != null  ,"pursuit target not assigned");
            //Debug.Assert(m_vOffset != Vector3.zero,"No offset assigned");

            //force = OffsetPursuit(m_pTargetAgent1, m_vOffset);

            //if (!AccumulateForce(ref steeringForce, force))
            //{
            //    return steeringForce;
            //}
        }

        if (On(behaviour_type.interpose))
        {
            Debug.Assert(m_pTargetAgent1 != null && m_pTargetAgent2 != null,"Interpose agents not assigned");

            force = Interpose(m_pTargetAgent1, m_pTargetAgent2) * m_fWeightInterpose;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }

        if (On(behaviour_type.hide))
        {
            Debug.Assert(m_pTargetAgent1 != null,"Hide target not assigned");

            force = Hide(m_pTargetAgent1, m_entity.World().Obstacles()) * m_fWeightHide;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }


        if (On(behaviour_type.follow_path))
        {
            force = FollowPath() * m_fWeightFollowPath;

            if (!AccumulateForce(ref steeringForce, force))
            {
                return steeringForce;
            }
        }

        return steeringForce;
    }

    /**
    *  this method sums up the active behaviors by assigning a probabilty
    *  of being calculated to each behavior. It then tests the first priority
    *  to see if it should be calculated this simulation-step. If so, it
    *  calculates the steering force resulting from this behavior. If it is
    *  more than zero it returns the force. If zero, or if the behavior is
    *  skipped it continues onto the next priority, and so on.
    *
    *  NOTE: Not all of the behaviors have been implemented in this method,
    *        just a few, so you get the general idea
    */
    private Vector3 CalculateDithered()
    {
        
        //reset the steering force
        steeringForce = Vector3.zero;

        if (On(behaviour_type.wall_avoidance) && MathUtil.RandFloat() < Prm.prWallAvoidance)
        {
            steeringForce += WallAvoidance(m_entity.World().Walls()) * (m_fWeightWallAvoidance / Prm.prWallAvoidance);

            if (!MathUtil.IsZero(steeringForce))
            {
                steeringForce = MathUtil.Truncate(steeringForce , m_entity.MaxForce());
                
                return steeringForce;
            }
        }

        if (On(behaviour_type.obstacle_avoidance) && MathUtil.RandFloat() < Prm.prObstacleAvoidance)
        {
            steeringForce += ObstacleAvoidance(m_entity.World().Obstacles()) * (m_fWeightObstacleAvoidance / Prm.prObstacleAvoidance);

            if (!MathUtil.IsZero(steeringForce))
            {
                steeringForce = MathUtil.Truncate(steeringForce, m_entity.MaxForce());

                return steeringForce;
            }
        }

        if (On(behaviour_type.separation) && MathUtil.RandFloat() < Prm.prSeparation)
        {
            steeringForce += Separation(m_entity.GetAIGroup().Agents()) * (m_fWeightSeparation / Prm.prSeparation);

            if (!MathUtil.IsZero(steeringForce))
            {
                steeringForce = MathUtil.Truncate(steeringForce, m_entity.MaxForce());

                return steeringForce;
            }
        }

       
        if (On(behaviour_type.flee) && MathUtil.RandFloat() < Prm.prFlee)
        {
             steeringForce += Flee(m_entity.VTarget()) * (m_fWeightFlee / Prm.prFlee);

            if (!MathUtil.IsZero(steeringForce))
            {
                steeringForce = MathUtil.Truncate(steeringForce, m_entity.MaxForce());

                return steeringForce;
            }
        }

        if (On(behaviour_type.evade) && MathUtil.RandFloat() < Prm.prEvade)
        {
            Debug.Assert(m_pTargetAgent1 != null,"Evade target not assigned");

            steeringForce += Evade(m_pTargetAgent1) * (m_fWeightEvade / Prm.prEvade);

            if (!MathUtil.IsZero(steeringForce))
            {
                MathUtil.Truncate(steeringForce , m_entity.MaxForce());
             
                return steeringForce;
            }
        }

        if (On(behaviour_type.allignment) && MathUtil.RandFloat() < Prm.prAlignment)
        {
            steeringForce += Alignment(m_entity.GetAIGroup().Agents()) * (m_fWeightAlignment / Prm.prAlignment);

            if (!MathUtil.IsZero(steeringForce))
            {
                steeringForce = MathUtil.Truncate(steeringForce, m_entity.MaxForce());

                return steeringForce;
            }
        }

        if (On(behaviour_type.cohesion) && MathUtil.RandFloat() < Prm.prCohesion)
        {
            steeringForce += Cohesion(m_entity.GetAIGroup().Agents()) * (m_fWeightCohesion / Prm.prCohesion);

            if (!MathUtil.IsZero(steeringForce))
            {
                steeringForce = MathUtil.Truncate(steeringForce, m_entity.MaxForce());

                return steeringForce;
            }
        }


        if (On(behaviour_type.wander) && MathUtil.RandFloat() < Prm.prWander)
        {
            steeringForce += Wander() * (m_fWeightWander / Prm.prWander);

            if (!MathUtil.IsZero(steeringForce))
            {
                steeringForce = MathUtil.Truncate(steeringForce, m_entity.MaxForce());

                return steeringForce;
            }
        }

        if (On(behaviour_type.seek) && MathUtil.RandFloat() < Prm.prSeek)
        {
            steeringForce += Seek(m_entity.VTarget()) * (m_fWeightSeek / Prm.prSeek); 

            if (!MathUtil.IsZero(steeringForce))
            {
                steeringForce = MathUtil.Truncate(steeringForce, m_entity.MaxForce());

                return steeringForce;
            }
        }

        if (On(behaviour_type.arrive) && MathUtil.RandFloat() < Prm.prArrive)
        {
           steeringForce += Arrive(m_entity.VTarget(), m_deceleration) * (m_fWeightArrive / Prm.prArrive);

            if (!MathUtil.IsZero(steeringForce))
            {
                steeringForce = MathUtil.Truncate(steeringForce, m_entity.MaxForce());

                return steeringForce;
            }
        }

        return steeringForce;
    }

    public Vector3 GravityAccel()
    {
        Vector3 acceleration = Vector3.zero;

        if(On(behaviour_type.gravity))
        {
            acceleration = FreeFall();
        }

        return acceleration;
    }

    public float GravityTruncate(float height)
    {
        if (On(behaviour_type.gravity))
        {
            height = Mathf.Max(height, GetLandHeight());
        }

        return height;
    }

    public void OnDebugRender()
    {
        if (On(behaviour_type.wander))
        {
            Vector3 m_vTCC = MathUtil.PointToWorldSpace(m_fWanderDistance * Vector3.forward,
                   m_entity.Heading(),
                   m_entity.Side(),
                   m_entity.VPos());
                        
            //draw the wander circle
            DebugExtension.DrawCircle(m_vTCC, Color.green, m_fWanderRadius);

            Vector3 target = (m_vWanderTarget + m_fWanderDistance * Vector3.forward);

            target = MathUtil.PointToWorldSpace(
                    target,
                    m_entity.Heading(),
                    m_entity.Side(),
                    m_entity.VPos());

            DebugExtension.DrawLineArrow(m_entity.VPos(), target);

            DebugExtension.DrawCircle(target, Color.red , 3);
        }

        if (On(behaviour_type.arrive) || On(behaviour_type.flee))
        {
            DebugExtension.DrawLineArrow(m_entity.VPos(), m_entity.VTarget());
            DebugExtension.DrawCircle(m_entity.VTarget(), Color.red, 3);
        }

        if(On(behaviour_type.follow_path))
        {
            m_pPath.OnDebugRender();
        }
        
        if(On(behaviour_type.obstacle_avoidance))
        {
            float length = Prm.MinDetectionBoxLength
                    + (m_entity.Speed() / m_entity.MaxSpeed())
                    * Prm.MinDetectionBoxLength;

            Matrix4x4 matTransform = Matrix4x4.TRS(m_entity.VPos(), Quaternion.LookRotation(m_entity.Heading(), Vector3.up), Vector3.one);

            Vector3 size = Vector3.zero;
            size.x = m_entity.BRadius();
            size.y = 0f;
            size.z = length;
            Vector3 center = Vector3.zero;
            center.x = 0f;
            center.y = 0f;
            center.z = length * 0.5f;

            DebugExtension.DrawLocalCube(matTransform, size, Color.black, center);

        }
    } 

}
