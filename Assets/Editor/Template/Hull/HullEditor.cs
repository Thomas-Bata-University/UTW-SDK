using Editor.Core;
using Other.Template.HullTemplate.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Hull {
    [CustomEditor(typeof(HullData))]
    public class HullEditor : DefaultEditor {

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.HULL_VISUAL);

            FindDefaultProperties();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            CreateInspector();
            LoadAssetBundles();
            AssetBundle();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void CreateInspector() {
            base.CreateInspector();
            //TODO Add more settings
        }

    }
} //END