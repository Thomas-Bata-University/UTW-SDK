using System.Linq;
using Editor.Helper;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Task {
    public static class PrefabCreator {

        public static void CreatePrefab(string prefabPath, string newName, string tag, string parentTag) {
            var parent = GameObject.FindGameObjectWithTag(parentTag);
            if (parent == null) {
                ColorLogger.LogWarning(
                    $"No parent with tag {parentTag} found. Plate will be placed at scene root.");
            }

            CreatePrefab(prefabPath, newName, tag, parent);
        }

        private static void CreatePrefab(string prefabPath, string newName, string tag, GameObject parent) {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) {
                ColorLogger.LogError($"Prefab not found at path: {prefabPath}");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null) {
                ColorLogger.LogError($"Failed to instantiate prefab!");
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