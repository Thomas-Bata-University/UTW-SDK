using UnityEditor;
using UnityEngine;

namespace Other.CoreScripts {
    public class AssetBundleController : EditorWindow {

        private static AssetBundleController _window;

        [MenuItem("UTW/Generator", false, 4)]
        public static void ShowWindow() {
            _window = GetWindow<AssetBundleController>("Generator");
        }
        
        private void OnGUI() {
            // ShowWindow();
        }

    }
} //END