using Editor.Core;
using Other.Template.WeaponryTemplate.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Suspension {
    [CustomEditor(typeof(WeaponryData))]
    public class SuspensionEditor : DefaultEditor {

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.SUSPENSION_VISUAL);

            FindDefaultProperties();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            CreateInspector();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void CreateInspector() {
            base.CreateInspector();
            //TODO Add more settings
        }

    }
} //END