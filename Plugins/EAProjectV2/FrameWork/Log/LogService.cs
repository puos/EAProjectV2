using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


/// <summary>
/// LogService class
/// </summary>
public class LogService
{
	public static bool enableLog;
	
	private static List<ILogger> loggerList = new List<ILogger>();

	static bool _hasFileLogger = false;
    static bool _hasConsoleLogger = false;

    public static string logPath = Application.persistentDataPath + "/logs";
	
	public static void AddFileLogger()
	{
		if(!_hasFileLogger)
        {
            loggerList.Add(new FileLogger(logPath));
            _hasFileLogger = true;
        } 
	}

    public static void AddConsoleLogger()
    {
        if(!_hasConsoleLogger)
        {
            loggerList.Add(new ConsoleLogger());
            _hasConsoleLogger = true;
        }
    }
    
	public static int LogTagMask { get; set; }

	public static bool HasLogTagMask(LogTag logTag)
	{
		return (LogTagMask & logTag.Flag) != 0;
	}

	// 사용예) 
	// LogService.GetLogger(System.Type.GetType("SomeLogger"))
	//		.Log("TAG", logString);
	//-----------------------------------------------------------------------------------------
	public static ILogger GetLogger(System.Type type)
	{
		foreach (var logger in loggerList)
		{
			if (logger.GetType() == type)
				return logger;
		}
		return null;
	}

	public static void AddLogger(ILogger logger)
	{
		if (logger == null)
			return;

		foreach (var l in loggerList)
		{
			if (l.GetType() == logger.GetType())
				return;
		}

		loggerList.Add(logger);
	}

	public static void RemoveLogger(ILogger logger)
	{
		if (logger == null)
			return;

		foreach (var l in loggerList)
		{
			if (l.GetType() == logger.GetType())
			{
				loggerList.Remove(logger);
				return;
			}
		}
	}

	public static void Log(LogTag tag, string logString, int colorTag = -1)
	{
		if (!enableLog)
			return;

		if (loggerList == null)
			return;

		foreach (var logger in loggerList)
		{
			logger.Log(LogType.Log, tag, logString, colorTag);
		}
	}

	public static void LogWarning(LogTag tag, string logString, int colorTag = -1)
	{
		if (!enableLog)
			return;

		if (loggerList == null)
			return;

		foreach (var logger in loggerList)
		{
			logger.Log(LogType.Warning, tag, logString, colorTag);
		}
	}

	public static void LogError(LogTag tag, string logString, int colorTag = -1)
	{
		if (!enableLog)
			return;

		if (loggerList == null)
			return;

		foreach (var logger in loggerList)
		{
			logger.Log(LogType.Error, tag, logString, colorTag);
		}
	}

	public static void DumpLog(LogType type, string msg)
	{
		int year = DateTime.Now.Year;
		int month = DateTime.Now.Month;
		int day = DateTime.Now.Day;

		string logFilePath = Application.persistentDataPath + "/dump/" + year.ToString() + "_" + month.ToString() + "_" + day.ToString();
		if (Directory.Exists(logFilePath) == false)
			Directory.CreateDirectory(logFilePath);

		logFilePath += "/dump.txt";
		using (StreamWriter w = File.AppendText(logFilePath))
		{
			w.WriteLine("\r\nDump : {0}", DateTime.Now.ToLongTimeString());
			switch (type)
			{
				case LogType.Log:
					w.WriteLine(" i : {0}", msg); break;
				case LogType.Warning:
					w.WriteLine(" w : {0}", msg); break;
				case LogType.Error:
				case LogType.Exception:
				case LogType.Assert:
					w.WriteLine(" e : {0}", msg);
					w.WriteLine("  : {0}", UnityEngine.StackTraceUtility.ExtractStackTrace());
					w.WriteLine("-------------------------------");
					break;
			}
		}
	}

	// 사용법
	// Debug.Error("error at " + DebugUtil.getCallerInfo());
	public static string GetCallMethodInfo()
	{
		var st = new System.Diagnostics.StackTrace(true);
		var sf = st.GetFrame(1);

		string filePath = sf.GetFileName().Substring(Application.dataPath.Length - "Assets".Length);

		return string.Format(" at {0}:{1}() (in {2}:line {3})",
			sf.GetMethod().ReflectedType.Name, sf.GetMethod().Name,
			filePath, sf.GetFileLineNumber());
	}

	public static string GetStackTrace()
	{
		System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
		return stackTrace.ToString();
	}
}
