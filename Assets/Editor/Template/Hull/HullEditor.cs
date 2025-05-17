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
        private GameObject _currentPreview;

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.HULL_VISUAL);

            _hullSize = serializedObject.FindProperty("hullSize");
            _showPreview = serializedObject.FindProperty("showPreview");

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
            } else {
                Debug.LogWarning($"Prefab not found at path: {assetPath}");
            }
        }

        private void DestroyExistingPreview() {
            var previews = FindObjectsOfType<GameObject>();
            Debug.Log(previews.Length);
            foreach (var go in previews) {
                if (go.name.StartsWith("Preview_") && go.hideFlags == HideFlags.HideAndDontSave) {
                    Debug.Log($"Destroyed {go.name}");
                    DestroyImmediate(go);
                }
            }

            _currentPreview = null;
        }

        private void ApplyHideFlagsRecursively(GameObject root) {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) {
                GameObject go = t.gameObject;
                go.hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable;
                SceneVisibilityManager.instance.DisablePicking(go, true);
            }
        }

    }
}
