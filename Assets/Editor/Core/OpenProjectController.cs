using System.Collections.Generic;
using System.IO;
using Editor.Helper;
using Editor.Task;
using UnityEditor;
using UnityEngine;
using static Editor.Const.AssetPaths;

namespace Editor.Core {
    public class OpenProjectController : EditorWindow {

        private static OpenProjectController _window;
        private const string MetadataPath = "OpenedProjectMetadata";
        public static bool IsOpenedProject = false;
        public static ProjectManager.Metadata MetaData;

        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private string[] _tabNames = { "Hull", "Turret", "Weaponry", "Suspension", "Maps" };
        private string[] _folderPaths = { HULL, TURRET, WEAPONRY, SUSPENSION, MAP };
        private Dictionary<string, string[]> _folderContents = new();

        [MenuItem("UTW/Open project", false, 3)]
        public static void ShowWindow() {
            _window = GetWindow<OpenProjectController>("Open project");
        }

        private void OnGUI() {
            LoadFolderContents();
            // EditorPrefs.SetString(MetadataPath, null);

            string metadataPath = EditorPrefs.GetString(MetadataPath, "");
            ProjectManager.Metadata metadata = ProjectManager.GetMetadata(metadataPath);
            IsOpenedProject =
                metadata is not null; //TODO musim to lepe upravit, zjistit jak poznat ze je otevren projekt
            if (IsOpenedProject) {
                MetaData = metadata;
                DisplayOpenedProject(metadata);
                return;
            }

            //Tab bar
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            GUILayout.Space(10);

            //Display contents for the selected tab
            DisplayFolderContents(_folderPaths[_selectedTab]);
        }

        private void LoadFolderContents() {
            foreach (string path in _folderPaths) {
                if (Directory.Exists(path)) {
                    string[] subFolders = Directory.GetDirectories(path);
                    _folderContents[path] = subFolders;
                }
                else {
                    _folderContents[path] = new string[0];
                }
            }
        }

        #region Display

        private void DisplayOpenedProject(ProjectManager.Metadata metadata) {
            GUILayout.Space(10);
            GUILayout.Label(metadata.projectName, StyleUtils.Style(25, EditorStyles.boldLabel));
            GUILayout.Space(20);

            //Show tasks
            TaskListWindow.DrawTasks(metadata);

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(GUILayout.Width(position.width));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save", GUILayout.Width(100))) {
                ProjectManager.OverridePrefab(metadata);
                ColorLogger.LogFormatted("Project {0} successfully saved.", metadata.projectName, "green", true);
            }

            if (GUILayout.Button("Close", GUILayout.Width(100))) {
                EditorPrefs.DeleteKey(MetadataPath);
                IsOpenedProject = false;
                ProjectManager.RemovePrefab(metadata);
                MetaData = null;
                TaskListWindow.tasks = null;
                ColorLogger.LogFormatted("Project {0} successfully closed.", metadata.projectName, "green", true);
            }

            GUILayout.EndHorizontal();
        }

        private void DisplayFolderContents(string folderPath) {
            if (_folderContents.ContainsKey(folderPath)) {
                GUILayout.BeginHorizontal(EditorStyles.boldLabel);
                GUILayout.Label("Project name", GUILayout.Width(position.width * 0.3f));
                GUILayout.Label("Size", GUILayout.Width(position.width * 0.15f));
                GUILayout.Label("Last update", GUILayout.Width(position.width * 0.25f));
                GUILayout.FlexibleSpace();
                GUILayout.Label("", GUILayout.Width(200)); // prostor pro tlačítka
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                float scrollViewHeight = position.height - 100;
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(scrollViewHeight));

                foreach (string subFolder in _folderContents[folderPath]) {
                    if (!Directory.Exists(subFolder)) continue;

                    string folderName = Path.GetFileName(subFolder);
                    long folderSize = GetFolderSize(subFolder);
                    string lastModified = Directory.GetLastWriteTime(subFolder).ToString("dd.MM.yyyy HH:mm");

                    GUILayout.BeginHorizontal("box");

                    GUILayout.Label(folderName, GUILayout.Width(position.width * 0.3f));
                    GUILayout.Label(FormatSize(folderSize), GUILayout.Width(position.width * 0.15f));
                    GUILayout.Label(lastModified, GUILayout.Width(position.width * 0.25f));

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Open", GUILayout.Width(80))) {
                        string metadataPath = subFolder + METADATA;
                        OpenProject(metadataPath);
                        MetaData = ProjectManager.CreatePrefabWithMetadata(metadataPath);
                        ColorLogger.LogFormatted("Project {0} successfully opened.", MetaData.projectName, "green",
                            true);
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(80))) {
                        string projectName = DeleteProject(subFolder);
                        ColorLogger.LogFormatted("Project {0} successfully deleted.", projectName, "green", true);
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
            }
            else {
                GUILayout.Label("No folders found.");
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create new project", GUILayout.Width(200))) {
                CreateProjectController.ShowWindow();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        private void CreateButton() {
            if (GUILayout.Button("Create new project", GUILayout.Width(200))) {
                CreateProjectController.ShowWindow();
            }
        }

        private long GetFolderSize(string folderPath) {
            long totalSize = 0;
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files) {
                FileInfo fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }

            return totalSize;
        }

        private string FormatSize(long sizeInBytes) {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;

            while (sizeInBytes >= 1024 && order < sizes.Length - 1) {
                order++;
                sizeInBytes /= 1024;
            }

            return $"{sizeInBytes:0.##} {sizes[order]}";
        }

        #endregion

        public static void OpenProject(string metadataPath) {
            EditorPrefs.SetString(MetadataPath, metadataPath);
            IsOpenedProject = true;
        }

        private string DeleteProject(string subFolder) {
            AssetDatabase.DeleteAsset(subFolder);
            MetaData = null;
            Repaint();
            return Path.GetFileName(subFolder);
        }

    }
} //END