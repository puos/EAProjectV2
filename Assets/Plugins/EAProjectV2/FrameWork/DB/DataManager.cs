using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Debug = EAFrameWork.Debug;

public class DataInfo
{
}

public class DataTable : ScriptableObject
{
    public virtual void Load()
    {        
    }

    public virtual DataInfo FindByKey(string key)
    {
       return default(DataInfo);
    }

    public virtual DataInfo[] GetArrayData()
    {
        return null;
    }

    public virtual void ParseCSV(Type classtype, string path)
    {
    }

    public Array ParseCSVToArray(Type classtype, string path)
    {
        return default(Array);
    }

}

public class DataManager<classT> : EAGenericSingleton<classT> where classT : new()
{
    Dictionary<int, DataTable> _dicDataTables = new Dictionary<int, DataTable>();
    
    #region Public Method

    public void InitializeTableData(Dictionary<string , string> DataInfoList)
    {
        foreach(KeyValuePair<string,string> values in DataInfoList)
        {
            DataTable so  = ResourceManager.instance.Load<DataTable>(values.Value);

           if(so != null)
           {
                int key = CRC32.GetHashForAnsi(values.Key);

                DataTable outDatas = null;

                if(!_dicDataTables.TryGetValue(key , out outDatas))
                {
                    so.Load();
                    _dicDataTables.Add(key, so);
                }
           } 
        } 
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="DataInfoList"></param>
	public void InitializePreData(Dictionary<string, string> DataInfoList)
	{
        foreach (KeyValuePair<string, string> values in DataInfoList)
        {
            DataTable so = ResourceManager.instance.Load<DataTable>(values.Value);

            if (so != null)
            {
                int key = CRC32.GetHashForAnsi(values.Key);

                DataTable outDatas = null;

                if (!_dicDataTables.TryGetValue(key, out outDatas))
                {
                    so.Load();
                    _dicDataTables.Add(key, so);
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableKey"></param>
    /// <returns></returns>
    public T GetData<T>(string tableKey) where T : DataInfo
    {
        DataTable table   = null;
        
        string tableName = typeof(T).Name;
        
        int key = CRC32.GetHashForAnsi(tableName);

        _dicDataTables.TryGetValue(key,out table);

        if(table != null)
        {
            return (T)table.FindByKey(tableKey);
        } 
        
        Debug.LogError("not find data type " + typeof(T) + " key=" + tableKey);

        return default(T);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="match"></param>
    /// <returns></returns>
    public List<T> GetData<T>(Predicate<T> match) where T : DataInfo
    {
        DataTable table = null;

        List<T> list = new List<T>();

        string tableName = typeof(T).Name;

        int key = CRC32.GetHashForAnsi(tableName);

        _dicDataTables.TryGetValue(key, out table);
        
        if (table != null)
        {
            T[] array = table.GetArrayData() as T[];

            for(int i = 0; i < array.Length; ++i)
            {
                if(match(array[i]))
                {
                    list.Add(array[i]);
                }
            }
        }

        return list;
    } 

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="match"></param>
    /// <returns></returns>
    public T GetDataFirst<T>(Predicate<T> match) where T : DataInfo
    {
        DataTable table = null;

        T info = null;

        string tableName = typeof(T).Name;

        int key = CRC32.GetHashForAnsi(tableName);

        _dicDataTables.TryGetValue(key, out table);

        if (table != null)
        {
            T[] array = table.GetArrayData() as T[];

            for (int i = 0; i < array.Length; ++i)
            {
                if (match(array[i]))
                {
                    info = array[i];
                    break;
                }
            }
        }

        return info;
    }

    /// <summary>
    /// Get array data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T[] GetArrayData<T>()
    {
        DataTable table = null;

        T[] array = null;

        string tableName = typeof(T).Name;

        int key = CRC32.GetHashForAnsi(tableName);

        _dicDataTables.TryGetValue(key, out table);

        if (table != null)
        {
            array = table.GetArrayData() as T[];
        }

        return array;
    } 

    /// <summary>
    ///  Find the name of a data asset by type using the following rules
    /// const string tmplDataFile = "[TableName]Info";
    /// const string tmplDataHolderFile = "[TableName]DataHolder";
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string Type2AssetName(Type type)
    {
        const string TABLE_NAME_POSTFIX = "Info";
        const string ASSET_NAME_POSTFIX = "DataHolderAsset";
        string _className = type.Name;
        return _className.Replace(TABLE_NAME_POSTFIX, ASSET_NAME_POSTFIX);
    }

    /// <summary>
    /// Translate with key
    /// </summary>
    /// <param name="langType"> Currently applied language type </param>
    /// <param name="uiType"> Currently applied ui type </param>
    /// <param name="uiID"></param>
    /// <param name="parms"></param>
    /// <returns></returns>
    public static string NotSetString = "<color=red><NotFound></color>";

    public virtual string TranslateKeyArgs(LANGUAGE_TYPE langType, UI_TEXT_TYPE uiType, string uiID, params object[] parms)
    {
         string text = uiID;
         return text;
    }
    
    /// <summary>
    ///  Reserved word processing routines
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parms"></param>
    /// <returns></returns>
    protected string TranslateKeyArgs(string value, params object[] parms)
    {
        // line feed 수정
        string LF = "\\n";
        value = value.Replace(LF, "\n");

        string translateValue = value;
        int startValue = 0;
        int paramCount = 0;

        //  Parse a text marker
        for (int i = 0; i < value.Length; ++i)
        {
            char c = value[i];

            if (c == '{')
            {
                startValue = i + 1;
            }
            else if (c == '}' && startValue > 0)
            {
                string tValue = value.Substring(startValue, i - startValue);

                string tReplaceValue = Translate(tValue, parms, paramCount);

                // 변경 및 replace
                translateValue = translateValue.Replace("{" + tValue + "}", tReplaceValue);

                startValue = 0;
                paramCount = paramCount + 1;
            }
        }

        return translateValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parms"></param>
    /// <param name="paramCount"></param>
    /// <returns></returns>
    protected string Translate(string value, object[] parms, int paramCount)
    {
        // 변경 및 replace
        if (parms.Length <= paramCount || parms.Length <= 0)
        {
            return value;
        }

        return Convert.ToString(parms[paramCount]);
    }
}