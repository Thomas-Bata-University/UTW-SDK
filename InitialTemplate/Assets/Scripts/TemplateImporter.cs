using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class TemplateImporter : EditorWindow {

    [MenuItem("Tools/Template Importer")]
    public static void ShowWindow() {
        GetWindow<TemplateImporter>("Template Importer");
    }

    private void OnGUI() {
        GUILayout.Label("Import Project Template", EditorStyles.boldLabel);

        if (GUILayout.Button("Import HULL template")) {
            DownloadPackage("https://github.com/Thomas-Bata-University/UTW-Hull-dev-package.git");
        }

        // Add more buttons for other templates
    }

    private void DownloadPackage(string url) {
        AddRequest request = Client.Add(url);

        while (!request.IsCompleted) {
            //Can add progress bar
        }

        if (request.Status == StatusCode.Success) {
            Debug.Log("Package successfully imported.");
        }
        else {
            Debug.LogError($"An error occured during importing: {request.Error.message}");
        }
    }

} //END