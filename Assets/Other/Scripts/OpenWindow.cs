using Other.Scripts;
using UnityEditor;

[InitializeOnLoad]
public class OpenWindow {

    static OpenWindow() {
        EditorApplication.delayCall += OpenMyEditorWindow;
    }

    private static void OpenMyEditorWindow() {
        // CreatePartController window = (CreatePartController)EditorWindow.GetWindow(typeof(CreatePartController));
        // window.Show();
    }

} //END