using System;


public static class Prm
{
    public static int NumAgents =   100;
    public static int NumObstacles = 10;
    public static float MinObstacleRadius = 15.0f;
    public static float MaxObstacleRadius = 25.0f;

    

    //number of horizontal cells used for spatial partitioning
    public static int NumCellsX = 10;
    //number of vertical cells used for spatial partitioning
    public static int NumCellsY = 10;
    //how many samples the smoother will use to average a value
    public static int NumSamplesForSmoothing;
    //used to tweak the combined steering force (simply altering the MaxSteeringForce
    //will NOT work! This tweaker affects all the steering force multipliers
    //too).
    public static float SteeringForceTweaker;
    public static float MaxSteeringForce;
    public static float MaxSpeed;
    public static float VehicleMass;
    public static float VehicleScale;
    public static float MaxTurnRatePerSecond;
    public static float SeparationWeight;
    public static float AlignmentWeight = 10.0f;
    public static float CohesionWeight = 10.0f;
    public static float ObstacleAvoidanceWeight = 10.0f;
    public static float WallAvoidanceWeight;
    public static float WanderWeight = 10.0f;
    public static float SeekWeight = 10.0f;
    public static float FleeWeight = 10.0f;
    public static float ArriveWeight = 10.0f;
    public static float PursuitWeight;
    public static float OffsetPursuitWeight;
    public static float InterposeWeight;
    public static float HideWeight;
    public static float EvadeWeight;
    public static float FollowPathWeight = 10.0f;
    //how close a neighbour must be before an agent perceives it (considers it
    //to be within its neighborhood)
    public static float ViewDistance = 50;
    //used in obstacle avoidance
    public static float MinDetectionBoxLength = 20.0f;
    //used in wall avoidance
    public static float WallDetectionFeelerLength;
    //these are the probabilities that a steering behavior will be used
    //when the prioritized dither calculate method is used
    public static float prWallAvoidance;
    public static float prObstacleAvoidance;
    public static float prSeparation;
    public static float prAlignment;
    public static float prCohesion;
    public static float prWander;
    public static float prSeek;
    public static float prFlee;
    public static float prEvade;
    public static float prHide;
    public static float prArrive;
}