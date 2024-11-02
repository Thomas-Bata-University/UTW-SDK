using Other.CoreScripts;
using Other.Template.TurretTemplate.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace Other.Template.TurretTemplate.Scripts.Editor {
    [CustomEditor(typeof(TurretData))]
    public class TurretEditor : UnityEditor.Editor {

        //Child object
        public GameObject turretVisual;

        private SerializedProperty _massProp;
        private SerializedProperty _meshProp;
        private SerializedProperty _numOfMatProp;
        private SerializedProperty _materialsProp;
        private SerializedProperty _numOfColProp;
        private SerializedProperty _collidersProp;

        private void OnEnable() {
            turretVisual = GameObject.FindWithTag(Tags.TURRET_VISUAL);

            _massProp = serializedObject.FindProperty("mass");
            _meshProp = serializedObject.FindProperty("mesh");
            _numOfMatProp = serializedObject.FindProperty("numOfMat");
            _materialsProp = serializedObject.FindProperty("materials");
            _numOfColProp = serializedObject.FindProperty("numOfCol");
            _collidersProp = serializedObject.FindProperty("colliders");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            CreateInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateInspector() {
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

        private void CreateMass() {
            EditorGUILayout.IntSlider(_massProp, 1, 100000, new GUIContent("Mass", "Mass of the turret."));
        }

        private void CreateMesh() {
            _meshProp.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Mesh", "Mesh"),
                _meshProp.objectReferenceValue,
                typeof(Mesh)) as Mesh;
        }

        private void CreateMaterial() {
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 100;
            int minValue = 1;
            int maxValue = 100;
            EditorGUILayout.LabelField("Number of Materials");

            var value = EditorGUILayout.IntField(_numOfMatProp.intValue);
            if (value <= maxValue) _numOfMatProp.intValue = value;

            GUIStyle boldButtonStyle = new GUIStyle(GUI.skin.button);
            boldButtonStyle.fontStyle = FontStyle.Bold;
            if (GUILayout.Button("-", boldButtonStyle, GUILayout.MinWidth(50)) &&
                _numOfMatProp.intValue > minValue) {
                _numOfMatProp.intValue--;
            }

            if (GUILayout.Button("+", boldButtonStyle, GUILayout.MinWidth(50)) &&
                _numOfMatProp.intValue < maxValue) {
                _numOfMatProp.intValue++;
            }

            GUILayout.EndHorizontal();

            _materialsProp.arraySize = _numOfMatProp.intValue;

            EditorGUI.indentLevel++;
            for (int i = 0; i < _numOfMatProp.intValue; i++) {
                EditorGUIUtility.labelWidth = 155;
                _materialsProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField(
                    "Material " + i, _materialsProp.GetArrayElementAtIndex(i).objectReferenceValue,
                    typeof(Material), false);
            }

            EditorGUI.indentLevel--;
        }

        private void CreateColliders() {
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 100;
            int minValue = 1;
            int maxValue = 10;
            EditorGUILayout.LabelField("Number of Colliders");

            var value = EditorGUILayout.IntField(_numOfColProp.intValue);
            if (value <= maxValue) _numOfColProp.intValue = value;

            GUIStyle boldButtonStyle = new GUIStyle(GUI.skin.button);
            boldButtonStyle.fontStyle = FontStyle.Bold;
            if (GUILayout.Button("-", boldButtonStyle, GUILayout.MinWidth(50)) &&
                _numOfColProp.intValue > minValue) {
                _numOfColProp.intValue--;
            }

            if (GUILayout.Button("+", boldButtonStyle, GUILayout.MinWidth(50)) &&
                _numOfColProp.intValue < maxValue) {
                _numOfColProp.intValue++;
            }

            GUILayout.EndHorizontal();

            _collidersProp.arraySize = _numOfColProp.intValue;

            EditorGUI.indentLevel++;
            for (int i = 0; i < _numOfColProp.intValue; i++) {
                EditorGUIUtility.labelWidth = 155;
                _collidersProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField(
                    "MeshCollider " + i, _collidersProp.GetArrayElementAtIndex(i).objectReferenceValue,
                    typeof(Mesh), false);
            }

            EditorGUI.indentLevel--;
        }

        private void Create() {
            //Set mesh
            turretVisual.GetComponent<MeshFilter>().mesh = _meshProp.objectReferenceValue as Mesh;

            //Set material
            Material[] materials = new Material [_numOfMatProp.intValue];
            for (int i = 0; i < materials.Length; i++) {
                materials[i] = _materialsProp.GetArrayElementAtIndex(i).objectReferenceValue as Material;
            }

            turretVisual.GetComponent<MeshRenderer>().materials = materials;

            //Set collider
            MeshCollider[] oldMeshColliders = turretVisual.GetComponents<MeshCollider>();
            for (int i = 0; i < oldMeshColliders.Length; i++) {
                var oldCollider = oldMeshColliders[i];
                EditorApplication.delayCall += () => DestroyImmediate(oldCollider);
            }

            for (int i = 0; i < _numOfColProp.intValue; i++) {
                MeshCollider meshCollider = turretVisual.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = _collidersProp.GetArrayElementAtIndex(i).objectReferenceValue as Mesh;
                meshCollider.convex = true;
            }
        }

        private void Space() {
            EditorGUILayout.Space(10);
        }

        private void DoubleSpace() {
            EditorGUILayout.Space(20);
        }

    }
} //END