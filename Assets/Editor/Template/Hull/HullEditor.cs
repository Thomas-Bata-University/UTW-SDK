using Editor.Const;
using Other.Template.HullTemplate.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Template.Hull {
    [CustomEditor(typeof(HullData))]
    public class HullEditor : DefaultEditor {

        private void OnEnable() {
            visual = GameObject.FindWithTag(Tags.HULL_VISUAL);

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