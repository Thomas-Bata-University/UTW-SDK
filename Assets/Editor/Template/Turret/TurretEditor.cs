using Editor.Const;
using Editor.Core;
using Other.Template.TurretTemplate.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Turret {
    [CustomEditor(typeof(TurretData))]
    public class TurretEditor : DefaultEditor {

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.TURRET_VISUAL);

            RegisterPart(visual);
            
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
            //TODO Add more settings
        }

    }
} //END