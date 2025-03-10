using System.IO;
using UnityEditor;
using UnityEngine;
using static Other.CoreScripts.AssetPaths;

namespace Other.CoreScripts {
    public class CreateProjectController : EditorWindow {

        private static CreateProjectController _window;

        [MenuItem("UTW/Create project", false, 2)]
        public static void ShowWindow() {
            _window = GetWindow<CreateProjectController>("Create project");

            StyleUtils.SetSize(_window, new Vector2(600, 325));
            StyleUtils.SetMiddle(_window);
        }

        private void OnGUI() {
            if (OpenProjectController.IsOpenedProject) {
                OpenProjectController.ShowWindow();
                Debug.LogWarning("Cannot create a new project when another one is open.");
                Close();
            }

            GUILayout.Space(10);
            CreateHeader();
            GUILayout.Space(20);
            CreateButtons();
        }

        private void CreateHeader() {
            GUILayout.Label("Create project", StyleUtils.Style(30, EditorStyles.boldLabel));
            GUILayout.Space(10);
            GUILayout.Label(
                "Choose from the following project templates\ndepending on what part of the tank you want to create.",
                StyleUtils.Style(13, EditorStyles.label));
        }

        /// <summary>
        /// Set which folders you want to copy -> foldersToCopy = { "Materials", "Prefabs" };
        /// </summary>
        private void CreateButtons() {
            if (GUILayout.Button("HULL")) {
                string[] foldersToCopy = { "Prefabs" };
                Create(foldersToCopy, HULL, "HullTemplate", "Hull", TankPart.HULL);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("TURRET")) {
                string[] foldersToCopy = { "Prefabs" };
                Create(foldersToCopy, TURRET, "TurretTemplate", "Turret", TankPart.TURRET);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("WEAPONRY")) {
                string[] foldersToCopy = { "Prefabs" };
                Create(foldersToCopy, WEAPONRY, "WeaponryTemplate", "Weaponry", TankPart.WEAPONRY);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("SUSPENSION")) {
                string[] foldersToCopy = { "Prefabs" };
                Create(foldersToCopy, SUSPENSION, "SuspensionTemplate", "Suspension", TankPart.SUSPENSION);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open project", GUILayout.Width(200))) {
                OpenProjectController.ShowWindow();
                Close();
            }
        }

        private void Create(string[] foldersToCopy, string path, string folderName, string prefabName, TankPart tankPart) {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string selectedPath = EditorUtility.OpenFolderPanel("Select folder", path, "");
            CopyPrefab(folderName, foldersToCopy, selectedPath, prefabName, tankPart);
        }

        #region Copy

        private void CopyPrefab(string folderName, string[] foldersToCopy, string destPath, string prefabName, TankPart tankPart) {
            if (string.IsNullOrEmpty(destPath)) return;

            string basePath = Path.Combine(TEMPLATE, folderName);
            CopyFolders(basePath, foldersToCopy, destPath);

            string relativePath = "Assets" + destPath.Substring(Application.dataPath.Length);
            string prefabPath = relativePath + "/Prefabs/" + prefabName;
            ProjectManager.CreatePrefab(prefabPath);
            string metadataPath = ProjectManager.CreateMetadata(new ProjectManager.Metadata(prefabPath, relativePath, tankPart));
            OpenProjectController.OpenProject(metadataPath);
            Close();
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