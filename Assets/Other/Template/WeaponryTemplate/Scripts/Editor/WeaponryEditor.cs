using Other.CoreScripts;
using Other.Template.Core;
using Other.Template.WeaponryTemplate.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace Other.Template.WeaponryTemplate.Scripts.Editor {
    [CustomEditor(typeof(WeaponryData))]
    public class WeaponryEditor : DefaultEditor {

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.WEAPONRY_VISUAL);
            
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