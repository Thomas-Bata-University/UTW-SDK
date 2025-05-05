using System.Collections.Generic;
using System.IO;
using Editor.Const;
using UnityEditor;
using UnityEngine;

namespace Editor.Core {
    public class ProjectDataBrowser : EditorWindow {

        private Object selectedAsset;
        private GUIStyle headerStyle;
        private GUIStyle gridItemStyle;
        private GUIStyle labelStyle;
        private GUIStyle selectedGridItemStyle;
        private Vector2 scroll;

        [MenuItem("Tools/Project Assets Browser")]
        public static void ShowWindow() {
            GetWindow<ProjectDataBrowser>("Project Assets");
        }

        private void InitStyles() {
            headerStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 18
            };

            gridItemStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fixedHeight = 20,
                margin = new RectOffset(4, 4, 2, 2),
                padding = new RectOffset(2, 2, 2, 2),
                normal = { textColor = Color.white }
            };

            selectedGridItemStyle = new GUIStyle(gridItemStyle);
            selectedGridItemStyle.normal.textColor = Color.white;
            selectedGridItemStyle.normal.background = CreateColorTexture(new Color(0.2f, 0.4f, 1f));

            labelStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 13
            };
        }

        private void OnGUI() {
            if (headerStyle == null || gridItemStyle == null || labelStyle == null || selectedGridItemStyle == null)
                InitStyles();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            GUILayout.Space(10);
            GUILayout.Label("Materials", headerStyle);
            DrawList(Path.Combine(AssetPaths.PROJECT_DATA, "Materials"));

            GUILayout.Space(15);
            GUILayout.Label("Mesh", headerStyle);
            DrawList(Path.Combine(AssetPaths.PROJECT_DATA, "Mesh"));

            GUILayout.Space(15);
            GUILayout.Label("Texture", headerStyle);
            DrawList(Path.Combine(AssetPaths.PROJECT_DATA, "Texture"));

            EditorGUILayout.EndScrollView();
            
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                GUI.FocusControl(null);
                selectedAsset = null;
                Repaint();
            }

            GUILayout.Space(10);
            DrawBottomBar();
        }

        private void DrawList(string folderPath) {
            if (!Directory.Exists(folderPath) || Directory.GetFiles(folderPath).Length == 0) {
                GUILayout.Label("No data.", new GUIStyle(EditorStyles.miniLabel));
                return;
            }

            string[] files = Directory.GetFiles(folderPath);

            foreach (string file in files) {
                if (file.EndsWith(".meta")) continue;
                string assetPath = file.Replace("\\", "/");
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset == null) continue;

                GUIContent content = EditorGUIUtility.ObjectContent(asset, asset.GetType());
                GUIStyle styleToUse = asset == selectedAsset ? selectedGridItemStyle : gridItemStyle;

                EditorGUILayout.BeginHorizontal();

                int iconSize = labelStyle.fontSize + 10;

                if (GUILayout.Button(content.image, GUIStyle.none, GUILayout.Width(iconSize),
                        GUILayout.Height(iconSize))) {
                    selectedAsset = asset;
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }


                if (GUILayout.Button(asset.name, styleToUse, GUILayout.ExpandWidth(true))) {
                    selectedAsset = asset;
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawBottomBar() {
            EditorGUILayout.BeginHorizontal("box");

            if (GUILayout.Button("Import Asset", GUILayout.Width(100), GUILayout.Height(26))) {
                string path = EditorUtility.OpenFilePanel("Import Asset", "", "");
                if (!string.IsNullOrEmpty(path)) {
                    string fileName = Path.GetFileName(path);
                    string ext = Path.GetExtension(fileName).ToLower();

                    // Cesty
                    string fbxFolder = Path.Combine(AssetPaths.PROJECT_DATA, "FBX");
                    string meshFolder = Path.Combine(AssetPaths.PROJECT_DATA, "Mesh");
                    string matFolder = Path.Combine(AssetPaths.PROJECT_DATA, "Materials");
                    string texFolder = Path.Combine(AssetPaths.PROJECT_DATA, "Texture");

                    Directory.CreateDirectory(fbxFolder);
                    Directory.CreateDirectory(meshFolder);
                    Directory.CreateDirectory(matFolder);
                    Directory.CreateDirectory(texFolder);

                    string targetPath = Path.Combine(
                        ext == ".fbx" ? fbxFolder :
                        ext == ".mat" ? matFolder :
                        ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" ? texFolder :
                        ext == ".asset" ? meshFolder :
                        AssetPaths.PROJECT_DATA,
                        fileName
                    ).Replace("\\", "/");

                    File.Copy(path, targetPath, true);
                    AssetDatabase.Refresh();

                    if (ext == ".fbx") {
                        EditorApplication.delayCall += () => {
                            ImportFbx(targetPath, AssetPaths.PROJECT_DATA);
                        };
                    }
                }
            }

            GUILayout.FlexibleSpace();

            GUI.enabled = selectedAsset != null;
            if (GUILayout.Button("Delete", GUILayout.Width(100), GUILayout.Height(26))) {
                string assetPath = AssetDatabase.GetAssetPath(selectedAsset);
                if (!string.IsNullOrEmpty(assetPath)) {
                    AssetDatabase.DeleteAsset(assetPath);
                    selectedAsset = null;
                    AssetDatabase.Refresh();
                }
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }
        
        private Texture2D CreateColorTexture(Color color) {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private void ImportFbx(string fbxPath, string destinationRoot) {
            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbx == null) {
                Debug.LogError("Invalid FBX file at path: " + fbxPath);
                return;
            }

            MeshFilter[] meshFilters = fbx.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in meshFilters) {
                Mesh mesh = filter.sharedMesh;
                if (mesh != null) {
                    string meshPath = Path.Combine(destinationRoot, "Mesh", mesh.name + ".asset").Replace("\\", "/");
                    Directory.CreateDirectory(Path.GetDirectoryName(meshPath));
                    AssetDatabase.CreateAsset(Instantiate(mesh), meshPath);
                    Debug.Log($"Mesh saved: {meshPath}");
                }
            }

            Renderer[] renderers = fbx.GetComponentsInChildren<Renderer>();
            HashSet<Material> collectedMaterials = new HashSet<Material>();
            foreach (var rend in renderers) {
                foreach (var mat in rend.sharedMaterials) {
                    if (mat != null && collectedMaterials.Add(mat)) {
                        string matPath = Path.Combine(destinationRoot, "Materials", mat.name + ".mat")
                            .Replace("\\", "/");
                        Directory.CreateDirectory(Path.GetDirectoryName(matPath));
                        Material matCopy = new Material(mat);
                        AssetDatabase.CreateAsset(matCopy, matPath);
                        Debug.Log($"Material saved: {matPath}");

                        Texture mainTex = mat.mainTexture;
                        if (mainTex != null) {
                            string texPath = Path.Combine(destinationRoot, "Texture", mainTex.name + ".asset")
                                .Replace("\\", "/");
                            Directory.CreateDirectory(Path.GetDirectoryName(texPath));
                            AssetDatabase.CreateAsset(Instantiate(mainTex), texPath);
                            Debug.Log($"Texture saved: {texPath}");
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Import completed.");
        }

    }
}