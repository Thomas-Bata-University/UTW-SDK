using System;
using System.Collections.Generic;
using Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Editor.Template {
    public abstract class DefaultEditor : UnityEditor.Editor {

        //Child object (name: Graphic)
        protected GameObject visual;

        protected List<PartProperties> parts = new();

        private string[] _assetBundleOptions;
        private int _selectedBundleIndex = 0;

        protected void RegisterPart(GameObject visualObject, string prefix = "") {
            var part = new PartProperties(visualObject, serializedObject, prefix);
            parts.Add(part);
        }

        protected virtual void CreateInspector() {
            foreach (var part in parts) {
                if (part.visualObject == null) continue;

                string partName = part.visualObject.transform.parent != null
                    ? part.visualObject.transform.parent.name
                    : part.visualObject.name;

                EditorGUILayout.LabelField($"{partName} Settings", EditorStyles.boldLabel);

                CreateMass(part.mass);
                Space();
                CreateMesh(part.mesh);
                Space();
                CreateMaterial(part.materials, part.numOfMaterials);
                Space();
                CreateColliders(part.colliders, part.numOfColliders);
                DoubleSpace();
            }

            if (GUI.changed) {
                foreach (var part in parts) {
                    Create(part);
                }
            }
        }

        protected void CreateMass(SerializedProperty prop, string text = "Mass", string tooltip = "Mass of the part.") {
            EditorGUIUtility.labelWidth = 155;
            EditorGUILayout.IntSlider(prop, 1, 100000, new GUIContent(text, tooltip));
        }
        
        protected void CreateMesh(SerializedProperty prop, string text = "Mesh", string tooltip = "Mesh") {
            EditorGUIUtility.labelWidth = 155;
            prop.objectReferenceValue = EditorGUILayout.ObjectField(
                new GUIContent(text, tooltip),
                prop.objectReferenceValue,
                typeof(Mesh),
                false
            ) as Mesh;
        }

        protected void CreateMaterial(SerializedProperty prop, SerializedProperty numProp,
            string text = "Number of Materials") {
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 100;
            int minValue = 1;
            int maxValue = 100;
            EditorGUILayout.LabelField(text);

            var value = EditorGUILayout.IntField(numProp.intValue);
            if (value <= maxValue) numProp.intValue = value;

            GUIStyle boldButtonStyle = new GUIStyle(GUI.skin.button);
            boldButtonStyle.fontStyle = FontStyle.Bold;
            if (GUILayout.Button("-", boldButtonStyle, GUILayout.MinWidth(50)) &&
                numProp.intValue > minValue) {
                numProp.intValue--;
            }

            if (GUILayout.Button("+", boldButtonStyle, GUILayout.MinWidth(50)) &&
                numProp.intValue < maxValue) {
                numProp.intValue++;
            }

            GUILayout.EndHorizontal();

            prop.arraySize = numProp.intValue;

            EditorGUI.indentLevel++;
            for (int i = 0; i < numProp.intValue; i++) {
                EditorGUIUtility.labelWidth = 155;
                prop.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField(
                    "Material " + i, prop.GetArrayElementAtIndex(i).objectReferenceValue,
                    typeof(Material), false);
            }

            EditorGUI.indentLevel--;
        }

        protected void CreateColliders(SerializedProperty prop, SerializedProperty numProp,
            string text = "Number of Colliders") {
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 100;
            int minValue = 1;
            int maxValue = 10;
            EditorGUILayout.LabelField(text);

            var value = EditorGUILayout.IntField(numProp.intValue);
            if (value <= maxValue) numProp.intValue = value;

            GUIStyle boldButtonStyle = new GUIStyle(GUI.skin.button);
            boldButtonStyle.fontStyle = FontStyle.Bold;
            if (GUILayout.Button("-", boldButtonStyle, GUILayout.MinWidth(50)) &&
                numProp.intValue > minValue) {
                numProp.intValue--;
            }

            if (GUILayout.Button("+", boldButtonStyle, GUILayout.MinWidth(50)) &&
                numProp.intValue < maxValue) {
                numProp.intValue++;
            }

            GUILayout.EndHorizontal();

            prop.arraySize = numProp.intValue;

            EditorGUI.indentLevel++;
            for (int i = 0; i < numProp.intValue; i++) {
                EditorGUIUtility.labelWidth = 155;
                prop.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField(
                    "MeshCollider " + i, prop.GetArrayElementAtIndex(i).objectReferenceValue,
                    typeof(Mesh), false);
            }

            EditorGUI.indentLevel--;
        }

        protected void Create(PartProperties part) {
            // Set mesh
            part.visualObject.GetComponent<MeshFilter>().mesh = part.mesh.objectReferenceValue as Mesh;

            // Set materials
            Material[] materials = new Material[part.numOfMaterials.intValue];
            for (int i = 0; i < materials.Length; i++) {
                materials[i] = part.materials.GetArrayElementAtIndex(i).objectReferenceValue as Material;
            }

            part.visualObject.GetComponent<MeshRenderer>().materials = materials;

            // Remove old colliders
            MeshCollider[] oldMeshColliders = part.visualObject.GetComponents<MeshCollider>();
            for (int i = 0; i < oldMeshColliders.Length; i++) {
                var oldCollider = oldMeshColliders[i];
                EditorApplication.delayCall += () => DestroyImmediate(oldCollider);
            }

            // Add new colliders
            for (int i = 0; i < part.numOfColliders.intValue; i++) {
                MeshCollider meshCollider = part.visualObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = part.colliders.GetArrayElementAtIndex(i).objectReferenceValue as Mesh;
                meshCollider.convex = true;
            }
        }


        protected void AssetBundle(GameObject go) {
            if (go?.transform.parent is null) return;

            GameObject gameObject = go.transform.parent.gameObject;

            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);

            if (!string.IsNullOrEmpty(assetPath)) {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null) {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("AssetBundle Settings", EditorStyles.boldLabel);

                    _selectedBundleIndex = Array.IndexOf(_assetBundleOptions, importer.assetBundleName);
                    if (_selectedBundleIndex < 0) _selectedBundleIndex = 0;

                    int newIndex = EditorGUILayout.Popup("AssetBundle", _selectedBundleIndex, _assetBundleOptions);

                    if (newIndex != _selectedBundleIndex) {
                        importer.assetBundleName = newIndex == 0 ? "" : _assetBundleOptions[newIndex];
                        importer.SaveAndReimport();
                        OpenProjectController.MetaData.assetBundle = importer.assetBundleName;
                    }

                    if (GUILayout.Button("Remove AssetBundle")) {
                        importer.assetBundleName = "";
                        importer.SaveAndReimport();
                    }
                }
            }
            else {
                EditorGUILayout.HelpBox("This object is not Prefab.", MessageType.Warning);
            }
        }

        protected void LoadAssetBundles() {
            HashSet<string> bundleNames = new HashSet<string> { "None" };
            foreach (string assetPath in AssetDatabase.GetAllAssetPaths()) {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName)) {
                    bundleNames.Add(importer.assetBundleName);
                }
            }

            _assetBundleOptions = new List<string>(bundleNames).ToArray();
        }

        protected GameObject FindInactiveWithTag(string tag) {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in allGameObjects) {
                if (!go.CompareTag(tag)) continue;

                if (go.hideFlags == HideFlags.None && go.scene.IsValid())
                    return go;
            }

            return null;
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