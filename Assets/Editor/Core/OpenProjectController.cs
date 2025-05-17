using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        //Data table
        private string _searchTerm = "";

        private enum SortColumn {

            Name,
            Size,
            Date

        }

        private SortColumn _sortBy = SortColumn.Name;
        private bool _sortAscending = true;


        [MenuItem("UTW/Open project", false, 3)]
        public static void ShowWindow() {
            _window = GetWindow<OpenProjectController>("Open project");
        }

        private void OnGUI() {
            LoadFolderContents();
            
            string metadataPath = EditorPrefs.GetString(MetadataPath, "");
            ProjectManager.Metadata metadata = ProjectManager.GetMetadata(metadataPath);
            IsOpenedProject =
                metadata is not null;
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
                DestroyPreviewObjects();
                ColorLogger.LogFormatted("Project {0} successfully closed.", metadata.projectName, "green", true);
            }

            GUILayout.EndHorizontal();
        }
        
        private static void DestroyPreviewObjects() {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in allObjects) {
                if (go.name.StartsWith("Preview_") && go.hideFlags == HideFlags.HideAndDontSave) {
                    DestroyImmediate(go);
                }
            }
        }

        private void DisplayFolderContents(string folderPath) {
            if (!_folderContents.ContainsKey(folderPath) || _folderContents[folderPath].Length == 0) {
                GUILayout.Label("No projects.", new GUIStyle(EditorStyles.centeredGreyMiniLabel));
                CreateButton();
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50));
            _searchTerm = GUILayout.TextField(_searchTerm);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(EditorStyles.boldLabel);

            if (GUILayout.Button(GetSortableLabel("Project name", SortColumn.Name), GUILayout.Width(position.width * 0.3f)))
                ToggleSort(SortColumn.Name);

            if (GUILayout.Button(GetSortableLabel("Size", SortColumn.Size), GUILayout.Width(position.width * 0.15f)))
                ToggleSort(SortColumn.Size);

            if (GUILayout.Button(GetSortableLabel("Last update", SortColumn.Date), GUILayout.Width(position.width * 0.25f)))
                ToggleSort(SortColumn.Date);

            GUILayout.FlexibleSpace();
            GUILayout.Label("", GUILayout.Width(200)); // Buttons
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            var filtered = new List<string>(_folderContents[folderPath]);
            if (!string.IsNullOrEmpty(_searchTerm))
                filtered = filtered.Where(f => Path.GetFileName(f).ToLower().Contains(_searchTerm.ToLower())).ToList();

            filtered = SortFolders(filtered, _sortBy, _sortAscending);

            float scrollViewHeight = position.height - 130;
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(scrollViewHeight));

            foreach (string subFolder in filtered) {
                if (!Directory.Exists(subFolder)) continue;

                string folderName = Path.GetFileName(subFolder);
                long folderSize = GetFolderSize(subFolder);
                DateTime modified = Directory.GetLastWriteTime(subFolder);
                string lastModified = modified.ToString("dd.MM.yyyy HH:mm");

                GUILayout.BeginHorizontal("box");
                GUILayout.Label(folderName, GUILayout.Width(position.width * 0.3f));
                GUILayout.Label(FormatSize(folderSize), GUILayout.Width(position.width * 0.15f));
                GUILayout.Label(lastModified, GUILayout.Width(position.width * 0.25f));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Open", GUILayout.Width(80))) {
                    string metadataPath = subFolder + METADATA;
                    OpenProject(metadataPath);
                    MetaData = ProjectManager.CreatePrefabWithMetadata(metadataPath);
                    ColorLogger.LogFormatted("Project {0} successfully opened.", MetaData.projectName, "green", true);
                }

                if (GUILayout.Button("Delete", GUILayout.Width(80))) {
                    string projectName = DeleteProject(subFolder);
                    ColorLogger.LogFormatted("Project {0} successfully deleted.", projectName, "green", true);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            CreateButton();
        }
        
        # region Filter and sort
        
        private void ToggleSort(SortColumn column) {
            if (_sortBy == column) {
                _sortAscending = !_sortAscending;
            } else {
                _sortBy = column;
                _sortAscending = true;
            }
        }

        private List<string> SortFolders(List<string> folders, SortColumn sortBy, bool ascending) {
            switch (sortBy) {
                case SortColumn.Name:
                    return ascending
                        ? folders.OrderBy(Path.GetFileName).ToList()
                        : folders.OrderByDescending(Path.GetFileName).ToList();
                case SortColumn.Size:
                    return ascending
                        ? folders.OrderBy(GetFolderSize).ToList()
                        : folders.OrderByDescending(GetFolderSize).ToList();
                case SortColumn.Date:
                    return ascending
                        ? folders.OrderBy(f => Directory.GetLastWriteTime(f)).ToList()
                        : folders.OrderByDescending(f => Directory.GetLastWriteTime(f)).ToList();
                default:
                    return folders;
            }
        }
        
        private string GetSortableLabel(string label, SortColumn column) {
            if (_sortBy != column) return label;
            return label + (_sortAscending ? " ↑" : " ↓");
        }
        
        # endregion

        private void CreateButton() {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create new project", GUILayout.Width(200))) {
                CreateProjectController.ShowWindow();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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