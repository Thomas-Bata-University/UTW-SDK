using UnityEditor;

[InitializeOnLoad]
public class OpenWindow {

    static OpenWindow() {
        EditorApplication.delayCall += OpenMyEditorWindow;
    }

    private static void OpenMyEditorWindow() {
        TemplateImporter window = (TemplateImporter)EditorWindow.GetWindow(typeof(TemplateImporter));
        window.Show();
    }

} //END