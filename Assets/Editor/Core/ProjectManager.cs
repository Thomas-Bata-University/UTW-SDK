using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Editor.Core {
    public class ProjectManager {

        public static string CreateMetadata(Metadata metadata) {
            string json = JsonUtility.ToJson(metadata);

            string path = metadata.projectPath + AssetPaths.METADATA;
            File.WriteAllText(path, json);

            Debug.Log($"Metadata created to {path}");
            OpenProjectController.MetaData = metadata;

            return path;
        }

        public static void UpdateMetadata(Metadata metadata) {
            CreateMetadata(metadata);
        }

        public static Metadata GetMetadata(string path) {
            if (string.IsNullOrEmpty(path)) return null;
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<Metadata>(json);
        }

        public static Metadata CreatePrefabWithMetadata(string metadataPath) {
            Metadata metadata = GetMetadata(metadataPath);
            CreatePrefab(metadata.prefabPath, metadata.assetBundle);
            return metadata;
        }

        public static void CreatePrefab(string prefabPath, string name) {
            string fullPath = prefabPath + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.tag = Tags.UNTAGGED;
            PrefabUtility.SaveAsPrefabAsset(instance, fullPath);
            AssetDatabase.SaveAssets();
            SetAssetBundle(fullPath, name);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        public static void OverridePrefab(Metadata metadata) {
            GameObject prefab = GameObject.Find(metadata.prefabName);
            string prefabPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(prefab));
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            string prefabPathAsset = AssetDatabase.GetAssetPath(prefabAsset);
            CopyData(prefabAsset, prefab);
            PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, prefabPath, InteractionMode.AutomatedAction);
            SetAssetBundle(prefabPathAsset, metadata.assetBundle);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            UpdateMetadata(metadata);
        }

        public static void RemovePrefab(Metadata metadata) {
            GameObject prefab = GameObject.Find(metadata.prefabName);
            Object.DestroyImmediate(prefab);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        private static void SetAssetBundle(string assetPath, string name) {
            Debug.Log(assetPath);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null && importer.assetBundleName != name) {
                importer.assetBundleName = name;
                importer.SaveAndReimport();
            }
        }

        private static void CopyData(GameObject prefabAsset, GameObject prefabInstance) {
            Transform prefabTransform = prefabAsset.transform;
            Transform instanceTransform = prefabInstance.transform;

            prefabTransform.position = instanceTransform.position;
            prefabTransform.rotation = instanceTransform.rotation;
            prefabTransform.localScale = instanceTransform.localScale;
        }

        [Serializable]
        public class Metadata {

            public string projectName;
            public string projectPath;

            public string prefabName;
            public string prefabPath;
            public string tankPart;
            public string assetBundle;

            public bool isMap;

            public string created;

            public Metadata(string prefabPath, string projectPath, TankPart tankPart) {
                this.projectName = Path.GetFileName(projectPath);
                this.projectPath = projectPath;
                this.prefabName = Path.GetFileName(prefabPath);
                this.prefabPath = prefabPath;
                this.created = DateTime.Now.ToString("yyyy MMMM dd", new CultureInfo("en-US"));
                this.tankPart = tankPart.ToString();
                this.isMap = tankPart == TankPart.NONE;
            }

        }

    }
} //END