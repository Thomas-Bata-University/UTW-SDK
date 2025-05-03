using System.IO;
using UnityEditor;

namespace Editor.Helper {
    public class UnityPackageExporter {

        [MenuItem("Tools/Export unitypackage")]
        public static void Export() {
            string[] assetsToInclude = {
                "Assets",
                "Packages"
            };

            string exportDirectory = "C:/GameDev";
            if (!Directory.Exists(exportDirectory)) {
                Directory.CreateDirectory(exportDirectory);
            }

            string exportPath = Path.Combine(exportDirectory, "UTW-SDK.unitypackage");

            AssetDatabase.ExportPackage(
                assetsToInclude,
                exportPath,
                ExportPackageOptions.Interactive | ExportPackageOptions.Recurse
            );

            ColorLogger.LogFormatted(
                "Package exported to '{0}' successfully.",
                new[] { exportPath },
                new[] { "green" },
                new[] { true }
            );
        }

    }
}