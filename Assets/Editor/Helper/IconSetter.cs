using System.Linq;
using Editor.Const;
using UnityEditor;
using UnityEngine;

namespace Editor.Helper {
    public static class IconSetter {

        public static void SetIcon(GameObject go, string iconName = IconNames.LabelGray) {
            var icons = GetIcons(iconName);
            if (icons != null && icons.Length > 0) {
                var icon = icons[0];
                if (icon != null) {
                    EditorGUIUtility.SetIconForObject(go, icon);
                }
            }
        }

        private static Texture2D[] GetIcons(string baseName) {
            return Resources.FindObjectsOfTypeAll<Texture2D>()
                .Where(t => t.name.StartsWith(baseName))
                .ToArray();
        }

    }
}