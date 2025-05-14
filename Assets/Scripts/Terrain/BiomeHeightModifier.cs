using UnityEngine;
using System.Collections.Generic;
using Terrain;

public static class BiomeHeightModifier
{
    /// <summary>
    /// Aplikuje úpravy výškové mapy na základě biomů – násobí výšky váženým průměrem výškových multiplikátorů jednotlivých biomů.
    /// </summary>
    /// <param name="heightMap">Vstupní (a výstupní) výšková mapa terénu, která bude upravena.</param>
    /// <param name="biomeWeightMap">Mapa vah biomů pro každý bod terénu (trojrozměrné pole [z, x, biom]).</param>
    /// <param name="biomes">Seznam definic biomů odpovídající indexům v biomeWeightMap.</param>
    public static void ModifyHeightsByBiome(float[,] heightMap, float[,,] biomeWeightMap, List<BiomeDefinition> biomes)
    {
        int height = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float weightedMultiplier = 0f;
                float totalWeight = 0f;

                for (int i = 0; i < biomes.Count; i++)
                {
                    if (biomes[i] == null) continue;

                    float weight = biomeWeightMap[z, x, i];
                    float multiplier = Mathf.Clamp(biomes[i].heightMultiplier, 0.7f, 1.3f);
                    weightedMultiplier += weight * multiplier;
                    totalWeight += weight;
                }

                // Bezpečná normalizace
                float finalMultiplier = (totalWeight > 0f) ? weightedMultiplier / totalWeight : 1f;

                // Změkčení multiplikátoru – lineární interpolace mezi původní výškou a upravenou (zabrání špičkám)
                float baseHeight = heightMap[z, x];
                float modifiedHeight = baseHeight * finalMultiplier;
                heightMap[z, x] = Mathf.Lerp(baseHeight, modifiedHeight, 0.7f); // 0.7 = míra vlivu biomu
            }
        }
    }

}