using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public static class BiomeHeightModifier
    {
        public static void ModifyHeightsByBiome(float[,] heightMap, float[,,] biomeWeightMap, List<BiomeDefinition> biomes)
        {
            int height = heightMap.GetLength(0);
            int width = heightMap.GetLength(1);
            int biomeCount = biomes.Count;

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    float weightedMultiplier = 0f;
                    float totalWeight = 0f;

                    for (int i = 0; i < biomeCount; i++)
                    {
                        if (biomes[i] == null) continue;

                        float weight = biomeWeightMap[z, x, i];
                        float multiplier = Mathf.Clamp(biomes[i].heightMultiplier, 0.7f, 1.3f);
                        weightedMultiplier += weight * multiplier;
                        totalWeight += weight;
                    }

                    float finalMultiplier = (totalWeight > 0f) ? weightedMultiplier / totalWeight : 1f;

                    float gradientMagnitude = ComputeBiomeWeightGradient(biomeWeightMap, x, z, biomeCount, width, height);
                    float blendFactor = Mathf.Clamp01(1f - gradientMagnitude * 1.5f);
                    blendFactor = Mathf.SmoothStep(0f, 1f, blendFactor);

                    float baseHeight = heightMap[z, x];
                    float modifiedHeight = baseHeight * finalMultiplier;

                    float maxDelta = 0.2f * baseHeight;
                    float delta = Mathf.Abs(modifiedHeight - baseHeight);
                    if (delta > maxDelta)
                    {
                        modifiedHeight = baseHeight + Mathf.Sign(modifiedHeight - baseHeight) * maxDelta;
                    }

                    heightMap[z, x] = Mathf.Lerp(baseHeight, modifiedHeight, blendFactor * 0.7f);
                }
            }
        }

        private static float ComputeBiomeWeightGradient(float[,,] biomeWeightMap, int x, int z, int biomeCount, int width, int height)
        {
            float maxDiff = 0f;

            for (int i = 0; i < biomeCount; i++)
            {
                float center = biomeWeightMap[z, x, i];
                float sumDiff = 0f;
                int count = 0;

                if (x > 0)
                {
                    sumDiff += Mathf.Abs(center - biomeWeightMap[z, x - 1, i]);
                    count++;
                }
                if (x < width - 1)
                {
                    sumDiff += Mathf.Abs(center - biomeWeightMap[z, x + 1, i]);
                    count++;
                }
                if (z > 0)
                {
                    sumDiff += Mathf.Abs(center - biomeWeightMap[z - 1, x, i]);
                    count++;
                }
                if (z < height - 1)
                {
                    sumDiff += Mathf.Abs(center - biomeWeightMap[z + 1, x, i]);
                    count++;
                }

                if (count > 0)
                {
                    float avgDiff = sumDiff / count;
                    maxDiff = Mathf.Max(maxDiff, avgDiff);
                }
            }

            return maxDiff;
        }
    }
}
