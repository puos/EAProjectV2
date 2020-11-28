#define DEBUG
using UnityEngine;
using System.Collections;

public class Log
{
	public static void d(LogTag tag, string message)
	{
#if DEBUG
		if ((LogService.LogTagMask & tag.Flag) == 0)
			LogService.Log( tag, message );
#endif
	}

	public static void d(string message)
	{
#if DEBUG
		LogService.Log(null, message);
#endif
	}

	public static void w(LogTag tag, string message)
	{
#if DEBUG
		if ((LogService.LogTagMask & tag.Flag) == 0)
			LogService.LogWarning( tag, message );
#endif
	}

	public static void w(string message)
	{
#if DEBUG
		LogService.LogWarning(null, message);
#endif
	}

	public static void e(LogTag tag, string message)
	{
		LogService.LogError( tag, message );
	}

	public static void e(string message)
	{
		LogService.LogError(null, message);
	}
}
