using UnityEngine;

namespace Other.Scripts {
    public class StyleUtils {

        public static GUIStyle Style(int size, GUIStyle style) {
            return new GUIStyle(style) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = size
            };
        }

    }
} //END