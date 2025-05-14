using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Terrain;
using Utilities; // předpokládejme namespace Terrain pro BiomeDefinition

public static class VegetationGenerator
{
    public static void GenerateVegetation(
        float[,] heightMap, float[,,] biomeWeightMap, 
        List<BiomeDefinition> biomes, GameObject terrainObject)
    {
        int width = heightMap.GetLength(1);
        int height = heightMap.GetLength(0);
        
        // 1. Určení dominantního biomu pro každou buňku
        int[,] dominantBiome = new int[height, width];
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int bestIndex = 0;
                float bestWeight = 0f;
                for (int b = 0; b < biomes.Count; b++)
                {
                    float w = biomeWeightMap[z, x, b];
                    if (w > bestWeight)
                    {
                        bestWeight = w;
                        bestIndex = b;
                    }
                }
                dominantBiome[z, x] = bestIndex;
            }
        }
        
        // Příprava rodičovského objektu Vegetation
        GameObject vegetationParent = new GameObject("Vegetation");
        
        // 2. Načtení prefabů pro každý biom (dle složek)
        Dictionary<int, List<GameObject>> treePrefabs = new Dictionary<int, List<GameObject>>();
        Dictionary<int, List<GameObject>> shrubPrefabs = new Dictionary<int, List<GameObject>>();
        Dictionary<int, List<GameObject>> grassPrefabs = new Dictionary<int, List<GameObject>>();
        
        for (int b = 0; b < biomes.Count; b++)
        {
            BiomeDefinition biome = biomes[b];
            if (biome != null && biome.generateVegetation)
            {
                // Najdi a načti všechny prefaby v určené složce
                treePrefabs[b] = new List<GameObject>();
                shrubPrefabs[b] = new List<GameObject>();
                grassPrefabs[b] = new List<GameObject>();
                string folder = biome.vegetationPrefabsFolder;
                string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null) continue;
                    string name = prefab.name.ToLower();
                    if (biome.includeTrees && name.Contains("tree"))
                        treePrefabs[b].Add(prefab);
                    if (biome.includeShrubs && name.Contains("shrub"))
                        shrubPrefabs[b].Add(prefab);
                    if (biome.includeGrass && name.Contains("grass"))
                        grassPrefabs[b].Add(prefab);
                }
            }
        }
        
        // 3. Generování pozic pro vegetaci
        List<(Vector3 pos, GameObject prefab)> instancesToPlace = new List<(Vector3, GameObject)>();
        
        // Pro každý biom samostatně:
        for (int b = 0; b < biomes.Count; b++)
        {
            BiomeDefinition biome = biomes[b];
            if (biome == null || !biome.generateVegetation) continue;
            
            // Shromážděné prefaby pro daný biom
            List<GameObject> treeList = treePrefabs.ContainsKey(b) ? treePrefabs[b] : null;
            List<GameObject> shrubList = shrubPrefabs.ContainsKey(b) ? shrubPrefabs[b] : null;
            List<GameObject> grassList = grassPrefabs.ContainsKey(b) ? grassPrefabs[b] : null;
            
            // (a) Stromy - Poisson disk sampling
            if (biome.includeTrees && treeList != null && treeList.Count > 0)
            {
                float R = biome.treeMinDistance;
                List<Vector2> treePositions = PoissonDiskSample(width, height, R,
                    (x,z) => dominantBiome[(int)z, (int)x] == b);
                foreach (Vector2 pos2D in treePositions)
                {
                    if (Random.value > biome.vegetationCoverage)
                        continue;
                    // výška terénu pro tuto pozici
                    float y = GetTerrainHeight(heightMap, pos2D.x, pos2D.y);
                    // vyber náhodný stromí prefab a ulož instanci k pozdějšímu vytvoření
                    GameObject prefab = treeList[Random.Range(0, treeList.Count)];
                    Vector3 position = new Vector3(pos2D.x, y, pos2D.y) + terrainObject.transform.position;
                    instancesToPlace.Add((position, prefab));
                }
            }
            
            // (b) Keře - Poisson disk sampling
            if (biome.includeShrubs && shrubList != null && shrubList.Count > 0)
            {
                float R = biome.shrubMinDistance;
                List<Vector2> shrubPositions = PoissonDiskSample(width, height, R,
                    (x,z) => dominantBiome[(int)z, (int)x] == b);
                foreach (Vector2 pos2D in shrubPositions)
                {
                    if (Random.value > biome.vegetationCoverage)
                        continue;
                    float y = GetTerrainHeight(heightMap, pos2D.x, pos2D.y);
                    GameObject prefab = shrubList[Random.Range(0, shrubList.Count)];
                    Vector3 position = new Vector3(pos2D.x, y, pos2D.y) + terrainObject.transform.position;
                    instancesToPlace.Add((position, prefab));
                }
            }
            
            // (c) Tráva - náhodné rozmístění
            if (biome.includeGrass && grassList != null && grassList.Count > 0 && biome.grassDensity > 0f)
            {
                // odhad počtu instancí trávy
                float area = 0;
                // spočítáme počet buněk náležících biomu b (dominantně)
                for (int z = 0; z < height; z++)
                    for (int x = 0; x < width; x++)
                        if (dominantBiome[z,x] == b) area += 1;
                int grassCount = Mathf.RoundToInt(area * biome.grassDensity * biome.vegetationCoverage);
                
                for (int i = 0; i < grassCount; i++)
                {
                    // náhodně vyber bod v hranicích biomu
                    int randX, randZ;
                    do {
                        randX = Random.Range(0, width);
                        randZ = Random.Range(0, height);
                    } while (dominantBiome[randZ, randX] != b);
                    
                    // malý náhodný offset uvnitř buňky, aby tráva nebyla jen přesně na mřížce
                    float fx = randX + Random.value;
                    float fz = randZ + Random.value;
                    if (fx >= width) fx = width - 0.001f;
                    if (fz >= height) fz = height - 0.001f;
                    
                    float y = GetTerrainHeight(heightMap, fx, fz);
                    GameObject prefab = grassList[Random.Range(0, grassList.Count)];
                    Vector3 position = new Vector3(fx, y, fz) + terrainObject.transform.position;
                    instancesToPlace.Add((position, prefab));
                }
            }
        }
        
        // 4. Instancování všech objektů ve scéně s variacemi
        foreach (var (position, prefab) in instancesToPlace)
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.position = position;
            obj.transform.SetParent(vegetationParent.transform);
            // náhodná rotace kolem Y
            float rotY = Random.Range(0f, 360f);
            obj.transform.rotation = Quaternion.Euler(0, rotY, 0);
            // náhodné mírné zmenšení/zvětšení
            float scaleFactor = Random.Range(0.9f, 1.1f);
            obj.transform.localScale = Vector3.one * scaleFactor;
            // (ponecháme sdílené materiály – neupravujeme obj.GetComponent<Renderer>().material)
        }
    }
    
    // Pomocná funkce: bilineární interpolace výšky
    private static float GetTerrainHeight(float[,] heightMap, float x, float z)
    {
        int width = heightMap.GetLength(1);
        int height = heightMap.GetLength(0);
        // indexy mřížky
        int x0 = Mathf.FloorToInt(x);
        int z0 = Mathf.FloorToInt(z);
        int x1 = Mathf.Min(x0 + 1, width - 1);
        int z1 = Mathf.Min(z0 + 1, height - 1);
        // frakce
        float tx = x - x0;
        float tz = z - z0;
        // výšky ve vrcholech buňky
        float h00 = heightMap[z0, x0];
        float h10 = heightMap[z0, x1];
        float h01 = heightMap[z1, x0];
        float h11 = heightMap[z1, x1];
        // interpolace
        float hz0 = Mathf.Lerp(h00, h10, tx);
        float hz1 = Mathf.Lerp(h01, h11, tx);
        float h = Mathf.Lerp(hz0, hz1, tz);
        return h;
    }
    
    // Pomocná funkce: Poisson disk sampling (Bridson)
    // (Zde jen deklarace pro ilustraci - implementace by generovala body s daným min. rozestupem)
    private static List<Vector2> PoissonDiskSample(int width, int height, float minDist, System.Func<float,float,bool> isInBiome)
    {
        var allPoints = FastPoissonDiskSampling.Sampling(
            new Vector2(0, 0),
            new Vector2(width, height),
            minDist
        );

        // Filtrování bodů podle biomu
        return allPoints.FindAll(p => isInBiome(p.x, p.y));
    }
}
