using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Terrain
{
    public static class HeightMapGenerator
    {
        public static float[,] GenerateHeightMap(
            int width, int height,
            int seed,
            FastNoiseLite.NoiseType noiseType,
            float heightRange,
            float[,,] biomeWeightMap,
            List<BiomeDefinition> biomes)
        {
            float[,] heightMap = new float[height, width];

            List<float[,]> biomeNoiseMaps = new List<float[,]>();
            Random.InitState(seed);
            float offsetX = Random.value * 1000f;
            float offsetZ = Random.value * 1000f;

            for (int b = 0; b < biomes.Count; b++)
            {
                float scale = Mathf.Clamp(biomes[b].noiseScale, 0.01f, 0.2f);
                float[,] map = new float[height, width];

                FastNoiseLite noise = new FastNoiseLite();
                noise.SetSeed(seed);
                noise.SetNoiseType(noiseType);
                noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                noise.SetFractalOctaves(5);

                for (int z = 0; z < height; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float sampleX = x * scale + offsetX;
                        float sampleZ = z * scale + offsetZ;
                        float value = noise.GetNoise(sampleX, sampleZ);
                        map[z, x] = (value + 1f) * 0.5f;
                    }
                }

                biomeNoiseMaps.Add(map);
            }

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = 0f;
                    float totalWeight = 0f;

                    for (int b = 0; b < biomes.Count; b++)
                    {
                        float weight = biomeWeightMap[z, x, b];
                        value += biomeNoiseMaps[b][z, x] * weight;
                        totalWeight += weight;
                    }

                    if (totalWeight > 0f)
                        value /= totalWeight;

                    heightMap[z, x] = value * heightRange;
                }
            }

            return heightMap;
        }
    }
}
