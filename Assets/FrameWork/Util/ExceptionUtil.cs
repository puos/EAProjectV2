using System;

public class ExceptionUtil
{
	public static void Throw(bool comparison, string form, params object[] parms)
	{
		if (comparison == false)
		{
			Throw(form, parms);
		}
	}

	public static void Throw(string form, params object[] parms)
	{
		string msg = string.Format(form, parms);
	
		throw new Exception(msg);
	}
}
