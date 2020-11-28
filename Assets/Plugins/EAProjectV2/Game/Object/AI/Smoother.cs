using System.Collections.Generic;
using UnityEngine;
using Debug = EAFrameWork.Debug;

public class Smoother
{
    //this holds the history
    private List<Vector3> m_History;

    private int m_iNextUpdateSlot;

    //an example of the 'zero' value of the type to be smoothed. This
    //would be something like Vector2D(0,0)
    private Vector3 m_ZeroValue = Vector3.zero;

    //to instantiate a Smoother pass it the number of samples you want
    //to use in the smoothing, and an exampe of a 'zero' type
    public Smoother(int SampleSize, Vector3 ZeroValue)
    {
        m_History = new List<Vector3>(SampleSize);

        for (int i = 0; i < SampleSize; i++)
        {
            m_History.Add(ZeroValue);
        }  
            
        m_ZeroValue = ZeroValue;
        m_iNextUpdateSlot = 0;
    }

    //each time you want to get a new average, feed it the most recent value
    //and this method will return an average over the last SampleSize updates
    public Vector3 Update(Vector3 MostRecentValue)
    {
        Debug.Assert(m_iNextUpdateSlot < m_History.Count, "next update slot is fault count : " + m_iNextUpdateSlot);
        
        if(m_iNextUpdateSlot < m_History.Count)
        {
            //overwrite the oldest value with the newest
            m_History[m_iNextUpdateSlot++] = MostRecentValue;
        }
       
        //make sure m_iNextUpdateSlot wraps around. 
        if (m_iNextUpdateSlot >= m_History.Count)
           m_iNextUpdateSlot = 0;

        //now to calculate the average of the history list
        //c++ code make a copy here, I use Zero method instead.
        //Another approach could be creating public clone() method in Vector2D ...
        Vector3 sum = m_ZeroValue;
        sum = Vector3.zero;

        for(int i = 0; i < m_History.Count; ++i)
        {
            sum += m_History[i];
        }

        sum = sum / m_History.Count;

        return sum;
    }
}