using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System;
using System.Text;
using System.IO;



/// <summary>
///  Log filtering enum
///  Default tag 0-5 used in log are fixed
/// </summary>
public enum LogTagFlags
{
    Default = (1 << 0),
    DB = (1 << 1),
    UUnit = (1 << 2),
    FrameWork = (1 << 3),
    File = (1 << 4),
    Resource = (1 << 5),
}

public class LogTag
{
	public int Flag;
	public string Name;
	//public string NameWithBracket;
	public LogTag(int flag, string name)
	{
		this.Flag = flag;
		this.Name = name;
	}

    public LogTag(LogTagFlags flag)
    {
        this.Flag = (int)flag;
        this.Name = flag.ToString();
    }
}


/// <summary>
/// 
/// </summary>
public class LogCatBasic
{
    public static LogTag Default = new LogTag(LogTagFlags.Default);
    public static LogTag DB = new LogTag(LogTagFlags.DB);
    public static LogTag UUnit = new LogTag(LogTagFlags.UUnit);
    public static LogTag FrameWork = new LogTag(LogTagFlags.FrameWork);
    public static LogTag File = new LogTag(LogTagFlags.File);
    public static LogTag Resource = new LogTag(LogTagFlags.Resource);
}

/// <summary>
/// ILogger interface
/// </summary>
public interface ILogger
{
    /// <summary>
    /// 로그를 필터링 한다. 
    /// </summary>
    /// <param name="type"> 로그타입(Info, Warning, Error, ...) </param>
    /// <param name="tag">  카테고리별 (컨텐츠와 관련) </param>
    /// <param name="logString"></param>
    /// <param name="colorTag"></param> 
    void Log(LogType type, LogTag tag, string logString, int colorTag = -1);

    void OnUpdate(float deltaTime);
}
