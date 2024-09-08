using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[InitializeOnLoad]
public class PackageCheck {

    public static bool PackageFound = false;

    //Package names
    public const string HULL_TEMPLATE = "com.utw.hull_template";
    public const string TURRET_TEMPLATE = "com.utw.turret_template";
    public const string WEAPONRY_TEMPLATE = "com.utw.weaponry_template";
    public const string SUSPENSION_TEMPLATE = "com.utw.suspension_template";

    public static readonly string[] RequiredPackages = {
        HULL_TEMPLATE,
        TURRET_TEMPLATE,
        WEAPONRY_TEMPLATE,
        SUSPENSION_TEMPLATE,
    };

    static PackageCheck() {
        EditorApplication.delayCall += CheckRequiredPackages;
    }

    private static void CheckRequiredPackages() {
        ListRequest listRequest = Client.List();
        while (!listRequest.IsCompleted) {
            // Waiting
        }

        if (listRequest.Status == StatusCode.Success) {
            foreach (string packageName in RequiredPackages) {
                foreach (var package in listRequest.Result) {
                    if (package.name == packageName) {
                        PackageFound = true;
                        break;
                    }
                }
            }
        }
        else if (listRequest.Status >= StatusCode.Failure) {
            Debug.LogError("Failed to list packages: " + listRequest.Error.message);
        }
    }

} //END