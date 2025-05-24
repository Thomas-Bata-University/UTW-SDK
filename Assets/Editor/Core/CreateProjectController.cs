using System.IO;
using Editor.Enums;
using Editor.Helper;
using UnityEditor;
using UnityEngine;
using static Editor.Const.AssetPaths;

namespace Editor.Core {
    public class CreateProjectController : EditorWindow {

        private static CreateProjectController _window;
        private string _projectName = "";

        [MenuItem("UTW/Create project", false, 2)]
        public static void ShowWindow() {
            _window = GetWindow<CreateProjectController>("Create project");

            StyleUtils.SetSize(_window, new Vector2(600, 350));
            StyleUtils.SetMiddle(_window);
        }

        private void OnGUI() {
            if (OpenProjectController.IsOpenedProject) {
                OpenProjectController.ShowWindow();
                ColorLogger.LogWarning("Cannot create a new project when another one is open.");
                Close();
                return;
            }

            GUILayout.Space(10);
            CreateHeader();
            GUILayout.Space(10);
            GUILayout.Label("Enter project name:");
            _projectName = GUILayout.TextField(_projectName, GUILayout.Height(25));
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

        private void CreateButtons() {
            if (GUILayout.Button("HULL")) {
                TryCreateProject(HULL, "HullTemplate", "Hull", TankPart.HULL);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("TURRET")) {
                TryCreateProject(TURRET, "TurretTemplate", "Turret", TankPart.TURRET);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("WEAPONRY")) {
                TryCreateProject(WEAPONRY, "WeaponryTemplate", "Weaponry", TankPart.WEAPONRY);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("SUSPENSION")) {
                TryCreateProject(SUSPENSION, "SuspensionTemplate", "Suspension", TankPart.SUSPENSION);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("MAP")) {
                TryCreateProject(MAP, "MapTemplate", "Map", TankPart.NONE);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Open project", GUILayout.Width(200))) {
                OpenProjectController.ShowWindow();
                Close();
            }
        }

        private void TryCreateProject(string basePath, string folderName, string prefabName, TankPart tankPart) {
            if (string.IsNullOrWhiteSpace(_projectName)) {
                ColorLogger.LogWarning("Please enter a valid project name.");
                return;
            }

            string targetPath = Path.Combine(basePath, _projectName);

            if (Directory.Exists(targetPath)) {
                ColorLogger.LogFormatted(
                    "Project with name {0} already exists in this category!",
                    new[] { _projectName },
                    new[] { "red" },
                    new[] { true },
                    ColorLogger.LogLevel.Warning
                );
                return;
            }

            Directory.CreateDirectory(targetPath);

            string[] foldersToCopy = { "Prefabs" };
            CopyPrefab(folderName, foldersToCopy, targetPath, prefabName, tankPart);
        }

        #region Copy

        private void CopyPrefab(string folderName, string[] foldersToCopy, string projectPath, string prefabName,
            TankPart tankPart) {
            if (string.IsNullOrEmpty(projectPath)) return;

            string basePath = Path.Combine(TEMPLATE, folderName);
            CopyFolders(basePath, foldersToCopy, projectPath);

            string prefabPath = Path.Combine(projectPath, "Prefabs", prefabName);

            string assetBundleName = "default";
            ProjectManager.CreatePrefab(prefabPath, assetBundleName);

            string metadataPath = ProjectManager.CreateMetadata(
                new ProjectManager.Metadata(prefabPath, projectPath, tankPart, assetBundleName));

            OpenProjectController.OpenProject(metadataPath);
            Close();
            ColorLogger.LogFormatted("Project {0} successfully created.",
                Path.GetFileName(projectPath), "green", true);
        }

        private void CopyFolders(string basePath, string[] foldersToImport, string destPath) {
            foreach (var folder in foldersToImport) {
                string sourcePath = Path.Combine(basePath, folder);
                string fullDestPath = Path.Combine(destPath, folder);

                if (Directory.Exists(sourcePath)) {
                    CopyDirectory(sourcePath, fullDestPath);
                }
                else {
                    ColorLogger.LogFormatted("Folder path does not exist: {0}",
                        new[] { sourcePath },
                        new[] { "yellow" },
                        new[] { true },
                        ColorLogger.LogLevel.Warning
                    );
                }
            }

            AssetDatabase.Refresh();
        }

        private void CopyDirectory(string sourceDir, string destDir) {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir)) {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));

                if (file.EndsWith(".meta")) //Ignore meta files - automatically creates new
                    continue;

                if (File.Exists(destFile)) {
                    string existingContent = File.ReadAllText(destFile);
                    string newContent = File.ReadAllText(file);

                    File.WriteAllText(destFile, existingContent + "\n" + newContent);
                    ColorLogger.Log($"Data appended to existing file: {destFile}");
                }
                else {
                    File.Copy(file, destFile, true);
                }
            }

            foreach (var directory in Directory.GetDirectories(sourceDir)) {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(directory));
                CopyDirectory(directory, destSubDir);
            }
        }

        #endregion

    }
}