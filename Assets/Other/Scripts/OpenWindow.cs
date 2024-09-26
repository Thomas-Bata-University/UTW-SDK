using UnityEditor;

namespace Other.Scripts {
    [InitializeOnLoad]
    public class OpenWindow {

        private const string FirstOpen = "FirstOpen";

        static OpenWindow() {
            EditorApplication.delayCall += OpenMyEditorWindow;
        }

        private static void OpenMyEditorWindow() {
            if (EditorPrefs.GetBool(FirstOpen, true)) {
                WelcomeController window = (WelcomeController)EditorWindow.GetWindow(typeof(WelcomeController));
                window.Show();
                EditorPrefs.SetBool(FirstOpen, false);
            }
        }

    }
} //END