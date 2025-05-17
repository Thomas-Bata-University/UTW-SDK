using Editor.Const;
using Editor.Core;
using Other.Template.HullTemplate.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Hull {
    [CustomEditor(typeof(HullData))]
    public class HullEditor : DefaultEditor {

        private SerializedProperty _hullSize;
        private SerializedProperty _showPreview;
        private SerializedProperty _trackOffset;

        private GameObject _currentPreview;

        private const float TrackMaxOffset = 5f;

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.HULL_VISUAL);

            _hullSize = serializedObject.FindProperty("hullSize");
            _showPreview = serializedObject.FindProperty("showPreview");
            _trackOffset = serializedObject.FindProperty("trackOffset");

            RegisterPart(visual);

            if (_showPreview.boolValue) {
                CreatePreview((HullSize)_hullSize.enumValueIndex);
            }

            LoadAssetBundles();
        }

        public override void OnInspectorGUI() {
            if (target == null || serializedObject.targetObject == null) return;

            serializedObject.Update();

            CreateInspector();
            AssetBundle(visual);

            serializedObject.ApplyModifiedProperties();
        }

        protected override void CreateInspector() {
            base.CreateInspector();
            SuspensionPreview();
            Space();
        }

        private void SuspensionPreview() {
            EditorGUILayout.LabelField("Suspension preview", EditorStyles.boldLabel);

            // Dropdown
            EditorGUI.BeginChangeCheck();
            int selectedIndex = _hullSize.enumValueIndex;
            int newIndex = EditorGUILayout.Popup("Hull Size", selectedIndex, HullSizeInfo.GetDisplayNames());

            if (EditorGUI.EndChangeCheck() && newIndex != selectedIndex) {
                _hullSize.enumValueIndex = newIndex;
                serializedObject.ApplyModifiedProperties();

                if (_showPreview.boolValue) {
                    CreatePreview((HullSize)newIndex);
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
                        CreatePreview((HullSize)_hullSize.enumValueIndex);
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

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Slider(_trackOffset, 0, TrackMaxOffset, new GUIContent("Track Offset"));
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                UpdateTrackOffset();
            }

            float actualDistance = _trackOffset != null ? _trackOffset.floatValue * 2f : 0f;
            EditorGUILayout.LabelField("Distance between tracks", $"{actualDistance:F2} meters");
        }

        private void CreatePreview(HullSize size) {
            OpenProjectController.DestroyPreviewObjects();
            _currentPreview = null;

            string prefabName = HullSizeInfo.Get(size).PrefabName;
            string assetPath = $"Assets/Other/Template/OtherTemplate/{prefabName}.prefab";

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null) {
                _currentPreview = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                _currentPreview.name = $"Preview_{size}";

                ApplyHideFlagsRecursively(_currentPreview);

                UpdateTrackOffset();
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

        private void UpdateTrackOffset() {
            if (_currentPreview == null || _trackOffset == null) return;

            float offset = _trackOffset.floatValue;

            Transform left = _currentPreview.transform.Find("Left");
            Transform right = _currentPreview.transform.Find("Right");

            if (left != null) {
                left.localPosition = new Vector3(-offset, left.localPosition.y, left.localPosition.z);
            }

            if (right != null) {
                right.localPosition = new Vector3(offset, right.localPosition.y, right.localPosition.z);
            }

            SceneView.RepaintAll();
        }

    }
}