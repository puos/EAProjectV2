using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System;
using System.Text;
using System.IO;

/// <summary>
/// ConsoleLogger class
/// Code to write error log without stacktrace output.
/// On a limited basis, this code allows you to click on a log message in the debug console to jump to the source code line at the top stack point instead of the current stack.
/// 
/// * Only available in Editor mode.
/// * Play is paused at the output because it is an error type log.
/// 
/// Excerpt Site: http://answers.unity3d.com/questions/238229/debugconsole-console-clicking.html
/// </summary>
public class ConsoleLogger : ILogger
{
	public static bool useTLog = false;

	private static MethodBase mUnityLog;

	static ConsoleLogger()
	{
		mUnityLog = typeof(UnityEngine.Debug).GetMethod("LogPlayerBuildError", BindingFlags.NonPublic | BindingFlags.Static);
	}

	
	public void OnUpdate(float deltaTime)
	{
	}

    /// <summary>
    /// Additional handling to avoid losing color for too long log text.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="colorTag"></param>
    /// <param name="msg"></param>
    /// <param name="logCb"></param>
    void _WriteUnityLogWithColor(string tag, string colorTag, string msg, System.Action<string> logCb)
	{
        // Short text
        const int UpperConsoleColorCapableSize = 1000;
		if (msg.Length <= UpperConsoleColorCapableSize)
		{
			if (colorTag == null)
				logCb((tag == null) ? msg : (tag + msg));
			else
			{
				if(tag == null)
					logCb(string.Format("<color={0}>{1}</color>", colorTag, msg));
				else
					logCb(string.Format("{0}<color={1}>{2}</color>", tag, colorTag, msg));
			}
			return;
		}

        //// Long text loses color in the upper console, so color cropped summaries. In addition, the text is displayed in succession.

        int pos = UpperConsoleColorCapableSize;
		while (pos < msg.Length && (msg[pos] != ' ' || msg[pos] != ',' || msg[pos+1] != '\"')) // If you break at the sequence character, it is an output error.
                        pos++;
        // Output summary text
        if (colorTag == null)
			logCb((tag == null) ? msg : (tag + msg.Substring(0, pos) + " ..."));
		else
		{
			if (tag == null)
				logCb(string.Format("<color={0}>{1} ...</color>\n", colorTag, msg.Substring(0, UpperConsoleColorCapableSize)));
			else
				logCb(string.Format("{0}<color={1}>{2} ...</color>\n", tag, colorTag, msg.Substring(0, UpperConsoleColorCapableSize)));
		}
		
		// longMsg output
		const int MaxIter = 100;
		const int SegmentSize = 15000;
		int prevPos = 0;
		pos = 0;

		for (int i = 0; i < MaxIter && prevPos < msg.Length; i++)
		{
			pos += Mathf.Min(msg.Length - pos, SegmentSize);

			while (pos < msg.Length && (msg[pos] != ' ' || msg[pos+1] != '\"')) // If you break at the sequence character, it is an output error.
                                pos++;

			string fullMsgMent = string.Format("(전체텍스트 #{0}) ", i);

			// msg output
			string msgPartial = msg.Substring(prevPos, pos - prevPos);

			if (colorTag == null)
				logCb(fullMsgMent + ((tag != null && i == 0) ? (tag + msgPartial) : msgPartial));
			else
			{
				if (tag != null && i == 0)
					logCb(string.Format("{0}{1}<color={2}>{3}</color>", fullMsgMent, tag, colorTag, msgPartial));
				else
					logCb(string.Format("{0}<color={1}>{2}</color>", fullMsgMent, colorTag, msgPartial));
			}
			prevPos = pos;
		}
	}
	//
	//-----------------------------------------------------------------------------------------
	public void Log(LogType type, LogTag tag, string logString, int colorTagIdx = -1)
	{
		if ((tag == null && (LogService.LogTagMask & LogCatBasic.Default.Flag) == 0)
			|| (tag != null && (LogService.LogTagMask & tag.Flag) == 0)
			|| logString == null)
			return;

		//if (!string.IsNullOrEmpty(tag))
		//	logString = string.Format("[{0}] {1}", tag, logString);

		DateTime currTime = DateTime.Now;

		string tagForm = (tag == null)
			? string.Format("{0:00}:{1:00}:{2:00} ", currTime.Hour, currTime.Minute, currTime.Second)
			: string.Format("{0:00}:{1:00}:{2:00} [{3}] ", currTime.Hour, currTime.Minute, currTime.Second, tag.Name);
			
		string colorTag = (colorTagIdx == -1) ? null : LogColorTagText.values[colorTagIdx];

		if (mUnityLog == null || useTLog == false)
		{
            //this happens outside of the editor mode
            //log the normal way and ignore everything that isn't an error
            //The default behavior is to print to the console.

            switch (type)
			{
				case LogType.Log:
					_WriteUnityLogWithColor(tagForm, colorTag, logString, delegate(string msg) { UnityEngine.Debug.Log(msg); });
					break;
				case LogType.Warning:
					_WriteUnityLogWithColor(tagForm, colorTag, logString, delegate(string msg) { UnityEngine.Debug.LogWarning(msg); });
					break;
				case LogType.Error:
				case LogType.Exception:
				case LogType.Assert:
					_WriteUnityLogWithColor(tagForm, colorTag, logString, delegate(string msg) { UnityEngine.Debug.LogError(msg); });
					break;
			}
			return;
		}

		StringBuilder message = new StringBuilder();
		switch (type)
		{
			case LogType.Log:
				message.Append(LogUtil.Style(logString, Color.white));
				break;
			case LogType.Warning:
				message.Append(LogUtil.Style(logString, Color.yellow));
				break;
			case LogType.Error:
			case LogType.Exception:
			case LogType.Assert:
				message.Append(LogUtil.Style(logString, Color.red));
				break;
		}
		message.Append("\n");

		StackTrace stackTrace = new StackTrace(true);
		StackFrame[] stackFrames = stackTrace.GetFrames();
		string file = "";
		int line = 0;
		int col = 0;

		bool foundStart = false;
		//look for the first method call in the stack that isn't from this class.
		//save the first one to jump into it later and add all further lines to the log
		for (int i = 0; i < stackFrames.Length; i++)
		{
			MethodBase mb = stackFrames[i].GetMethod();
			if (foundStart == false
				&& mb.DeclaringType != typeof(Log)
				&& mb.DeclaringType != typeof(LogService)
				&& mb.DeclaringType != typeof(ConsoleLogger)
				&& mb.DeclaringType != typeof(EADebug))
			{
				file = FormatFileName(stackFrames[i].GetFileName());
				line = stackFrames[i].GetFileLineNumber();
				col = stackFrames[i].GetFileColumnNumber();
				foundStart = true;
			}

			if (foundStart)
			{
				message.Append(mb.DeclaringType.FullName);
				message.Append(":");
				message.Append(mb.Name);
				message.Append("(");

				bool showParameters = true;
				if (showParameters)
				{
					ParameterInfo[] paramters = mb.GetParameters();
					for (int k = 0; k < paramters.Length; k++)
					{
						message.Append(paramters[k].ParameterType.Name);
						if (k + 1 < paramters.Length)
							message.Append(", ");
					}
				}

				message.Append(")");

				message.Append(" (at ");
				//the first stack message is found now we add the other stack frames to the log
				message.Append(FormatFileName(stackFrames[i].GetFileName()));
				message.Append(":");
				message.Append(stackFrames[i].GetFileLineNumber());
				message.Append(")");
				message.Append("\n");
			}
		}
		mUnityLog.Invoke(null, new object[] { message.ToString(), file, line, col });
	}

	private static string FormatFileName(String file)
	{
		if (string.IsNullOrEmpty(file))
			return string.Empty;
		//remove everything of the absolute path that is before the Assetfolder
		//using the destination of the Assetfolder to get the right length (not ideal)
		return file.Remove(0, Application.dataPath.Length - "Assets".Length);
	}
}

