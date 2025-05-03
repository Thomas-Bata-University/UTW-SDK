using UnityEditor;
using UnityEngine;

namespace Editor.Helper {
    public class ClearAllAssetBundleNames {

        [MenuItem("Tools/Clear AssetBundle Names")]
        private static void ClearAllAssetBundles() {
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
            int clearedCount = 0;

            foreach (string path in assetPaths) {
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName)) {
                    ColorLogger.Log($"Removing AssetBundle name from: {path}");
                    importer.assetBundleName = "";
                    clearedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ColorLogger.Log($"Successfuly removed AssetBundle name from {clearedCount} assets.");
        }

    }
} //END