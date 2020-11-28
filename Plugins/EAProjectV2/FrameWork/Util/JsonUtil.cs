using JsonFx.Json;
using System.Collections.Generic;
using System.Text;

public static class JsonUtil
{
    public static string ObjToJson(object obj, bool prettyPrint = false)
    {
		if (prettyPrint)
		{
			JsonWriterSettings jwsetting = new JsonWriterSettings();
			jwsetting.PrettyPrint = true;
			StringBuilder sb = new StringBuilder();
			JsonWriter writer = new JsonWriter(sb, jwsetting);
			writer.Write(obj);

			return sb.ToString();
		}
		else
		{
			return JsonWriter.Serialize(obj);
		}
    }
    public static T JsonToObj<T>(string json)
    {
		return JsonReader.Deserialize<T>(json);
    }

    public static Dictionary<string, object> JsonToDic(string json)
    {
        return JsonReader.Deserialize<Dictionary<string, object>>(json);
    }
    public static object JsonToObj(string json, System.Type type)
    {
        
		return JsonReader.Deserialize(json, type);        
    }


}
