using UnityEngine;

namespace Editor.Helper {
    public static class ColorLogger {

        public static void Log(string message) => Debug.Log(message);
        public static void LogWarning(string message) => Debug.LogWarning(message);
        public static void LogError(string message) => Debug.LogError(message);

        public static void LogColor(string message, string color) =>
            Debug.Log($"<color={color}>{message}</color>");

        public static void LogBold(string message) =>
            Debug.Log($"<b>{message}</b>");

        public static void LogColorBold(string message, string color) =>
            Debug.Log($"<color={color}><b>{message}</b></color>");

        public static void LogFormatted(
            string format,
            string highlightedText,
            string color = "white",
            bool bold = false,
            LogLevel level = LogLevel.Info) {
            string formattedHighlight = highlightedText;

            if (!string.IsNullOrEmpty(color))
                formattedHighlight = $"<color={color}>{formattedHighlight}</color>";

            if (bold)
                formattedHighlight = $"<b>{formattedHighlight}</b>";

            string final = string.Format(format, formattedHighlight);
            LogWithLevel(final, level);
        }

        public static void LogFormatted(
            string format,
            string[] values,
            string[] colors = null,
            bool[] bolds = null,
            LogLevel level = LogLevel.Info) {
            object[] formattedValues = new object[values.Length];

            for (int i = 0; i < values.Length; i++) {
                string value = values[i];
                string color = (colors != null && i < colors.Length) ? colors[i] : null;
                bool bold = (bolds != null && i < bolds.Length) ? bolds[i] : false;

                if (!string.IsNullOrEmpty(color))
                    value = $"<color={color}>{value}</color>";

                if (bold)
                    value = $"<b>{value}</b>";

                formattedValues[i] = value;
            }

            string result = string.Format(format, formattedValues);
            LogWithLevel(result, level);
        }

        private static void LogWithLevel(string message, LogLevel level) {
            switch (level) {
                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                    Debug.LogError(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }

        public enum LogLevel {

            Info,
            Warning,
            Error

        }

    }
}