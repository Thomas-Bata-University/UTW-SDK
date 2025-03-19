using System;
using System.Collections.Generic;
using Editor.Core;
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
        
        private string[] assetBundleOptions;
        private int selectedBundleIndex = 0;

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

        protected void AssetBundle() {
            if (visual?.transform.parent is null) return;

            GameObject gameObject = visual.transform.parent.gameObject;
            
            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);

            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("AssetBundle Settings", EditorStyles.boldLabel);

                    selectedBundleIndex = Array.IndexOf(assetBundleOptions, importer.assetBundleName);
                    if (selectedBundleIndex < 0) selectedBundleIndex = 0;

                    int newIndex = EditorGUILayout.Popup("AssetBundle", selectedBundleIndex, assetBundleOptions);
                
                    if (newIndex != selectedBundleIndex)
                    {
                        importer.assetBundleName = newIndex == 0 ? "" : assetBundleOptions[newIndex];
                        importer.SaveAndReimport();
                        OpenProjectController.MetaData.assetBundle = importer.assetBundleName;
                        Debug.Log($"AssetBundle for {gameObject.name} changed to: {importer.assetBundleName}");
                    }

                    if (GUILayout.Button("Remove AssetBundle"))
                    {
                        importer.assetBundleName = "";
                        importer.SaveAndReimport();
                        Debug.Log($"AssetBundle removed from '{gameObject.name}'");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("This object is not Prefab.", MessageType.Warning);
            }
        }
        
        protected void LoadAssetBundles()
        {
            // Načte všechny unikátní AssetBundle názvy v projektu
            HashSet<string> bundleNames = new HashSet<string> { "None" }; // "None" jako výchozí hodnota
            foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
            {
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                {
                    bundleNames.Add(importer.assetBundleName);
                }
            }

            assetBundleOptions = new List<string>(bundleNames).ToArray();
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