using System.Collections.Generic;

public interface IObject
{
    uint GetIdx();
    void SetIdx(uint idx);
}

public interface IObjectFactory<T> where T : IObject
{
    T CreateObject(uint idx);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class GeneralPool<T> where T : IObject
{
    Dictionary<uint, T> m_list = new Dictionary<uint, T>();
    Queue<T> m_Queue = new Queue<T>();
    protected uint m_nIDMaxCount = 0;
    IObjectFactory<T> m_factory = null;

    public GeneralPool(IObjectFactory<T> factory , uint nIDMaxCount = 50)
    {
        m_factory     = factory;
        m_nIDMaxCount = nIDMaxCount;

        ReGenerate();
    }

    public void ReGenerate()
    {
        m_Queue.Clear();

        for (uint i = 1; i <= m_nIDMaxCount; ++i)
        {
            T obj = m_factory.CreateObject(i);
            m_Queue.Enqueue(obj);
        }

        m_list.Clear();
    }

    public T Create()
    {
        if (m_Queue.Count <= 0)
        {
            m_nIDMaxCount = m_nIDMaxCount + 1;
            m_Queue.Enqueue(m_factory.CreateObject(m_nIDMaxCount));
        }

        T obj = m_Queue.Dequeue();
        m_list.Add(obj.GetIdx(), obj);

        return obj;
    }

    public void Free(T obj)
    {
        T _obj;

        if (m_list.TryGetValue(obj.GetIdx() , out _obj) == true)
        {
            m_list.Remove(obj.GetIdx());
            m_Queue.Enqueue(obj);
        }
    }
}