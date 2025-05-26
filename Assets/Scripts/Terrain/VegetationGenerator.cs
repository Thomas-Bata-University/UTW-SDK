using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace Terrain
{
    public static class VegetationGenerator
    {
        public static void GenerateVegetation(
            float[,] heightMap, float[,,] biomeWeightMap, 
            List<BiomeDefinition> biomes, GameObject terrainObject, bool[,] cityMask)
        {
            int width = heightMap.GetLength(1);
            int height = heightMap.GetLength(0);
        
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
        
            GameObject vegetationParent = new GameObject("Vegetation");
            vegetationParent.transform.SetParent(terrainObject.transform, false);
        
            Dictionary<int, List<GameObject>> treePrefabs = new();
            Dictionary<int, List<GameObject>> shrubPrefabs = new();
            Dictionary<int, List<GameObject>> grassPrefabs = new();
        
            for (int b = 0; b < biomes.Count; b++)
            {
                BiomeDefinition biome = biomes[b];
                if (biome != null && biome.generateVegetation)
                {
                    treePrefabs[b] = new();
                    shrubPrefabs[b] = new();
                    grassPrefabs[b] = new();
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
        
            List<(Vector3 pos, GameObject prefab)> instancesToPlace = new List<(Vector3, GameObject)>();
        
            for (int b = 0; b < biomes.Count; b++)
            {
                BiomeDefinition biome = biomes[b];
                if (biome == null || !biome.generateVegetation) continue;
            
                List<GameObject> treeList = treePrefabs.ContainsKey(b) ? treePrefabs[b] : null;
                List<GameObject> shrubList = shrubPrefabs.ContainsKey(b) ? shrubPrefabs[b] : null;
                List<GameObject> grassList = grassPrefabs.ContainsKey(b) ? grassPrefabs[b] : null;
            
                if (biome.includeTrees && treeList?.Count > 0)
                {
                    float R = biome.treeMinDistance;
                    List<Vector2> positions = GenerateClusteredPositions(width, height, R, biome.vegetationCoverage,
                        (x, z) => dominantBiome[(int)z, (int)x] == b && (cityMask == null || !cityMask[(int)z, (int)x]),
                        biome.useClusters, biome.clusterDensity);
                    foreach (Vector2 pos in positions)
                    {
                        float y = GetTerrainHeight(heightMap, pos.x, pos.y);
                        GameObject prefab = treeList[Random.Range(0, treeList.Count)];
                        Vector3 position = new Vector3(pos.x, y, pos.y) + terrainObject.transform.position;
                        instancesToPlace.Add((position, prefab));
                    }
                }

                if (biome.includeShrubs && shrubList?.Count > 0)
                {
                    float R = biome.shrubMinDistance;
                    List<Vector2> positions = GenerateClusteredPositions(width, height, R, biome.vegetationCoverage,
                        (x, z) => dominantBiome[(int)z, (int)x] == b && (cityMask == null || !cityMask[(int)z, (int)x]),
                        biome.useClusters, biome.clusterDensity);
                    foreach (Vector2 pos in positions)
                    {
                        float y = GetTerrainHeight(heightMap, pos.x, pos.y);
                        GameObject prefab = shrubList[Random.Range(0, shrubList.Count)];
                        Vector3 position = new Vector3(pos.x, y, pos.y) + terrainObject.transform.position;
                        instancesToPlace.Add((position, prefab));
                    }
                }
            
                if (biome.includeGrass && grassList != null && grassList.Count > 0 && biome.grassDensity > 0f)
                {
                    float area = 0;
                    for (int z = 0; z < height; z++)
                    for (int x = 0; x < width; x++)
                        if (dominantBiome[z,x] == b && (cityMask == null || !cityMask[z, x])) area += 1;
                    float R = Mathf.Max(1f, Mathf.Lerp(0.5f, 2f, 1f - biome.grassDensity));
                    List<Vector2> positions = GenerateClusteredPositions(width, height, R, biome.vegetationCoverage,
                        (x, z) => dominantBiome[(int)z, (int)x] == b && (cityMask == null || !cityMask[(int)z, (int)x]),
                        biome.useClusters, biome.clusterDensity);

                    foreach (Vector2 pos in positions)
                    {
                        float y = GetTerrainHeight(heightMap, pos.x, pos.y);
                        GameObject prefab = grassList[Random.Range(0, grassList.Count)];
                        Vector3 position = new Vector3(pos.x, y, pos.y) + terrainObject.transform.position;
                        instancesToPlace.Add((position, prefab));
                    }
                }
            }
        
            foreach (var (position, prefab) in instancesToPlace)
            {
                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                obj.transform.position = position;
                obj.transform.SetParent(vegetationParent.transform);
                float rotY = Random.Range(0f, 360f);
                obj.transform.rotation = Quaternion.Euler(0, rotY, 0);
                float scaleFactor = Random.Range(0.9f, 1.1f);
                obj.transform.localScale = Vector3.one * scaleFactor;
            }
        }
    
        private static float GetTerrainHeight(float[,] heightMap, float x, float z)
        {
            int width = heightMap.GetLength(1);
            int height = heightMap.GetLength(0);
            int x0 = Mathf.FloorToInt(x);
            int z0 = Mathf.FloorToInt(z);
            int x1 = Mathf.Min(x0 + 1, width - 1);
            int z1 = Mathf.Min(z0 + 1, height - 1);
            float tx = x - x0;
            float tz = z - z0;
            float h00 = heightMap[z0, x0];
            float h10 = heightMap[z0, x1];
            float h01 = heightMap[z1, x0];
            float h11 = heightMap[z1, x1];
            float hz0 = Mathf.Lerp(h00, h10, tx);
            float hz1 = Mathf.Lerp(h01, h11, tx);
            float h = Mathf.Lerp(hz0, hz1, tz);
            return h;
        }

        private static List<Vector2> GenerateClusteredPositions(
            int width, int height, float minDist, float coverage,
            System.Func<float, float, bool> isInBiome,
            bool useClusters = true, float clusterDensity = 0.5f)
        {
            List<Vector2> allPoints = new();

            if (!useClusters || coverage >= 0.95f)
            {
                List<Vector2> uniformPoints = FastPoissonDiskSampling.Sampling(
                    new Vector2(0, 0),
                    new Vector2(width, height),
                    minDist);

                foreach (var p in uniformPoints)
                {
                    if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height &&
                        isInBiome(p.x, p.y) && Random.value <= coverage)
                    {
                        allPoints.Add(p);
                    }
                }
            }
            else
            {
                int maxClusterCount = 20;
                int clusterCount = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(1f, maxClusterCount, clusterDensity)), 1, maxClusterCount);

                float totalTargetArea = coverage * width * height;
                float clusterRadius = Mathf.Sqrt(totalTargetArea / (clusterCount * Mathf.PI));

                float adjustedCoverage = Mathf.Clamp01(coverage * 1.25f);

                for (int c = 0; c < clusterCount; c++)
                {
                    int cx, cz;
                    int attempts = 0;
                    do
                    {
                        cx = Random.Range(0, width);
                        cz = Random.Range(0, height);
                        attempts++;
                    } while (!isInBiome(cx, cz) && attempts < 100);

                    if (attempts >= 100) continue;

                    float jitterX = Random.Range(-0.5f, 0.5f) * clusterRadius * 0.2f;
                    float jitterZ = Random.Range(-0.5f, 0.5f) * clusterRadius * 0.2f;
                    Vector2 center = new Vector2(cx + jitterX, cz + jitterZ);

                    List<Vector2> cluster = FastPoissonDiskSampling.Sampling(
                        center - Vector2.one * clusterRadius,
                        center + Vector2.one * clusterRadius,
                        minDist * 0.8f);

                    foreach (var p in cluster)
                    {
                        Vector2 local = p - center;
                        local.x *= Random.Range(0.8f, 1.2f);
                        local.y *= Random.Range(0.8f, 1.2f);

                        float nx = p.x / width;
                        float nz = p.y / height;
                        float noiseValue = Mathf.PerlinNoise(nx * 5f, nz * 5f);
                        if (local.magnitude > clusterRadius || noiseValue < 0.4f) continue;

                        if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height &&
                            isInBiome(p.x, p.y) && Random.value <= adjustedCoverage)
                        {
                            allPoints.Add(p);
                        }
                    }
                }
            }

            return allPoints;
        }
    }  
}