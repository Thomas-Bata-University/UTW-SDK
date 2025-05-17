using Editor.Const;
using Other.Template.WeaponryTemplate.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Weaponry {
    [CustomEditor(typeof(WeaponryData))]
    public class WeaponryEditor : DefaultEditor {

        //Child objects
        protected GameObject barrel;
        protected GameObject mantlet;

        private PartProperties _cannonPart;
        private PartProperties _barrelPart;
        private PartProperties _mantletPart;

        private SerializedProperty useBarrel;
        private SerializedProperty useMantlet;

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.WEAPONRY_VISUAL);
            barrel = GameObject.FindWithTag(Tags.BARREL_VISUAL);
            mantlet = GameObject.FindWithTag(Tags.MANTLET_VISUAL);

            _cannonPart = new PartProperties(visual, serializedObject);
            _mantletPart = new PartProperties(mantlet, serializedObject, "mantlet");
            _barrelPart = new PartProperties(barrel, serializedObject, "barrel");

            useBarrel = serializedObject.FindProperty("useBarrel");
            useMantlet = serializedObject.FindProperty("useMantlet");

            LoadAssetBundles();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            CreateInspector();
            AssetBundle(visual);

            serializedObject.ApplyModifiedProperties();
        }

        protected override void CreateInspector() {
            // Cannon
            EditorGUILayout.LabelField("Cannon Settings", EditorStyles.boldLabel);
            CreateMass(_cannonPart.mass);
            Space();
            CreateMesh(_cannonPart.mesh);
            Space();
            CreateMaterial(_cannonPart.materials, _cannonPart.numOfMaterials);
            Space();
            CreateColliders(_cannonPart.colliders, _cannonPart.numOfColliders);
            DoubleSpace();

            // Toggle for Barrel
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Barrel Settings", EditorStyles.boldLabel);
            bool prevBarrel = useBarrel.boolValue;
            useBarrel.boolValue = EditorGUILayout.Toggle(useBarrel.boolValue, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            if (barrel is null) {
                barrel = FindInactiveWithTag(Tags.BARREL_VISUAL);
                _barrelPart = new PartProperties(barrel, serializedObject, "barrel");
            }

            if (prevBarrel != useBarrel.boolValue)
                barrel.SetActive(useBarrel.boolValue);

            if (useBarrel.boolValue) {
                CreateMass(_barrelPart.mass);
                Space();
                CreateMesh(_barrelPart.mesh);
                Space();
                CreateMaterial(_barrelPart.materials, _barrelPart.numOfMaterials);
                Space();
                CreateColliders(_barrelPart.colliders, _barrelPart.numOfColliders);
                DoubleSpace();
            }

            // Toggle for Mantlet
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mantlet Settings", EditorStyles.boldLabel);
            bool prevMantlet = useMantlet.boolValue;
            useMantlet.boolValue = EditorGUILayout.Toggle(useMantlet.boolValue, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            if (mantlet is null) {
                mantlet = FindInactiveWithTag(Tags.MANTLET_VISUAL);
                _mantletPart = new PartProperties(mantlet, serializedObject, "mantlet");
            }

            if (prevMantlet != useMantlet.boolValue)
                mantlet.SetActive(useMantlet.boolValue);

            if (useMantlet.boolValue) {
                CreateMass(_mantletPart.mass);
                Space();
                CreateMesh(_mantletPart.mesh);
                Space();
                CreateMaterial(_mantletPart.materials, _mantletPart.numOfMaterials);
                Space();
                CreateColliders(_mantletPart.colliders, _mantletPart.numOfColliders);
                DoubleSpace();
            }

            if (GUI.changed) {
                Create(_cannonPart);
                Create(_barrelPart);
                Create(_mantletPart);
            }
        }

    }
} //END