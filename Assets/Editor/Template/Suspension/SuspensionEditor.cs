using Editor.Const;
using Editor.Core;
using Other.Template;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Suspension {
    [CustomEditor(typeof(SuspensionData))]
    public class SuspensionEditor : DefaultEditor {

        protected GameObject sprocket;
        protected GameObject idler;

        private PartProperties _roadWheelPart;
        private PartProperties _sprocketPart;
        private PartProperties _idlerPart;

        private SerializedProperty _tankSize;
        private SerializedProperty _showPreview;
        private SerializedProperty _trackOffset;
        private SerializedProperty _sprocketTrackOffset;
        private SerializedProperty _idlerTrackOffset;
        private SerializedProperty _roadWheelScale;
        
        private GameObject _currentPreview;

        private const float TrackMaxOffset = 5f;

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.SUSPENSION_VISUAL);
            sprocket = GameObject.FindWithTag(Tags.SPROCKET_VISUAL);
            idler = GameObject.FindWithTag(Tags.IDLER_VISUAL);

            _roadWheelPart = new PartProperties(visual, serializedObject);
            _sprocketPart = new PartProperties(sprocket, serializedObject, "sprocket");
            _idlerPart = new PartProperties(idler, serializedObject, "idler");

            _tankSize = serializedObject.FindProperty("tankSize");
            _showPreview = serializedObject.FindProperty("showPreview");
            _trackOffset = serializedObject.FindProperty("trackOffset");
            _sprocketTrackOffset = serializedObject.FindProperty("sprocketTrackOffset");
            _idlerTrackOffset = serializedObject.FindProperty("idlerTrackOffset");
            _roadWheelScale = serializedObject.FindProperty("wheelScale");

            if (_showPreview.boolValue) {
                CreatePreview((TankSize)_tankSize.enumValueIndex);
            }
            
            LoadAssetBundles();
            
            var count = serializedObject.FindProperty("wheelCount").intValue;
            var spacing = serializedObject.FindProperty("wheelSpacing").floatValue;
            RecreateRoadWheels(count, spacing);
            
            UpdateTrackOffset(sprocket, _sprocketTrackOffset.floatValue);
            UpdateTrackOffset(idler, _idlerTrackOffset.floatValue);
        }

        public override void OnInspectorGUI() {
            if (target == null || serializedObject.targetObject == null) return;

            serializedObject.Update();

            CreateInspector();
            AssetBundle(visual);

            serializedObject.ApplyModifiedProperties();
        }

        protected override void CreateInspector() {
            // Road wheel
            EditorGUILayout.LabelField("Road wheel Settings", EditorStyles.boldLabel);
            CreateMass(_roadWheelPart.mass);
            Space();
            CreateMesh(_roadWheelPart.mesh, visual);
            Space();
            CreateMaterial(_roadWheelPart.materials, _roadWheelPart.numOfMaterials, visual);
            Space();
            CreateWheels();
            DoubleSpace();

            // Sprocket
            EditorGUILayout.LabelField("Sprocket Settings", EditorStyles.boldLabel);
            CreateMass(_sprocketPart.mass);
            Space();
            CreateMesh(_sprocketPart.mesh, sprocket);
            Space();
            CreateMaterial(_sprocketPart.materials, _sprocketPart.numOfMaterials, sprocket);
            Space();
            CreateTrackOffset(sprocket, _sprocketTrackOffset);
            DoubleSpace();

            // Idler
            EditorGUILayout.LabelField("Idler Settings", EditorStyles.boldLabel);
            CreateMass(_idlerPart.mass);
            Space();
            CreateMesh(_idlerPart.mesh, idler);
            Space();
            CreateMaterial(_idlerPart.materials, _idlerPart.numOfMaterials, idler);
            Space();
            CreateTrackOffset(idler, _idlerTrackOffset);
            DoubleSpace();

            HullPreview();
        }

        private void CreateWheels() {
            SerializedProperty roadWheelCount = serializedObject.FindProperty("wheelCount");
            SerializedProperty wheelSpacing = serializedObject.FindProperty("wheelSpacing");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.IntSlider(roadWheelCount, 0, 12, new GUIContent("Count"));
            EditorGUILayout.Slider(wheelSpacing, 0.2f, 3f, new GUIContent("Spacing"));

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                RecreateRoadWheels(roadWheelCount.intValue, wheelSpacing.floatValue);
            }
            
            CreateTrackOffset(visual, _trackOffset);
        }

        private void RecreateRoadWheels(int count, float spacing) {
            if (visual == null) return;

            for (int i = visual.transform.childCount - 1; i >= 0; i--) {
                Transform child = visual.transform.GetChild(i);
                if (child.name.StartsWith("RoadWheel_")) {
                    DestroyImmediate(child.gameObject);
                }
            }

            string assetPath = $"{AssetPaths.TEMPLATE}/SuspensionTemplate/Prefabs/RoadWheel.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) {
                Debug.LogWarning("RoadWheel prefab not found.");
                return;
            }

            float offset = -(count - 1) * spacing / 2f;
            float trackOffset = _trackOffset.floatValue;

            for (int i = 0; i < count; i++) {
                float zPos = offset + i * spacing;

                // Left
                GameObject leftWheel = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                PrefabUtility.UnpackPrefabInstance(leftWheel, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                leftWheel.name = $"RoadWheel_L_{i + 1}";
                leftWheel.transform.SetParent(visual.transform);
                leftWheel.transform.localPosition = new Vector3(-trackOffset, 0, zPos);

                // Right
                GameObject rightWheel = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                PrefabUtility.UnpackPrefabInstance(rightWheel, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                rightWheel.name = $"RoadWheel_R_{i + 1}";
                rightWheel.transform.SetParent(visual.transform);
                rightWheel.transform.localPosition = new Vector3(trackOffset, 0, zPos);
            }
            
            ApplyMeshToAllRoadWheels(_roadWheelPart.mesh.objectReferenceValue as Mesh, visual);

            Material[] mats = new Material[_roadWheelPart.numOfMaterials.intValue];
            for (int i = 0; i < mats.Length; i++) {
                mats[i] = _roadWheelPart.materials.GetArrayElementAtIndex(i).objectReferenceValue as Material;
            }
            ApplyMaterialsToAllRoadWheels(mats, visual);

            ApplyCollidersToAllRoadWheels(_roadWheelPart.colliders, _roadWheelPart.numOfColliders, visual);

            SceneView.RepaintAll();
        }

        private void CreateTrackOffset(GameObject wheelPart, SerializedProperty offset) {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Slider(offset, 0, TrackMaxOffset, new GUIContent("Track Offset"));

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                UpdateTrackOffset(wheelPart, offset.floatValue);

                if (offset.name == "_trackOffset" || offset.name == "trackOffset") {
                    var count = serializedObject.FindProperty("wheelCount").intValue;
                    var spacing = serializedObject.FindProperty("wheelSpacing").floatValue;
                    RecreateRoadWheels(count, spacing);
                }
            }
        }


        private void HullPreview() {
            EditorGUILayout.LabelField("Hull preview", EditorStyles.boldLabel);

            // Dropdown
            EditorGUI.BeginChangeCheck();
            int selectedIndex = _tankSize.enumValueIndex;
            int newIndex = EditorGUILayout.Popup("Tank Size", selectedIndex, TankSizeInfo.GetDisplayNames());

            if (EditorGUI.EndChangeCheck() && newIndex != selectedIndex) {
                _tankSize.enumValueIndex = newIndex;
                serializedObject.ApplyModifiedProperties();

                if (_showPreview.boolValue) {
                    CreatePreview((TankSize)newIndex);
                }
            }

            // Toggle
            EditorGUI.BeginChangeCheck();
            bool newShow = EditorGUILayout.Toggle("Show", _showPreview.boolValue);
            if (EditorGUI.EndChangeCheck()) {
                _showPreview.boolValue = newShow;
                serializedObject.ApplyModifiedProperties();

                if (newShow) {
                    if (_currentPreview == null) {
                        CreatePreview((TankSize)_tankSize.enumValueIndex);
                    }
                    else {
                        _currentPreview.SetActive(true);
                    }
                }
                else {
                    if (_currentPreview != null) {
                        _currentPreview.SetActive(false);
                    }
                }
            }
        }

        private void CreatePreview(TankSize size) {
            if (visual is null) return;

            OpenProjectController.DestroyPreviewObjects();
            _currentPreview = null;

            string prefabName = TankSizeInfo.Get(size).PrefabName;
            string assetPath = $"Assets/Other/Template/OtherTemplate/HullPreview/{prefabName}.prefab";

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null) {
                _currentPreview = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                _currentPreview.name = $"Preview_{size}";

                ApplyHideFlagsRecursively(_currentPreview);
            }
            else {
                Debug.LogWarning($"Prefab not found at path: {assetPath}");
            }
        }

        private void ApplyHideFlagsRecursively(GameObject root) {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) {
                GameObject go = t.gameObject;
                go.hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable;
                SceneVisibilityManager.instance.DisablePicking(go, true);
            }
        }

        private void UpdateTrackOffset(GameObject wheelPart, float offset) {
            if (wheelPart == null || _trackOffset == null) return;

            Transform left = wheelPart.transform.Find("Left");
            Transform right = wheelPart.transform.Find("Right");

            if (left != null) {
                left.localPosition = new Vector3(-offset, left.localPosition.y, left.localPosition.z);
            }

            if (right != null) {
                right.localPosition = new Vector3(offset, right.localPosition.y, right.localPosition.z);
            }

            SceneView.RepaintAll();
        }

        #region Override
        
        private void CreateMesh(SerializedProperty prop, GameObject parent, string text = "Mesh", string tooltip = "Mesh") {
            EditorGUIUtility.labelWidth = 155;

            EditorGUI.BeginChangeCheck();
            var newMesh = EditorGUILayout.ObjectField(new GUIContent(text, tooltip), prop.objectReferenceValue, typeof(Mesh), false) as Mesh;
            if (EditorGUI.EndChangeCheck()) {
                prop.objectReferenceValue = newMesh;
                serializedObject.ApplyModifiedProperties();

                ApplyMeshToAllRoadWheels(newMesh, parent);
            }
        }
        
        protected void CreateMaterial(SerializedProperty prop, SerializedProperty numProp,
            GameObject parent, string text = "Number of Materials") {

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

            if (GUI.changed) {
                Material[] newMaterials = new Material[numProp.intValue];
                for (int i = 0; i < numProp.intValue; i++) {
                    newMaterials[i] = prop.GetArrayElementAtIndex(i).objectReferenceValue as Material;
                }
                ApplyMaterialsToAllRoadWheels(newMaterials, parent);
            }
        }
        
        private void CreateColliders(SerializedProperty prop, SerializedProperty numProp,
            GameObject parent, string text = "Number of Colliders") {
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

            if (GUI.changed) {
                ApplyCollidersToAllRoadWheels(prop, numProp, parent);
            }
        }

        private void ApplyMeshToAllRoadWheels(Mesh mesh, GameObject parent) {
            if (parent == null || mesh == null) return;

            foreach (Transform child in parent.transform) {
                    MeshFilter mf = child.GetComponent<MeshFilter>();
                    if (mf != null) mf.sharedMesh = mesh;
            }
        }
        
        private void ApplyMaterialsToAllRoadWheels(Material[] materials, GameObject parent) {
            if (parent == null || materials == null || materials.Length == 0) return;

            foreach (Transform child in parent.transform) {
                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    if (mr != null) mr.sharedMaterials = materials;
            }
        }
        
        private void ApplyCollidersToAllRoadWheels(SerializedProperty colliders, SerializedProperty countProp, GameObject parent) {
            if (parent == null || colliders == null || countProp == null) return;

            foreach (Transform child in parent.transform) {
                if (!child.name.StartsWith("RoadWheel_")) continue;

                // Remove old
                foreach (var old in child.GetComponents<MeshCollider>()) {
                    DestroyImmediate(old);
                }

                // Add new
                for (int j = 0; j < countProp.intValue; j++) {
                    Mesh mesh = colliders.GetArrayElementAtIndex(j).objectReferenceValue as Mesh;
                    if (mesh == null) continue;

                    MeshCollider mc = child.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mesh;
                    mc.convex = true;
                }
            }
        }
        
        #endregion

    }
} //END