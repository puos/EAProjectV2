using System.Collections;

public class EAGenericSingleton<T>
        where T : new()
{
    static protected T _instance;
    
    static public T instance
    {
        get
        {
            if (_instance == null)
                 _instance = new T();

            return _instance;
        }
     }

    // Prevent generation from outside
    protected EAGenericSingleton()
    {
    }
  
}
