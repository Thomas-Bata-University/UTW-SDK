using Terrain;
using UnityEditor;

namespace Editor.MapGenerator
{
    [CustomEditor(typeof(BiomeDefinition))]
    public class BiomeDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BiomeDefinition biome = (BiomeDefinition)target;

            biome.biomeName = EditorGUILayout.TextField("Název biomu", biome.biomeName);
            biome.color = EditorGUILayout.ColorField("Barva", biome.color);
            biome.heightMultiplier = EditorGUILayout.Slider("Výškový multiplikátor", biome.heightMultiplier, 0f, 2f);
            biome.noiseScale = EditorGUILayout.Slider("Škála šumu", biome.noiseScale, 0f, 1f);
        
            EditorGUILayout.Space(10);
            biome.generateVegetation = EditorGUILayout.Toggle("Generovat vegetaci", biome.generateVegetation);
            if (biome.generateVegetation)
            {
                biome.vegetationCoverage = EditorGUILayout.Slider("Pokrytí vegetací", biome.vegetationCoverage, 0f, 1f);

                biome.includeGrass = EditorGUILayout.Toggle("Zahrnout trávu", biome.includeGrass);
                if (biome.includeGrass)
                {
                    biome.grassDensity = EditorGUILayout.Slider("Hustota trávy", biome.grassDensity, 0f, 1f);
                }

                biome.includeShrubs = EditorGUILayout.Toggle("Zahrnout keře", biome.includeShrubs);
                biome.shrubMinDistance = EditorGUILayout.FloatField("Rozestup keřů", biome.shrubMinDistance);

                biome.includeTrees = EditorGUILayout.Toggle("Zahrnout stromy", biome.includeTrees);
                biome.treeMinDistance = EditorGUILayout.FloatField("Rozestup stromů", biome.treeMinDistance);

                biome.useClusters = EditorGUILayout.Toggle("Použít shluky", biome.useClusters);
                if (biome.useClusters)
                {
                    biome.clusterDensity = EditorGUILayout.Slider("Hustota shluků", biome.clusterDensity, 0f, 1f);
                }
            }

            EditorUtility.SetDirty(biome);
        }
    }
}