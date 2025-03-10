using Other.CoreScripts;
using Other.Template.Core;
using Other.Template.HullTemplate.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace Other.Template.HullTemplate.Scripts.Editor {
    [CustomEditor(typeof(HullData))]
    public class HullEditor : DefaultEditor {

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.HULL_VISUAL);
            
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