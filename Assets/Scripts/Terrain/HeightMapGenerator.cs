using System.Collections.Generic;
using Core;
using Terrain;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public static class HeightMapGenerator
{
    /// <summary>
    /// Vygeneruje výškovou mapu (heightMap) o daných rozměrech pomocí zadaného typu šumu.
    /// </summary>
    /// <param name="width">Počet vzorků na ose X (šířka mapy).</param>
    /// <param name="height">Počet vzorků na ose Z (délka mapy).</param>
    /// <param name="seed">Seed pro generátor šumu (zajišťuje reprodukovatelnost).</param>
    /// <param name="noiseType">Typ šumu (Perlin, Simplex,...).</param>
    /// <param name="heightRange">Maximální výškový rozsah (amplituda) terénu.</param>
    /// <returns>Dvourozměrné pole [height, width] s generovanými hodnotami výšek (0..heightRange).</returns>
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

        // Vygeneruj šumové mapy pro každý biom
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
                    map[z, x] = (value + 1f) * 0.5f; // normalizace do 0–1
                }
            }

            biomeNoiseMaps.Add(map);
        }

        // Vytvoř výstupní výškovou mapu smícháním jednotlivých map
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
