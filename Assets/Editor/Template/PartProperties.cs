using Editor.Helper;
using UnityEditor;
using UnityEngine;

namespace Editor.Template {
    public class PartProperties {

        public SerializedProperty mass;
        public SerializedProperty mesh;
        public SerializedProperty numOfMaterials;
        public SerializedProperty materials;
        public SerializedProperty numOfColliders;
        public SerializedProperty colliders;

        public GameObject visualObject;

        public PartProperties(GameObject visual, SerializedObject serialized, string prefix = "") {
            visualObject = visual;

            mass = FindPropertySafe(serialized, prefix + "Mass");
            mesh = FindPropertySafe(serialized, prefix + "Mesh");
            numOfMaterials = FindPropertySafe(serialized, prefix + "NumOfMat");
            materials = FindPropertySafe(serialized, prefix + "Materials");
            numOfColliders = FindPropertySafe(serialized, prefix + "NumOfCol");
            colliders = FindPropertySafe(serialized, prefix + "Colliders");
        }

        private SerializedProperty FindPropertySafe(SerializedObject serialized, string name) {
            var prop = serialized.FindProperty(name);
            if (prop == null) {
                ColorLogger.LogFormatted(
                    "Property '{0}' not found on {1}!",
                    new[] { name, serialized.targetObject.GetType().Name },
                    new[] { "red", null },
                    new[] { true, false },
                    ColorLogger.LogLevel.Error
                );
            }

            return prop;
        }

    }
}