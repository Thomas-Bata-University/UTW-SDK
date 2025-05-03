using Editor.Helper;
using UnityEditor;
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
        }

        private static void OpenMyEditorWindow() {
            WelcomeController window = (WelcomeController)EditorWindow.GetWindow(typeof(WelcomeController));
            StyleUtils.SetSize(window, new Vector2(600, 325));
            StyleUtils.SetMiddle(window);
            window.Show();
        }

    }
}