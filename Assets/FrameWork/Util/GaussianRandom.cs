using System;
using System.Collections.Generic;

public class GaussianRandom
{
    List<int> m_pValue;
    Random rand = new Random();

    public List<int> Value
    {
        get { return m_pValue; }
        set { m_pValue = value; }
    }

    public GaussianRandom()
    {
        // Value.Clear();
    }

    public void Refresh(double mu, double sigma, int count)
    {
        bool fillList = false;

        if (m_pValue == null)
        {
            m_pValue = new List<int>(count);
            fillList = true;
        }
        else if (m_pValue.Count != count)
        {
            m_pValue.Clear();
            m_pValue.Capacity = count;
            fillList = true;

       // Debug.LogError("G Exist but Clear cuz different size ! () !! ");
        }

        if (fillList)
        {
            for (int i = 0; i < count; i++)
            {
                m_pValue.Add(0);
            }
        }
        else
        {
             // UnityEngine.Debug.LogError("R E U S E  !! ");

            for (int i = 0; i < count; i++)
            {
                m_pValue[i] = 0;
            }
        }

       float digit = (float)System.Math.Log(count, 10) - 2;

        digit = (digit <= 0) ? 0 : digit;

        int sum = 0; 

        while (count > sum) 
        {
        
            for (int i = 0; i < count; ++i)
            {
                var value = NextGaussian(rand, mu, sigma);

                int x = (int)(value * Math.Pow(10, digit));

                if (x >= 0 && x < m_pValue.Count)
                {
                    m_pValue[x]++;
                    sum++;
                }

                if (count <= sum) 
                    break;
            }
        }
    }

    public GaussianRandom(double mu, double sigma, int count)
    {
        Refresh(mu, sigma, count);
    }

    int Sum(List<int> list)
    {
        int result = 0;

        for (int i = 0; i < list.Count; i++)
        {
            result += list[i];
        }

        return result;
    }

    double NextGaussian(Random r, double mu = 0, double sigma = 1)
    {
        var u1 = r.NextDouble();
        var u2 = r.NextDouble();

        var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                            Math.Sin(2.0 * Math.PI * u2);

        var rand_normal = mu + sigma * rand_std_normal;

        return rand_normal;
    }

    public KeyValuePair<int, int> Next()
    {
        Random random = new Random(Guid.NewGuid().GetHashCode());
        int probeValue = random.Next(0, Value.Count);

        int currentProbability = 0;
        int probability = 0;

        for (int i = 0; i < Value.Count; ++i)
        {
            currentProbability += Value[i];

            if (currentProbability >= probeValue)
            {
                probability = i + 1;
                break;
            }
        }

        probability = (currentProbability == 0) ? Value.Count : probability;

        return new KeyValuePair<int, int>(probeValue, probability);
    }

}