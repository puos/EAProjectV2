using System;
using System.Collections.Generic;

public class EARandom
{
    public int ValueMin = 0;                                 // Minimum value
    public int ValueMax = 0;                                 // Maximum value

    public EARandom(int min, int max)
    {
        ValueMin = min;
        ValueMax = max;
        InitRandom(ValueMin, ValueMax);
    }

    int LastValue = int.MaxValue;                     // 
    List<int> m_pValue = new List<int>();
    List<int> Value
    {
        get { return m_pValue; }
        set { m_pValue = value; }
    }

    public void InitRandom(int min, int max)       // Store the last random value
    {
        ValueMin = min;
        ValueMax = max;
        m_pValue.Clear();
        for (int i = min; i < max; i++)             // From minimum to maximum entered. Maximum does not include. ex] (0, 10) => output from 0 to 9
        {
            m_pValue.Add(i);
        }
        if (LastValue < int.MaxValue)
        {
            m_pValue.Remove(LastValue);      // Subtract the last value
        }
   
    }

    public void Reset()
    {
        m_pValue.Clear();
        LastValue = int.MaxValue;                   // So that the last value is included in the list
        InitRandom(ValueMin, ValueMax);
    }

    public void delItem(int nValue)
    {
        if (Value != null && Value.Count != 0)
        {
            Value.Remove(nValue);

            // Reinitialize if list is empty
            if (Value.Count == 0)                   
            {
                InitRandom(ValueMin, ValueMax);      
            }
        }
    }

    /// <summary>
    /// random function
    /// </summary>
    /// <returns></returns>
    public int Random()                         
    {
             
        Random random = new Random(Guid.NewGuid().GetHashCode());

        int List_Rndnum = random.Next(0, Value.Count);
     
        LastValue = Value[List_Rndnum];

        // Delete the value. Duplicate output protection.
        m_pValue.Remove(Value[List_Rndnum]);

        // Reinitialize if list is empty
        if (Value.Count == 0)                   
        {
            InitRandom(ValueMin, ValueMax);      
            m_pValue.Add(LastValue);
        }
               
        return LastValue;
    }

}