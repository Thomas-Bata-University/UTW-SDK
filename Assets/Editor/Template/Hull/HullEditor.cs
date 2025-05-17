using Editor.Const;
using Other.Template.HullTemplate.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Hull {
    [CustomEditor(typeof(HullData))]
    public class HullEditor : DefaultEditor {

        private SerializedProperty _hullSize;
        private GameObject _currentPreview;

        private SerializedProperty _showPreview;

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.HULL_VISUAL);
            
            _hullSize = serializedObject.FindProperty("hullSize");
            _showPreview = serializedObject.FindProperty("showPreview");
            
            RegisterPart(visual);

            CreatePreview((HullSize)_hullSize.enumValueIndex);

            LoadAssetBundles();
        }

        public override void OnInspectorGUI() {
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

            EditorGUI.BeginChangeCheck();

            string[] displayNames = HullSizeInfo.GetDisplayNames();

            int selectedIndex = _hullSize.enumValueIndex;
            int newIndex = EditorGUILayout.Popup("Hull Size", selectedIndex, displayNames);

            if (EditorGUI.EndChangeCheck() && newIndex != selectedIndex) {
                _hullSize.enumValueIndex = newIndex;
                serializedObject.ApplyModifiedProperties();
                CreatePreview((HullSize)newIndex);
            }
            
            _showPreview.boolValue = EditorGUILayout.Toggle("Show", _showPreview.boolValue);
            if (_currentPreview != null) {
                _currentPreview.SetActive(_showPreview.boolValue);
            }
        }

        private void CreatePreview(HullSize size) {
            if (_currentPreview != null) {
                DestroyImmediate(_currentPreview);
                _currentPreview = null;
            }

            string prefabName = HullSizeInfo.Get(size).PrefabName;
            string assetPath = $"Assets/Other/Template/OtherTemplate/{prefabName}.prefab";

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null) {
                _currentPreview = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                _currentPreview.hideFlags = HideFlags.HideAndDontSave;
                _currentPreview.name = $"Preview_{size}";
            }
            else {
                Debug.LogWarning($"Prefab not found at path: {assetPath}");
            }
        }

    }
} //END