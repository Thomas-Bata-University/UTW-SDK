using UnityEditor;
using UnityEngine;

namespace Other.CoreScripts {
    public class LayoutChanger : Editor {

        [MenuItem("Tools/Change Layout")]
        public static void ChangeLayout() {
            string layoutPath = "Assets/Other/Layout/default.wlt";
            EditorUtility.LoadWindowLayout(layoutPath);
            Debug.Log("Layout has been changed!");
        }

    }
} //END