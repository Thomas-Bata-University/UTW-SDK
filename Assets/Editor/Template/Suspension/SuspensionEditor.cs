using Editor.Const;
using Editor.Core;
using Other.Template.SuspensionTemplate.Data;
using Other.Template.WeaponryTemplate.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Suspension {
    [CustomEditor(typeof(SuspensionData))]
    public class SuspensionEditor : DefaultEditor {

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.SUSPENSION_VISUAL);

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