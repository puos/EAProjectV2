using System;

public class EA_EventMsg
{
    public static string m_sGroupName;		// Group name to receive the message
    public static string m_sTargetName;    // Target name to receive the message

    public static string m_sEventName;		// Name of the event receiving the message 
    public static string m_sBuffer;           // The message is saved. Separate strings with semicolons.

    public const string sperateChar = ";";

    public static string[] GetBuffer()
    {
        string szSeparateExt = EA_EventMsg.sperateChar;
        string[] arExtParser = m_sBuffer.Split(szSeparateExt.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        return arExtParser;
    }
}