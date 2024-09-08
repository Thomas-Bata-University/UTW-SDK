using System.Net;
using UnityEditor;
using UnityEngine;

public class TemplateImporter : EditorWindow {

    [MenuItem("Tools/Template Importer")]
    public static void ShowWindow() {
        GetWindow<TemplateImporter>("Template Importer");
    }

    private void OnGUI() {
        GUILayout.Label("Import Project Template", EditorStyles.boldLabel);

        if (GUILayout.Button("Import HULL template")) {
            ImportTemplate("https://github.com/Thomas-Bata-University/UTW-Templates/blob/main/packages/Hull_Template.unitypackage");
        }

        // Add more buttons for other templates
    }

    private void ImportTemplate(string url) {
        string packagePath = Application.dataPath + "/DownloadedTemplate.unitypackage";

        using (WebClient client = new WebClient()) {
            client.DownloadFile(url, packagePath);
        }

        AssetDatabase.ImportPackage(packagePath, true);

        // Add your custom setup code here (e.g., configuring scenes, assets, etc.)
    }

} //END