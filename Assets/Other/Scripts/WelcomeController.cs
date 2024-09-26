using UnityEditor;
using UnityEngine;

namespace Other.Scripts {
    public class WelcomeController : EditorWindow {

        private static WelcomeController _window;

        [MenuItem("UTW/Welcome", false, 1)]
        public static void ShowWindow() {
            _window = GetWindow<WelcomeController>("Welcome");

            var size = new Vector2(600, 325);
            _window.minSize = size;
            _window.maxSize = size;
        }

        private void OnGUI() {
            CreateHeader();
            GUILayout.FlexibleSpace();
            CreateButton();
            GUILayout.FlexibleSpace();
        }

        private void CreateHeader() {
            GUILayout.Label("Welcome in UTW - Development Kit", StyleUtils.Style(30, EditorStyles.boldLabel));
            GUILayout.Space(10);
            GUILayout.Label("This is a Unity project for creating tank parts for the UTW game.",
                StyleUtils.Style(13, EditorStyles.label));
        }

        private void CreateButton() {
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create first project", GUILayout.Width(200))) {
                CreateProjectController.ShowWindow();
                Close();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

    }
} //END