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
        // Parametry generování terénu zadávané uživatelem v EditorWindow GUI.
        private int seed = 0; // Seed pro generátor náhodných čísel (pro opakovatelnost výsledků).
        private bool randomizeSeed = false; // Pokud true, při generování se ignoruje zadaný seed a použije se náhodný.
        private bool generateWithVegetation = false; // Pokud true, generuje se vegetace na terénu.
        private int mapWidth = 100; // Šířka terénu (počet bodů v ose X).
        private int mapHeight = 100; // Délka terénu (počet bodů v ose Z).
        private float heightRange = 20f; // Výškový rozsah terénu (maximální výška nerovností).
        private FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2; // Zvolený typ šumu pro generování výškové mapy.
        private BiomeAlgorithm biomeAlgorithm = BiomeAlgorithm.Voronoi; // Algoritmus pro generování biomů.
        private int biomeCount = 1; // Počet biomů na mapě.
        private List<BiomeDefinition> selectedBiomes = new List<BiomeDefinition>();
        private float[,] generatedHeightMap; // Výšková mapa terénu (2D pole hodnot výšek).
        private float[,,] generatedBiomeMap; // Mapa biomů (3D pole hodnot váhy biomů pro každý bod terénu).
        private Vector2 scrollPos;
        // Seznam vybraných definic biomů (ScriptableObject). Počet prvků odpovídá biomeCount.

        // Parametry pro vytvoření výstupního GameObjectu
        private string terrainName = "ProceduralTerrain"; // Jméno generovaného objektu terénu.
        private string terrainTag = ""; // Tag pro generovaný objekt (prázdný = žádný tag).

        // Přidání položky do menu pro otevření okna nástroje v Unity Editoru.
        [MenuItem("Tools/Procedural Terrain Generator")]
        public static void ShowWindow()
        {
            // Otevře okno EditorWindow s titulem "Procedural Terrain Generator".
            GetWindow<GeneratorSettingsEditor>("Procedural Terrain Generator");
        }

        private void OnGUI()
        {
            // Hlavička okna
            GUILayout.Label("Nastavení generátoru terénu", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            // Vstupní parametry terénu
            EditorGUI.BeginDisabledGroup(randomizeSeed);
            seed = EditorGUILayout.IntField("Seed", seed);
            EditorGUI.EndDisabledGroup();
            // Toggle pro náhodné generování seedu
            randomizeSeed = EditorGUILayout.Toggle("Náhodný seed", randomizeSeed);
            generateWithVegetation = EditorGUILayout.Toggle("Generovat vegetaci", generateWithVegetation);
            mapWidth = EditorGUILayout.IntField("Šířka (X)", mapWidth);
            mapHeight = EditorGUILayout.IntField("Výška (Z)", mapHeight);
            heightRange = EditorGUILayout.FloatField("Výškový rozsah", heightRange);
            noiseType = (FastNoiseLite.NoiseType)EditorGUILayout.EnumPopup("Typ šumu", noiseType);
            biomeAlgorithm = (BiomeAlgorithm)EditorGUILayout.EnumPopup("Algoritmus biomů", biomeAlgorithm);
            biomeCount = EditorGUILayout.IntField("Počet biomů", biomeCount);
            
            if (GUILayout.Button("Přidat nový biom"))
            {
                // Open the biome creation popup window
                BiomeCreationWindow.Open(newBiome =>
                {
                    if (newBiome != null)
                    {
                        // Add the created BiomeDefinition to the list and update count
                        selectedBiomes.Add(newBiome);
                        biomeCount = selectedBiomes.Count;
                        // Ensure the main window reflects the new list
                        Repaint();
                    }
                });
            }

            // Zajistí, že seznam selectedBiomes má správnou délku odpovídající počtu biomů
            if (biomeCount < 0) biomeCount = 0;
            // Úprava délky seznamu definic biomů podle biomeCount
            while (selectedBiomes.Count < biomeCount)
            {
                selectedBiomes.Add(null); // přidá prázdnou položku, kterou může uživatel vyplnit
            }
            while (selectedBiomes.Count > biomeCount)
            {
                selectedBiomes.RemoveAt(selectedBiomes.Count - 1);
            }

            // Vstup pro výběr konkrétních BiomeDefinition pro každý biom (pokud biomeCount > 0)
            for (int i = 0; i < biomeCount; i++)
            {
                selectedBiomes[i] = (BiomeDefinition)EditorGUILayout.ObjectField(
                    $"Biome {i + 1}",
                    selectedBiomes[i],
                    typeof(BiomeDefinition),
                    false);
            }

            // Parametry výstupního objektu
            GUILayout.Space(10);
            GUILayout.Label("Nastavení výstupu", EditorStyles.boldLabel);
            terrainName = EditorGUILayout.TextField("Jméno objektu", terrainName);
            terrainTag = EditorGUILayout.TagField("Tag objektu", terrainTag);

            GUILayout.Space(10);
            // Tlačítko pro vygenerování terénu
            if (GUILayout.Button("Generovat terén"))
            {
                GenerateTerrain();

                if (generateWithVegetation)
                {
                    GameObject terrainObject = GameObject.Find(terrainName);
                    VegetationGenerator.GenerateVegetation(generatedHeightMap, generatedBiomeMap, selectedBiomes, terrainObject);
                }
            }
            
            EditorGUILayout.EndScrollView();
        }

        // Metoda, která po stisknutí tlačítka provede celé generování terénu.
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
                if (child.name == terrainName || child.name == "Vegetation")
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
            
            int actualSeed = seed;
            if (randomizeSeed)
            {
                actualSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                seed = actualSeed; // aktualizuje seed, aby byl viditelný a znovu použitelný
            }

            float[,,] biomeMap = null;
            if (biomeCount > 0)
            {
                // Použije zvolený algoritmus biomů k vytvoření mapy biomů.
                biomeMap = BiomeMapGenerator.GenerateBiomeWeightMap(mapWidth, mapHeight, biomeCount, biomeAlgorithm,
                    selectedBiomes, actualSeed);
            }
            // 1. Generování výškové mapy pomocí HeightMapGenerator
            float[,] heightMap =
                HeightMapGenerator.GenerateHeightMap(mapWidth, mapHeight, actualSeed, noiseType, heightRange, biomeMap,
                    selectedBiomes);

            // 3. Úprava výškové mapy podle biomů (pokud nějaké biomy existují a mapa biomů je k dispozici)
            if (biomeMap != null && biomeCount > 0)
            {
                BiomeHeightModifier.ModifyHeightsByBiome(heightMap, biomeMap, selectedBiomes);
            }

            Mesh terrainMesh = MeshTerrainGenerator.GenerateTerrainMesh(heightMap, biomeMap, selectedBiomes);
            
            ErosionSimulator.ApplyThermalErosion(heightMap, iterations: 10, talus: 1.0f);
            ErosionSimulator.ApplyHydraulicErosion(heightMap, iterations: 15, rainAmount: 0.2f, evaporationRate: 0.1f);

            // 5. Vytvoření nového GameObjectu v aktuální scéně s Mesh terénu a potřebnými komponentami
            GameObject terrainObject = new GameObject(terrainName);
            terrainObject.transform.SetParent(mapGO.transform, false);
            generatedHeightMap = heightMap;
            generatedBiomeMap = biomeMap;
            // Nastavení tagu, pokud je vyplněn
            if (!string.IsNullOrEmpty(terrainTag))
            {
                terrainObject.tag = terrainTag;
            }

            // Přidání komponent MeshFilter, MeshRenderer a MeshCollider
            MeshFilter mf = terrainObject.AddComponent<MeshFilter>();
            MeshRenderer mr = terrainObject.AddComponent<MeshRenderer>();
            MeshCollider mc = terrainObject.AddComponent<MeshCollider>();
            // Přiřazení vygenerovaného meshe do MeshFilteru a MeshCollideru
            mf.sharedMesh = terrainMesh;
            mc.sharedMesh = terrainMesh;
            // Nastavení základního materiálu pro MeshRenderer (zde standardní shader, jednobarevný šedý materiál)
            Material mat = null;
            if (selectedBiomes != null && selectedBiomes.Count > 0)
            {
                
                BiomeDefinition dominantBiome = selectedBiomes[0];
                if (dominantBiome != null && dominantBiome.biomeMaterial != null)
                {
                    mat = dominantBiome.biomeMaterial;
                }
            }

            mr.sharedMaterial = mat ?? new Material(Shader.Find("Standard")); // fallback

            // Zaregistrování akce do Undo systému Unity (umožní vrátit vytvoření objektu zpět pomocí Ctrl+Z)
            Undo.RegisterCreatedObjectUndo(terrainObject, "Create Procedural Terrain");

            // Výpis do konzole s informací o použitém seedu
            if (randomizeSeed)
            {
                Debug.Log("Procedurální terén vygenerován: " + terrainObject.name + " (náhodný seed " + actualSeed + ")");
            }
            else
            {
                Debug.Log("Procedurální terén vygenerován: " + terrainObject.name + " (seed " + actualSeed + ")");
            }
        }
    }
}
