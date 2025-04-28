using Editor.Const;
using Other.Template.WeaponryTemplate.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Weaponry {
    [CustomEditor(typeof(WeaponryData))]
    public class WeaponryEditor : DefaultEditor {

        //Child object (name: Graphic 2)
        protected GameObject barrel;

        private PartProperties _cannonPart;
        private PartProperties _barrelPart;


        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.WEAPONRY_VISUAL);
            barrel = GameObject.FindWithTag(Tags.BARREL_VISUAL);

            _cannonPart = new PartProperties(visual, serializedObject);
            _barrelPart = new PartProperties(barrel, serializedObject, "barrel");

            LoadAssetBundles();
        }


        public override void OnInspectorGUI() {
            serializedObject.Update();

            CreateInspector();
            AssetBundle(visual);

            serializedObject.ApplyModifiedProperties();
        }

        protected override void CreateInspector() {
            EditorGUILayout.LabelField("Cannon Settings", EditorStyles.boldLabel);
            CreateMass(_cannonPart.mass);
            Space();
            CreateMesh(_cannonPart.mesh);
            Space();
            CreateMaterial(_cannonPart.materials, _cannonPart.numOfMaterials);
            Space();
            CreateColliders(_cannonPart.colliders, _cannonPart.numOfColliders);
            DoubleSpace();

            // Barrel
            EditorGUILayout.LabelField("Barrel Settings", EditorStyles.boldLabel);
            CreateMass(_barrelPart.mass);
            Space();
            CreateMesh(_barrelPart.mesh);
            Space();
            CreateMaterial(_barrelPart.materials, _barrelPart.numOfMaterials);
            Space();
            CreateColliders(_barrelPart.colliders, _barrelPart.numOfColliders);
            DoubleSpace();

            if (GUI.changed) {
                Create(_cannonPart);
                Create(_barrelPart);
            }
        }

    }
} //END