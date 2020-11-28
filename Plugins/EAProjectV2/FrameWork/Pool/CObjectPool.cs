using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Used by using TakeWrapper of ObjectPool.
/// 
/// This allows you to use the Release function directly using only the Release () function.
/// </summary>
public class CObjectPoolWrapper<T>
{
    private CObjectPool<T> _parent;
    private T _value;

    public T Value
    {
        get
        {
            return _value;
        }
        protected set
        {
            _value = value;
        }
    }

    public CObjectPoolWrapper(CObjectPool<T> pool, T value)
    {
        _parent = pool;
        _value = value;
    }
    public CObjectPoolWrapper(CObjectPool<T> pool)
    {
        _parent = pool;
    }

    public void Release()
    {
        _parent.Release(this);
    }
}

/// <summary>
/// The object pool actually used. If CreateFunc is not entered, an error occurs in the automatic creation and initialization.
/// To remove a pool, use the Dispose () method.
/// </summary>
/// <code>
/// 
/// ObjectPool<GameObject> pool = new ObjectPool<GameObject>();
/// GameObject go = pool.Take(); //acheive
/// 
/// //something
/// 
/// pool.Release(go); //return
/// 
/// </code>
/// <typeparam name="T">Generic arguments can only be used as classes.</typeparam>
public class CObjectPool<T> : IDisposable
{
    private class ObjectPoolException : ApplicationException
    {
        public ObjectPoolException() : base() { }
        public ObjectPoolException(string message) : base(message) { }
        public ObjectPoolException(string message, Exception innerException) : base(message, innerException) { }
    }

    public delegate T CreateFunc();
    public delegate void ActionFunc(T value);
    public delegate void ActionFuncWrapper(CObjectPoolWrapper<T> value);

    /// <summary>
    /// When the create function is entered, an array is created.
    /// </summary>
    public CreateFunc OnCreate
    {
        get
        {
            return _onCreate;
        }
        set
        {
            _onCreate = value;
            if (objects == null)
            {
                objects = new Stack<CObjectPoolWrapper<T>>();
                CreateObjects(InitializeCreateCount);
            }
        }
    }
    private CreateFunc _onCreate;

    public ActionFunc OnTake;
    public ActionFuncWrapper OnTakeWrapper;
    public ActionFunc OnRelease;
    public ActionFunc OnObjectDestroy;
    public System.Action OnPoolDestroy;

    public int InitializeCreateCount = 10;
    public int ExpandCreateCount = 10;
    private int _createCount = 0;
    public int TotalCreated { get { return _createCount; } }

    public Stack<CObjectPoolWrapper<T>> Objects
    {
        get
        {
            return objects;
        }
    }

    private Stack<CObjectPoolWrapper<T>> objects;

    #region LifeCycle
    /// <summary>
    /// Create a pool that does nothing.
    /// </summary>
    /// <param name="array"></param>
    public CObjectPool()
    {
    }
    /// <summary>
    /// Ignore default arguments and put arrays on the stack
    /// </summary>
    /// <param name="array"></param>
    public CObjectPool(T[] array)
    {
        objects = new Stack<CObjectPoolWrapper<T>>();
        InitializeCreateCount = array.Length;
        for (int i = 0; i < InitializeCreateCount; i++)
        {
            objects.Push(new CObjectPoolWrapper<T>(this, array[i]));
        }
    }
    /// <summary>
    /// Create a pool with the size of the initial value.
    /// </summary>
    public CObjectPool(CreateFunc func)
    {
        OnCreate = func;
    }
    /// <summary>
    /// Create a pool with argument values
    /// </summary>
    /// <param name="initCount"></param>
    public CObjectPool(CreateFunc func, int initCount)
    {
        InitializeCreateCount = initCount;
        OnCreate = func;
    }
    #endregion

    #region clear
    /// <summary>
    /// Remove values ​​other than the initial number of creation or the number of objects outside the pool.
    /// </summary>
    public void Reset()
    {
        int len = objects.Count;
        int i = _createCount - len; // The number of objects that went out of the pool and did not come back
        i = Math.Max(i, InitializeCreateCount);

        if (OnObjectDestroy != null)
        {
            for (; i <= len; i++)
            {
                OnObjectDestroy(objects.Pop().Value);
            }
        }
        else
        {
            for (; i <= len; i++)
            {
                objects.Pop();
            }
        }
    }

    /// <summary>
    /// Free each object.
    /// </summary>
    public void Dispose()
    {
        if (OnObjectDestroy != null)
        {
            var e = objects.GetEnumerator();
            while (e.MoveNext())
            {
                OnObjectDestroy(e.Current.Value);
            }
        }

        if (OnPoolDestroy != null)
            OnPoolDestroy();
    }
    #endregion

    #region creation
    /// <summary>
    /// Increase the size by the argument count.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private void CreateObjects(int count)
    {
        if (OnCreate == null)
            throw new ObjectPoolException("Not Support Create More Objects");

        _createCount += count;

        for (int i = 0; i < count; i++)
        {
            objects.Push(_creation());
        }
    }
    private CObjectPoolWrapper<T> _creation()
    {
        T temp = OnCreate();

        if (temp is CObjectPoolWrapper<T>)
        {
            return temp as CObjectPoolWrapper<T>;
        }
        else
        {
            return new CObjectPoolWrapper<T>(this, temp);
        }
    }
    #endregion

    #region Pooling
    /// <summary>
    /// Fetch the object stored in the pool. If there is no saved object, create as many additional ExpandCreateCount.
    /// </summary>
    public CObjectPoolWrapper<T> Take()
    {
        CObjectPoolWrapper<T> result;
        if (objects == null)
        {
            throw new ObjectPoolException("Not Initialize Pool. Please assign OnCreate Function");
        }
        else if (objects.Count == 0)
        {
            CreateObjects(ExpandCreateCount - 1);
            result = _creation();
        }
        else
        {
            result = objects.Pop();
        }

        if (OnTake != null)
            OnTake(result.Value);

        if (OnTakeWrapper != null)
            OnTakeWrapper(result);

        return result;
    }
    /// <summary>
    /// Put the object back into the pool.
    /// </summary>
    /// <param name="obj"></param>
    public void Release(CObjectPoolWrapper<T> obj)
    {
        if (OnRelease != null)
            OnRelease(obj.Value);

        objects.Push(obj);
    }
    #endregion

}
