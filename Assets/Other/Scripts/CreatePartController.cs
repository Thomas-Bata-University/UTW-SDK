using System.IO;
using UnityEditor;
using UnityEngine;
using static Other.Scripts.AssetPaths;

namespace Other.Scripts {
    public class CreatePartController : EditorWindow {

        private static CreatePartController _window;

        [MenuItem("UTW/Create")]
        public static void ShowWindow() {
            _window = GetWindow<CreatePartController>("Create part");

            var size = new Vector2(600, 325);
            _window.minSize = size;
            _window.maxSize = size;
        }

        private void OnGUI() {
            GUILayout.Space(10);
            CreateHeader();
            GUILayout.Space(20);
            CreateButtons();
        }

        private void CreateHeader() {
            GUILayout.Label("Welcome in UTW - Development Kit", StyleUtils.Style(30, EditorStyles.boldLabel));
            GUILayout.Space(10);
            GUILayout.Label("This is a Unity project for creating tank parts for the UTW game.\n" +
                            "Choose from the following project templates\ndepending on what part of the tank you want to create.",
                StyleUtils.Style(13, EditorStyles.label));
        }

        private void CreateButtons() {
            GUILayout.Label("Create part", StyleUtils.Style(25, EditorStyles.boldLabel));
            GUILayout.Space(10);

            if (GUILayout.Button("HULL")) {
                string[] foldersToCopy = { "Materials", "Prefabs" };
                string path = EditorUtility.OpenFolderPanel("Select folder", HULL, "NewProject");
                CopyPrefab("HullTemplate", foldersToCopy, path, "Hull");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("TURRET")) {
            }

            GUILayout.Space(10);

            if (GUILayout.Button("WEAPONRY")) {
            }

            GUILayout.Space(10);

            if (GUILayout.Button("SUSPENSION")) {
            }
        }

        #region Copy

        private void CopyPrefab(string folderName, string[] foldersToCopy, string destPath, string prefabName) {
            string basePath = Path.Combine("Assets", "Other", folderName);
            CopyFolders(basePath, foldersToCopy, destPath);

            string relativePath = "Assets" + destPath.Substring(Application.dataPath.Length);
            string prefabPath = Path.Combine(relativePath, "Prefabs", prefabName);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + ".prefab");
            PrefabUtility.InstantiatePrefab(prefab);
        }

        private void CopyFolders(string basePath, string[] foldersToImport, string destPath) {
            foreach (var folder in foldersToImport) {
                string sourcePath = Path.Combine(basePath, folder);
                string fullDestPath = Path.Combine(destPath, folder);

                if (Directory.Exists(sourcePath)) {
                    CopyDirectory(sourcePath, fullDestPath);
                }
                else {
                    Debug.LogWarning($"Folder path does not exist: {sourcePath}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Selected folders copied successfully!");
        }

        private void CopyDirectory(string sourceDir, string destDir) {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir)) {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));

                if (File.Exists(destFile)) {
                    string existingContent = File.ReadAllText(destFile);
                    string newContent = File.ReadAllText(file);

                    File.WriteAllText(destFile, existingContent + "\n" + newContent);
                    Debug.Log($"Data appended to existing file: {destFile}");
                }
                else {
                    File.Copy(file, destFile, true);
                    Debug.Log($"File copied: {destFile}");
                }
            }

            foreach (var directory in Directory.GetDirectories(sourceDir)) {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(directory));
                CopyDirectory(directory, destSubDir);
            }
        }

        #endregion

    }
} //END