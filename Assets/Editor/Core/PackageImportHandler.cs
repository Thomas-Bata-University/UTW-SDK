using System.IO;
using Editor.Const;
using Editor.Helper;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor.Core {
    [InitializeOnLoad]
    public class PackageImportHandler {

        static PackageImportHandler() {
            AssetDatabase.importPackageCompleted += OnPackageImportCompleted;
        }

        static void OnPackageImportCompleted(string packageName) {
            ColorLogger.Log($"Package {packageName} imported!");
            LayoutChanger.ChangeLayout();
            OpenMyEditorWindow();
            CreateDirectories();
        }

        private static void CreateDirectories() {
            if (!Directory.Exists(AssetPaths.ASSET_BUNDLE))
                Directory.CreateDirectory(AssetPaths.ASSET_BUNDLE);

            if (!Directory.Exists(AssetPaths.PROJECT))
                Directory.CreateDirectory(AssetPaths.PROJECT);
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