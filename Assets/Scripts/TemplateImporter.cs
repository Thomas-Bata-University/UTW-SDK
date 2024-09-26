using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class TemplateImporter : EditorWindow {

    private static TemplateImporter _window;

    [MenuItem("UTW/Template Importer")]
    public static void ShowWindow() {
        _window = GetWindow<TemplateImporter>("Create part");

        var size = new Vector2(600, 325);
        _window.minSize = size;
        _window.maxSize = size;
    }

    private void OnGUI() {
            GUILayout.Space(10);
            CreateHeader();
            GUILayout.Space(20);
            CreateImport();
            GUILayout.Space(20);
            CreateHint();
    }

    private void CreateHeader() {
        GUILayout.Label("Welcome in UTW - Development Kit", Style(30, EditorStyles.boldLabel));
        GUILayout.Space(10);
        GUILayout.Label("This is a Unity template for creating tank parts for the UTW game.\n" +
                        "Choose from the following project templates\ndepending on what part of the tank you want to create.",
            Style(13, EditorStyles.label));
    }

    private void CreateImport() {
        GUILayout.Label("Create part", Style(25, EditorStyles.boldLabel));
        GUILayout.Space(10);

        if (GUILayout.Button("HULL")) {
            string[] foldersToImport = { "Scenes" };
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

    private void CreateHint() {
        GUILayout.FlexibleSpace();
        GUILayout.Label("HINT: Hello.", EditorStyles.helpBox);
    }

    private GUIStyle Style(int size, GUIStyle style) {
         return new GUIStyle(style) {
            alignment = TextAnchor.MiddleCenter,
            fontSize = size
        };
    }

    #region Package

    /// <summary>
    /// We can copy data from Package to Assets if needed
    /// </summary>
    /// <param name="package"></param>
    /// <param name="foldersToImport"></param>
    private void CopyPackageFiles(string package, string[] foldersToImport) {
        string packageBasePath = Path.Combine("Packages", package, "Editor", "Content");

        foreach (var folder in foldersToImport) {
            string packagePath = Path.Combine(packageBasePath, folder);
            string destPath = Path.Combine(Application.dataPath, folder);

            if (Directory.Exists(packagePath)) {
                CopyDirectory(packagePath, destPath);
            }
            else {
                Debug.LogWarning($"Folder path does not exist: {packagePath}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Selected folders copied successfully!");
    }

    private void CopyDirectory(string sourceDir, string destDir) {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir)) {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            string destMetaFile = destFile + ".meta";

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

            string sourceMetaFile = file + ".meta";
            if (File.Exists(sourceMetaFile)) {
                CopyAndModifyMetaFile(sourceMetaFile, destMetaFile);
            }
        }

        foreach (var directory in Directory.GetDirectories(sourceDir)) {
            string destSubDir = Path.Combine(destDir, Path.GetFileName(directory));
            CopyDirectory(directory, destSubDir);
        }
    }

    private void CopyAndModifyMetaFile(string sourceMetaFile, string destMetaFile) {
        File.Copy(sourceMetaFile, destMetaFile, true);
        ModifyMetaFile(destMetaFile);
    }

    private void ModifyMetaFile(string metaFilePath) {
        string content = File.ReadAllText(metaFilePath);
        string guidPattern = @"guid: (\w+)";
        Match match = Regex.Match(content, guidPattern);

        if (match.Success) {
            string oldGuid = match.Groups[1].Value;
            string newGuid = Guid.NewGuid().ToString("N");

            content = content.Replace(oldGuid, newGuid);
            File.WriteAllText(metaFilePath, content);

            Debug.Log($"Modified GUID in meta file: {metaFilePath}");
        }
    }

    #endregion

} //END