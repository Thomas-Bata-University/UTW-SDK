using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor.Core {
    public class AssetBundleController : EditorWindow {

        private static AssetBundleController _window;

        [MenuItem("UTW/Generator", false, 4)]
        public static void ShowWindow() {
            _window = GetWindow<AssetBundleController>("Generator");
        }

        private void OnGUI() {
            CreateButton();
        }

        private static void BuildAllAssetBundles() {
            string bundleDirectory = AssetPaths.ASSET_BUNDLE;

            if (!Directory.Exists(bundleDirectory)) {
                Directory.CreateDirectory(bundleDirectory);
            }

            BuildPipeline.BuildAssetBundles(bundleDirectory, BuildAssetBundleOptions.None,
                BuildTarget.StandaloneWindows);

            AssetDatabase.Refresh();
            Debug.Log("AssetBundles successfully generated.");
        }

        private void CreateButton() {
            Color buttonColor = new Color(0.2f, 0.6f, 0.2f);
            Color borderColor = new Color(0.1f, 0.3f, 0.1f);

            float borderThickness = 5f;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = MakeTex(1, 1, buttonColor) },
                active = { textColor = Color.white, background = MakeTex(1, 1, new Color(0.15f, 0.5f, 0.15f)) },
                border = new RectOffset(5, 5, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            };

            Rect windowRect = new Rect(0, 0, position.width, position.height);
            EditorGUI.DrawRect(windowRect, borderColor);

            GUILayout.BeginVertical();
            GUILayout.Space(borderThickness);

            if (GUILayout.Button("Generate AssetBundles", buttonStyle,
                    GUILayout.Width(position.width - borderThickness * 2),
                    GUILayout.Height(position.height - borderThickness * 2))) {
                BuildAllAssetBundles();
            }

            GUILayout.Space(borderThickness);
            GUILayout.EndVertical();
        }

        private static Texture2D MakeTex(int width, int height, Color col) {
            Texture2D tex = new Texture2D(width, height);
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

    }
} //END