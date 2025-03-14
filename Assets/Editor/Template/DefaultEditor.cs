using UnityEditor;
using UnityEngine;

namespace Editor.Template {
    public abstract class DefaultEditor : UnityEditor.Editor {

        //Child object (name: Graphic)
        protected GameObject visual;

        protected SerializedProperty massProp;
        protected SerializedProperty meshProp;
        protected SerializedProperty numOfMatProp;
        protected SerializedProperty materialsProp;
        protected SerializedProperty numOfColProp;
        protected SerializedProperty collidersProp;

        protected void FindDefaultProperties() {
            massProp = serializedObject.FindProperty("mass");
            meshProp = serializedObject.FindProperty("mesh");
            numOfMatProp = serializedObject.FindProperty("numOfMat");
            materialsProp = serializedObject.FindProperty("materials");
            numOfColProp = serializedObject.FindProperty("numOfCol");
            collidersProp = serializedObject.FindProperty("colliders");
        }
        
        protected virtual void CreateInspector() {
            //Basic settings
            EditorGUILayout.HelpBox("Basic Settings", MessageType.None, true);
            CreateMass();
            Space();
            CreateMesh();
            Space();
            CreateMaterial();
            DoubleSpace();

            //Collider settings
            EditorGUILayout.HelpBox("Collider Settings", MessageType.None, true);
            CreateColliders();
            DoubleSpace();

            if (GUI.changed) {
                Create();
            }
        }

        protected void CreateMass() {
            EditorGUILayout.IntSlider(massProp, 1, 100000, new GUIContent("Mass", "Mass of the part."));
        }

        protected void CreateMesh() {
            meshProp.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Mesh", "Mesh"),
                meshProp.objectReferenceValue,
                typeof(Mesh)) as Mesh;
        }

        protected void CreateMaterial() {
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 100;
            int minValue = 1;
            int maxValue = 100;
            EditorGUILayout.LabelField("Number of Materials");

            var value = EditorGUILayout.IntField(numOfMatProp.intValue);
            if (value <= maxValue) numOfMatProp.intValue = value;

            GUIStyle boldButtonStyle = new GUIStyle(GUI.skin.button);
            boldButtonStyle.fontStyle = FontStyle.Bold;
            if (GUILayout.Button("-", boldButtonStyle, GUILayout.MinWidth(50)) &&
                numOfMatProp.intValue > minValue) {
                numOfMatProp.intValue--;
            }

            if (GUILayout.Button("+", boldButtonStyle, GUILayout.MinWidth(50)) &&
                numOfMatProp.intValue < maxValue) {
                numOfMatProp.intValue++;
            }

            GUILayout.EndHorizontal();

            materialsProp.arraySize = numOfMatProp.intValue;

            EditorGUI.indentLevel++;
            for (int i = 0; i < numOfMatProp.intValue; i++) {
                EditorGUIUtility.labelWidth = 155;
                materialsProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField(
                    "Material " + i, materialsProp.GetArrayElementAtIndex(i).objectReferenceValue,
                    typeof(Material), false);
            }

            EditorGUI.indentLevel--;
        }

        protected void CreateColliders() {
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 100;
            int minValue = 1;
            int maxValue = 10;
            EditorGUILayout.LabelField("Number of Colliders");

            var value = EditorGUILayout.IntField(numOfColProp.intValue);
            if (value <= maxValue) numOfColProp.intValue = value;

            GUIStyle boldButtonStyle = new GUIStyle(GUI.skin.button);
            boldButtonStyle.fontStyle = FontStyle.Bold;
            if (GUILayout.Button("-", boldButtonStyle, GUILayout.MinWidth(50)) &&
                numOfColProp.intValue > minValue) {
                numOfColProp.intValue--;
            }

            if (GUILayout.Button("+", boldButtonStyle, GUILayout.MinWidth(50)) &&
                numOfColProp.intValue < maxValue) {
                numOfColProp.intValue++;
            }

            GUILayout.EndHorizontal();

            collidersProp.arraySize = numOfColProp.intValue;

            EditorGUI.indentLevel++;
            for (int i = 0; i < numOfColProp.intValue; i++) {
                EditorGUIUtility.labelWidth = 155;
                collidersProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField(
                    "MeshCollider " + i, collidersProp.GetArrayElementAtIndex(i).objectReferenceValue,
                    typeof(Mesh), false);
            }

            EditorGUI.indentLevel--;
        }

        protected void Create() {
            //Set mesh
            visual.GetComponent<MeshFilter>().mesh = meshProp.objectReferenceValue as Mesh;

            //Set material
            Material[] materials = new Material [numOfMatProp.intValue];
            for (int i = 0; i < materials.Length; i++) {
                materials[i] = materialsProp.GetArrayElementAtIndex(i).objectReferenceValue as Material;
            }

            visual.GetComponent<MeshRenderer>().materials = materials;

            //Set collider
            MeshCollider[] oldMeshColliders = visual.GetComponents<MeshCollider>();
            for (int i = 0; i < oldMeshColliders.Length; i++) {
                var oldCollider = oldMeshColliders[i];
                EditorApplication.delayCall += () => DestroyImmediate(oldCollider);
            }

            for (int i = 0; i < numOfColProp.intValue; i++) {
                MeshCollider meshCollider = visual.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = collidersProp.GetArrayElementAtIndex(i).objectReferenceValue as Mesh;
                meshCollider.convex = true;
            }
        }

        #region Style

        protected void Space() {
            EditorGUILayout.Space(10);
        }

        protected void DoubleSpace() {
            EditorGUILayout.Space(20);
        }

        #endregion

    }
} //END