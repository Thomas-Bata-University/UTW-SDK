using Other.Template.PlateTemplate.Data;
using UnityEditor;

namespace Editor.Plate {
    [CustomEditor(typeof(PlateData))]
    public class PlateEditor : UnityEditor.Editor {

        protected SerializedProperty armorQualityProp;
        protected SerializedProperty tinyFragmentChanceProp;
        protected SerializedProperty mediumFragmentChanceProp;
        protected SerializedProperty sizableFragmentChanceProp;

        private void OnEnable() {
            armorQualityProp = serializedObject.FindProperty("armorQuality");
            tinyFragmentChanceProp = serializedObject.FindProperty("tinyFragmentChance");
            mediumFragmentChanceProp = serializedObject.FindProperty("mediumFragmentChance");
            sizableFragmentChanceProp = serializedObject.FindProperty("sizableFragmentChance");
        }

    }
} //END