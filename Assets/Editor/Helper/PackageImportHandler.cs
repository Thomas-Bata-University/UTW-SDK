using System.IO;
using Editor.Const;
using Editor.Core;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor.Helper {
    [InitializeOnLoad]
    public class PackageImportHandler {

        static PackageImportHandler() {
            AssetDatabase.importPackageCompleted += OnPackageImportCompleted;
        }

        static void OnPackageImportCompleted(string packageName) {
            AddRequiredPackages();
            ColorLogger.Log($"Package {packageName} imported!");
            
            LayoutChanger.ChangeLayout();
            OpenMyEditorWindow();
            CreateDirectories();
        }
        
        private static void AddRequiredPackages() {
            AddPackage("https://github.com/Thomas-Bata-University/UTW-Library.git");
        }

        private static void AddPackage(string packageName) {
            var request = Client.Add(packageName);
            EditorApplication.update += () => {
                if (request.IsCompleted) {
                    if (request.Status == StatusCode.Success) {
                        Debug.Log($"Package added: {request.Result.packageId}");
                    } else {
                        Debug.LogError($"Failed to add package {packageName}: {request.Error.message}");
                    }

                    EditorApplication.update -= () => { };
                }
            };
        }

        private static void CreateDirectories() {
            if (!Directory.Exists(AssetPaths.ASSET_BUNDLE))
                Directory.CreateDirectory(AssetPaths.ASSET_BUNDLE);

            if (!Directory.Exists(AssetPaths.PROJECT))
                Directory.CreateDirectory(AssetPaths.PROJECT);
            
            if (!Directory.Exists(AssetPaths.PROJECT_DATA))
                Directory.CreateDirectory(AssetPaths.PROJECT_DATA);
        }

        private static void OpenMyEditorWindow() {
            EditorSceneManager.OpenScene("Assets/Other/Scenes/MainScene.unity");
            WelcomeController window = (WelcomeController)EditorWindow.GetWindow(typeof(WelcomeController));
            StyleUtils.SetSize(window, new Vector2(600, 325));
            StyleUtils.SetMiddle(window);
            window.Show();
        }

    }
}