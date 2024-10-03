using UnityEditor;
using UnityEngine;

namespace Other.CoreScripts {
    public class StyleUtils {

        public static GUIStyle Style(int size, GUIStyle style) {
            return new GUIStyle(style) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = size
            };
        }

        public static void SetMiddle(EditorWindow window) {
            Rect screen = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
            window.position = new Rect(
                (screen.width - window.minSize.x) / 2,
                (screen.height - window.minSize.y) / 2,
                window.minSize.x,
                window.minSize.y
            );
        }

        public static void SetSize(EditorWindow window, Vector2 size) {
            window.minSize = size;
            window.maxSize = size;
        }

    }
} //END