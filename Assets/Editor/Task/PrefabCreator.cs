using System.Linq;
using Editor.Const;
using Editor.Helper;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Task {
    public static class PrefabCreator {

        public static void CreateMountPoint(string prefabPath, string newName, string tag) {
            var parent = GetFirstRootObject();
            CreatePrefab(prefabPath, newName, tag, parent, "mount point");
        }

        public static void CreateInternalModule(string prefabPath, string newName, string tag) {
            var parent = GetFirstRootObject();
            CreatePrefab(prefabPath, newName, tag, parent, "internal module");
        }

        public static void CreatePlate(string prefabPath, string newName, string tag) {
            var parent = GameObject.FindGameObjectWithTag(Tags.PLATE_PARENT);
            if (parent == null) {
                ColorLogger.LogWarning(
                    $"No parent with tag '{Tags.PLATE_PARENT}' found. Plate will be placed at scene root.");
            }

            CreatePrefab(prefabPath, newName, tag, parent, "plate");
        }

        private static void CreatePrefab(string prefabPath, string newName, string tag, GameObject parent,
            string objectType) {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) {
                ColorLogger.LogError($"Prefab not found at path: {prefabPath}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null) {
                ColorLogger.LogError($"Failed to instantiate {objectType} prefab!");
                return;
            }

            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            instance.name = newName;
            instance.tag = tag;
            instance.transform.position = Vector3.zero;

            if (parent != null) {
                instance.transform.SetParent(parent.transform, true);
                IconSetter.SetIcon(instance);
                ColorLogger.LogFormatted("Created {0} under parent {1}",
                        new[] { instance.name, parent.name },
                        bolds: new[] { true, true });
            }
            else {
                ColorLogger.LogFormatted("Created {0} at scene root.", instance.name, bold: true);
            }
        }

        private static GameObject GetFirstRootObject() {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            return rootObjects.FirstOrDefault();
        }

    }
}