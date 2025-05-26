using System.Collections.Generic;
using Core;
using Editor.Core;
using Terrain;
using UnityEditor;
using UnityEngine;

namespace Editor.MapGenerator
{
    public class GeneratorSettingsEditor : EditorWindow
    {
        private int seed = 0;
        private bool randomizeSeed = false;
        private bool generateWithVegetation = false;
        //private bool generateCities = false;
        private int mapWidth = 100;
        private int mapHeight = 100;
        private float heightRange = 20f;
        private FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;
        private BiomeAlgorithm biomeAlgorithm = BiomeAlgorithm.Worley;
        private int biomeCount = 1;
        private List<BiomeDefinition> selectedBiomes = new List<BiomeDefinition>();
        private float[,] generatedHeightMap;
        private float[,,] generatedBiomeMap;
        private Vector2 scrollPos;

        private string terrainName = "ProceduralTerrain";
        private string terrainTag = "";

        [MenuItem("Tools/Procedural Terrain Generator")]
        public static void ShowWindow()
        {
            GetWindow<GeneratorSettingsEditor>("Procedural Terrain Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Nastavení generátoru terénu", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUI.BeginDisabledGroup(randomizeSeed);
            seed = EditorGUILayout.IntField("Seed", seed);
            EditorGUI.EndDisabledGroup();
            randomizeSeed = EditorGUILayout.Toggle("Náhodný seed", randomizeSeed);
            generateWithVegetation = EditorGUILayout.Toggle("Generovat vegetaci", generateWithVegetation);
            mapWidth = EditorGUILayout.IntField("Šířka (X)", mapWidth);
            mapHeight = EditorGUILayout.IntField("Délka (Z)", mapHeight);
            heightRange = EditorGUILayout.FloatField("Výškový rozsah", heightRange);
            biomeCount = EditorGUILayout.IntField("Počet biomů", biomeCount);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Přidat nový biom"))
            {
                BiomeCreationWindow.Open(newBiome =>
                {
                    if (newBiome != null)
                    {
                        selectedBiomes.Add(newBiome);
                        biomeCount = selectedBiomes.Count;
                        Repaint();
                    }
                });
            }

            if (GUILayout.Button("Upravit biom"))
            {
                BiomeEditWindow.Open();
            }
            EditorGUILayout.EndHorizontal();

            if (biomeCount < 0) biomeCount = 0;
            while (selectedBiomes.Count < biomeCount)
            {
                selectedBiomes.Add(null);
            }
            while (selectedBiomes.Count > biomeCount)
            {
                selectedBiomes.RemoveAt(selectedBiomes.Count - 1);
            }

            for (int i = 0; i < biomeCount; i++)
            {
                selectedBiomes[i] = (BiomeDefinition)EditorGUILayout.ObjectField(
                    $"Biome {i + 1}",
                    selectedBiomes[i],
                    typeof(BiomeDefinition),
                    false);
            }

            GUILayout.Space(10);
            GUILayout.Label("Nastavení výstupu", EditorStyles.boldLabel);
            terrainName = EditorGUILayout.TextField("Jméno objektu", terrainName);
            terrainTag = EditorGUILayout.TagField("Tag objektu", terrainTag);

            GUILayout.Space(10);
            if (GUILayout.Button("Generovat terén"))
            {
                GenerateTerrain();
                
                bool[,] cityMask = null;

                if (generateWithVegetation)
                {
                    GameObject oldVegetation = GameObject.Find("Vegetation");
                    if (oldVegetation != null)
                    {
                        Undo.DestroyObjectImmediate(oldVegetation);
                    }
                    
                    GameObject terrainObject = GameObject.Find(terrainName);
                    VegetationGenerator.GenerateVegetation(generatedHeightMap, generatedBiomeMap, selectedBiomes, terrainObject, cityMask);
                }
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void GenerateTerrain()
        {
            GameObject mapGO = GameObject.FindWithTag(Tags.MAP_VISUAL);
            if (mapGO == null)
            {
                Debug.LogError("Map GameObject nebyl nalezen ve scéně. Ujisti se, že existuje GameObject s názvem 'Map'.");
                return;
            }
            
            foreach (Transform child in mapGO.transform)
            {
                if (child.name == terrainName)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
            
            int actualSeed = seed;
            if (randomizeSeed)
            {
                actualSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                seed = actualSeed;
            }

            float[,,] biomeMap = null;
            if (biomeCount > 0)
            {
                biomeMap = BiomeMapGenerator.GenerateBiomeWeightMap(mapWidth, mapHeight, biomeCount, biomeAlgorithm,
                    selectedBiomes, actualSeed);
            }
            
            float[,] heightMap =
                HeightMapGenerator.GenerateHeightMap(mapWidth, mapHeight, actualSeed, noiseType, heightRange, biomeMap,
                    selectedBiomes);
            
            if (biomeMap != null && biomeCount > 0)
            {
                BiomeHeightModifier.ModifyHeightsByBiome(heightMap, biomeMap, selectedBiomes);
            }

            Mesh terrainMesh = MeshTerrainGenerator.GenerateTerrainMesh(heightMap, biomeMap, selectedBiomes);
            
            ErosionSimulator.ApplyThermalErosion(heightMap, iterations: 10, talus: 1.0f);
            ErosionSimulator.ApplyHydraulicErosion(heightMap, iterations: 15, rainAmount: 0.2f, evaporationRate: 0.1f);

            GameObject terrainObject = new GameObject(terrainName);
            terrainObject.transform.SetParent(mapGO.transform, false);
            generatedHeightMap = heightMap;
            generatedBiomeMap = biomeMap;
            if (!string.IsNullOrEmpty(terrainTag))
            {
                terrainObject.tag = terrainTag;
            }

            MeshFilter mf = terrainObject.AddComponent<MeshFilter>();
            MeshRenderer mr = terrainObject.AddComponent<MeshRenderer>();
            MeshCollider mc = terrainObject.AddComponent<MeshCollider>();
            mf.sharedMesh = terrainMesh;
            mc.sharedMesh = terrainMesh;
            Material mat = null;
            if (selectedBiomes != null && selectedBiomes.Count > 0)
            {
                
                BiomeDefinition dominantBiome = selectedBiomes[0];
                if (dominantBiome != null && dominantBiome.biomeMaterial != null)
                {
                    mat = dominantBiome.biomeMaterial;
                }
            }

            mr.sharedMaterial = mat ?? new Material(Shader.Find("Standard"));

            Undo.RegisterCreatedObjectUndo(terrainObject, "Create Procedural Terrain");

            if (randomizeSeed)
            {
                Debug.Log("Procedurální terén vygenerován: " + terrainObject.name + " (náhodný seed " + actualSeed + ")");
            }
            else
            {
                Debug.Log("Procedurální terén vygenerován: " + terrainObject.name + " (seed " + actualSeed + ")");
            }
        }
        
        private List<GameObject> LoadCityPrefabs(BiomeDefinition biome)
        {
            List<GameObject> buildings = new();
            if (biome == null || string.IsNullOrWhiteSpace(biome.biomeName)) return buildings;

            string folderPath = $"Assets/Biomes/{biome.biomeName}/City";
            if (!AssetDatabase.IsValidFolder(folderPath)) return buildings;

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                    buildings.Add(prefab);
            }

            return buildings;
        }
    }
}
