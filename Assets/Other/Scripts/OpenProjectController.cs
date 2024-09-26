using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Other.Scripts.AssetPaths;

namespace Other.Scripts {
    public class OpenProjectController : EditorWindow {

        private static OpenProjectController _window;
        private const string ProjectPath = "OpenedFolderPath";
        public static bool isOpenedProject = false;

        private int _selectedTab = 0;
        private string[] _tabNames = { "Hull", "Turret", "Weaponry", "Suspension" };
        private string[] _folderPaths = { HULL, TURRET, WEAPONRY, SUSPENSION };
        private Dictionary<string, string[]> _folderContents = new();

        [MenuItem("UTW/Open project", false, 3)]
        public static void ShowWindow() {
            _window = GetWindow<OpenProjectController>("Open project");

            var size = new Vector2(700, 325);
            _window.minSize = size;
            _window.maxSize = size;
        }

        private void OnGUI() {
            LoadFolderContents();

            string projectPath = EditorPrefs.GetString(ProjectPath, "");
            isOpenedProject = !string.IsNullOrEmpty(projectPath);
            if (isOpenedProject) {
                DisplayOpenedProject(projectPath);
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
                    Debug.LogWarning($"Folder does not exist: {path}");
                    _folderContents[path] = new string[0];
                }
            }
        }

        #region Display

        private void DisplayOpenedProject(string projectPath) {
            GUILayout.Space(10);
            GUILayout.Label(Path.GetFileName(projectPath), StyleUtils.Style(25, EditorStyles.boldLabel));
            GUILayout.Space(20);

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(GUILayout.Width(position.width));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save", GUILayout.Width(100))) {
            }

            if (GUILayout.Button("Close", GUILayout.Width(100))) {
                EditorPrefs.DeleteKey(ProjectPath);
                isOpenedProject = false;
            }

            GUILayout.EndHorizontal();
        }

        private void DisplayFolderContents(string folderPath) {
            if (_folderContents.ContainsKey(folderPath)) {
                //Header
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(200));
                GUILayout.Label("Size", EditorStyles.boldLabel, GUILayout.Width(100));
                GUILayout.Label("Last update", EditorStyles.boldLabel, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                foreach (string subFolder in _folderContents[folderPath]) {
                    if (!Directory.Exists(subFolder)) return;
                    string folderName = Path.GetFileName(subFolder);
                    long folderSize = GetFolderSize(subFolder);
                    string lastModified = Directory.GetLastWriteTime(subFolder).ToString("dd.MM.yyyy HH:mm");

                    GUILayout.BeginHorizontal();

                    GUILayout.Label(folderName, GUILayout.Width(200));

                    GUILayout.Label(FormatSize(folderSize), GUILayout.Width(100));

                    GUILayout.Label(lastModified, GUILayout.Width(150));

                    if (GUILayout.Button("Open", GUILayout.Width(100))) {
                        OpenProject(subFolder);
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(100))) {
                        DeleteProject(subFolder);
                    }

                    GUILayout.EndHorizontal();
                }
            }
            else {
                GUILayout.Label("No folders found.");
            }

            CreateButton();
        }

        private void CreateButton() {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create new project", GUILayout.Width(200))) {
                CreateProjectController.ShowWindow();
                Close();
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

        private void OpenProject(string subFolder) {
            EditorPrefs.SetString(ProjectPath, subFolder);
        }

        private void DeleteProject(string subFolder) {
            AssetDatabase.DeleteAsset(subFolder);
            Debug.Log($"Deleted {subFolder}");
            Repaint();
        }

    }
} //END