using System.Collections.Generic;
using UnityEngine;


public class AIGroup
{
    private Vector3 m_vCrosshair = Vector3.zero;

    private List<AIAgent> m_aiAgents = new List<AIAgent>();

    private string name = "";

    public AIGroup(string id = "basic")
    {
        name = id;
        m_vCrosshair = Vector3.zero;
    }

    public List<AIAgent> Agents()
    {
        return m_aiAgents;
    }


    public Vector3 Crosshair()
    {
        return m_vCrosshair;
    }

    public void SetCrosshair(Vector3 p)
    {
        m_vCrosshair = p;

        for(int i = 0;i < m_aiAgents.Count; ++i)
        {
            m_aiAgents[i].SetVTarget(p);
        }   
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="agent"></param>
    public void AddAgent(AIAgent agent)
    {
        int idx = m_aiAgents.FindIndex(x => x.GetHashCode().Equals(agent.GetHashCode()));

        if (idx == -1)
        {
            m_aiAgents.Add(agent);
            agent.SetAIGroup(this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="agent"></param>
    public void RemoveAgent(AIAgent agent)
    {
        m_aiAgents.Remove(agent);
    }
} 


public class GameWorld
{
    static GameWorld _instance = null;

    public static GameWorld instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new GameWorld();
            }

            return _instance;
        }
    }

    private List<AIObject> m_Obstacles = new List<AIObject>();

    private Dictionary<int , AIGroup> m_aiGroup = new Dictionary<int , AIGroup>();

    private List<Wall> m_walls = new List<Wall>();

    private CellSpacePartition<SteeringBehaviour> m_pCellSpace = null;

    Matrix4x4 space;

    public float cxClient
    {
        get; private set;
    }

    public float cyClient
    {
        get; private set;
    }

    public List<Wall> Walls()
    {
        return m_walls;
    } 

    public List<AIObject> Obstacles()
    {
        return m_Obstacles;
    }

    public void OpenWorld(float cx, float cy , float angle = 0, int cellX = 0 , int cellY = 0 , int numAgents = 0)
    {
        cxClient = cx;
        cyClient = cy;

        Quaternion rotation = Quaternion.Euler(0 , angle , 0);
        Vector3 position = Vector3.zero;
        position.x = -1 * cxClient * 0.5f;
        position.z = -1 * cyClient * 0.5f;

        space = Matrix4x4.TRS(position, rotation, Vector3.one);

        if (cellX > 0 && cellY > 0)
        {
            Prm.NumCellsX = cellX;
            Prm.NumCellsY = cellY;
            Prm.NumAgents = numAgents;

            //setup the spatial subdivision class
            m_pCellSpace = new CellSpacePartition<SteeringBehaviour>(cx, cy,
                    Prm.NumCellsX, Prm.NumCellsY, Prm.NumAgents);
        }  
     }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="agent"></param>
    public void AddAgent(string teamId , AIAgent agent)
    {
        int key = CRC32.GetHashForAnsi(teamId);

        AIGroup group = null;

        if(!m_aiGroup.TryGetValue(key, out group))
        {
            m_aiGroup.Add(key, new AIGroup(teamId));
        }

        RemoveAgent(agent);
        m_aiGroup[key].AddAgent(agent);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="agent"></param>
    public void RemoveAgent(AIAgent agent)
    {
        agent.GetAIGroup().RemoveAgent(agent);
    }

    /**
     * Sets up the vector of obstacles with random positions and sizes. Makes
     *  sure the obstacles do not overlap
     */
    public void CreateObstacles()
    {
        //create a number of randomly sized tiddlywinks
        for (int o = 0; o < Prm.NumObstacles; ++o)
        {
            bool bOverlapped = true;

            //keep creating tiddlywinks until we find one that doesn't overlap
            //any others.Sometimes this can get into an endless loop because the
            //obstacle has nowhere to fit. We test for this case and exit accordingly

            int NumTrys = 0;
            int NumAllowableTrys = 2000;

            while (bOverlapped)
            {
                NumTrys++;

                if (NumTrys > NumAllowableTrys)
                {
                    return;
                }
                int radius = MathUtil.RandInt((int)Prm.MinObstacleRadius, (int)Prm.MaxObstacleRadius);
                int border = 10;
                int MinGapBetweenObstacles = 20;

                ObStacle ob = new ObStacle(MathUtil.RandInt(radius + border, (int)(cxClient - radius - border)),
                                          MathUtil.RandInt(radius + border, (int)(cyClient - radius - 30 - border)),
                                          radius);

                if (!Overlapped((AIObject)ob, m_Obstacles, MinGapBetweenObstacles))
                {
                    //its not overlapped so we can add it
                    m_Obstacles.Add(ob);
                    bOverlapped = false;
                }
                else
                {
                    ob = null;
                }
            }
        }
    }

    void CreateWalls()
    {

    }

    
    public bool Overlapped(AIObject ob, List<AIObject> conOb, float MinDistBetweenObstacles)
    {
        IEnumerator<AIObject> it = conOb.GetEnumerator();

        while (it.MoveNext())
        {
            AIObject tmp = it.Current;

            if (MathUtil.TwoCirclesOverlapped(ob.VPos(),
                    ob.BRadius() + MinDistBetweenObstacles,
                    tmp.VPos(),
                    tmp.BRadius()))
            {
                return true;
            }
        }

        return false;
    }

    public void SetCrosshair(Vector3 p, string id = "basic")
    {
        Vector3 ProposedPosition = p;

        //make sure it's not inside an obstacle
        for(int i = 0; i < m_Obstacles.Count; ++i)
        {
            AIObject curOb = m_Obstacles[i];
            if (MathUtil.PointInCircle(curOb.VPos(), curOb.BRadius(), ProposedPosition))
            {
                return;
            }

        }

        AIGroup aiGroup = GetAIGroup(id);

        if (aiGroup != null)
        {
            aiGroup.SetCrosshair(p);
        }

    }

    public AIGroup GetAIGroup(string id)
    {
        int key = CRC32.GetHashForAnsi(id);

        AIGroup aiGroup = null;

        m_aiGroup.TryGetValue(key, out aiGroup);

        return aiGroup;
    }

    public void OnDebugRender()
    {
        for (int ob = 0; ob < m_Obstacles.Count ; ++ob)
        {
            DebugExtension.DrawCircle(m_Obstacles[ob].VPos(),Color.red , m_Obstacles[ob].BRadius());
        }

        if (m_pCellSpace != null)
        {
            m_pCellSpace.OnDebugRender();
        } 
        else
        {
            Vector3 size = Vector3.zero;

            size.x = cxClient;
            size.z = cyClient;

            Vector3 center = Vector3.zero;

            center.x = cxClient * 0.5f;
            center.z = cyClient * 0.5f;
            
            DebugExtension.DrawLocalCube(space, size , Color.blue , center);
        } 
    }

    public void TagObstaclesWithinViewRange(AIAgent entity, float radius)
    {
        IEnumerator<AIObject> it = m_Obstacles.GetEnumerator();

        while (it.MoveNext())
        {
            AIObject curEntity = it.Current;

            //first clear any current tag
            curEntity.UnTagging();

            Vector3 to = curEntity.VPos() - entity.VPos();

            //the bounding radius of the other is taken into account by adding it 
            //to the range
            float range = radius + curEntity.BRadius();

            //if entity within range, tag for further consideration. (working in
            //distance-squared space to avoid sqrts)
            if ((curEntity.GetHashCode() != entity.GetHashCode()) && 
                (to.sqrMagnitude < range * range))
            {
                curEntity.Tagging();
            }
        }
    }

    public void TagAIAgentWithinViewRange(AIAgent entity, float radius)
    {
        foreach (KeyValuePair<int,AIGroup> ai in m_aiGroup)
        {
            List<AIAgent> entities = ai.Value.Agents();

            for(int i = 0; i < entities.Count; ++i)
            {
                AIAgent curEntity = entities[i];

                //first clear any current tag
                curEntity.UnTagging();

                Vector3 to = curEntity.VPos() - entity.VPos();

                //the bounding radius of the other is taken into account by adding it 
                //to the range
                float range = radius + curEntity.BRadius();

                //if entity within range, tag for further consideration. (working in
                //distance-squared space to avoid sqrts)
                if ((curEntity.GetHashCode() != entity.GetHashCode()) && 
                    (to.sqrMagnitude < range * range))
                {
                    curEntity.Tagging();
                }
            }  
        }
    }
}