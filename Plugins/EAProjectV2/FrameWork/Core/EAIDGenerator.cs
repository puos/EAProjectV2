using System.Collections.Generic;


public class EAIDGenerator
{
    Dictionary<uint, uint> m_idList = new Dictionary<uint,uint>();
    Queue<uint> m_idQueue = new Queue<uint>();
    protected uint m_nIDMaxCount = 0;

    public EAIDGenerator(uint nIDMaxCount = 1000)
    {
        m_nIDMaxCount = nIDMaxCount;

        ReGenerate();
    }

    public void ReGenerate()
    {
        m_idQueue.Clear();

        for (uint i = 1; i <= m_nIDMaxCount; ++i)
        {
            m_idQueue.Enqueue(i);
        }

        m_idList.Clear();
    }
        
    public uint GenerateID()
    {
        if (m_idQueue.Count <= 0)
        {
            return 0;
        }

        uint id = m_idQueue.Dequeue();
        m_idList.Add(id, id);

        return id;
    }

    public void FreeID( uint id )
    {
	   if(m_idList.ContainsKey(id) == true)
       {
           m_idList.Remove(id);
           m_idQueue.Enqueue(id);
       }  
    }
}


