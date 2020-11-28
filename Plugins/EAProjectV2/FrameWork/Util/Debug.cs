using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngineInternal;

namespace EAFrameWork
{
    public static class Debug
    {
        public static bool isDebugBuild
        {
            get { return UnityEngine.Debug.isDebugBuild; }
        }

        public static string ToStringOrEmpty(this object value)
        {
            return value == null ? "" : value.ToString();
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void Log(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void LogError(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogError(message, context);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message.ToString());
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void LogWarning(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(message.ToString(), context);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
        {
            UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void DrawRay(Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
        {
            UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void Assert(bool condition)
        {
            UnityEngine.Debug.Assert(condition, "");
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void Exception(bool condition)
        {
            if (!condition) throw new Exception();
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void Assert(bool condition, string message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void Break()
        {
            UnityEngine.Debug.Break();
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void Throw(string message)
        {
            throw new Exception(message);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(context, format, args);
        }

        [System.Diagnostics.Conditional("FRAMEWORK_DEBUG")]
        public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(context, format, args);
        }
    }
}

