using Other.CoreScripts;
using Other.Template.Core;
using Other.Template.TurretTemplate.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace Other.Template.TurretTemplate.Scripts.Editor {
    [CustomEditor(typeof(TurretData))]
    public class TurretEditor : DefaultEditor {

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.TURRET_VISUAL);
            
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