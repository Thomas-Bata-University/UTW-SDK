using System.IO;
using Editor.Const;
using UnityEditor;
using UnityEngine;

namespace Editor.AssetBundle {
    public class AssetBundleBuilder {

        public static void Build() {
            string bundleDirectory = AssetPaths.ASSET_BUNDLE;

            if (!Directory.Exists(bundleDirectory)) {
                Directory.CreateDirectory(bundleDirectory);
            }

            BuildPipeline.BuildAssetBundles(bundleDirectory, BuildAssetBundleOptions.None,
                BuildTarget.StandaloneWindows);

            AssetDatabase.Refresh();
            Debug.Log("AssetBundles successfully generated.");
        }

    }
} //END