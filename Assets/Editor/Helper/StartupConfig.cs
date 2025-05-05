using UnityEditor;
using UnityEngine;

namespace Editor.Helper {
    [InitializeOnLoad]
    public class StartupConfig {

        static StartupConfig() {
            SetStackTrace();
        }

        private static void SetStackTrace() {
            // Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            // Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            // Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
            // Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.None);
        }

    }
}