using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Terrain
{
    public static class BiomeMapGenerator
    {
        /// <summary>
        /// Vygeneruje mapu biomů pro dané rozměry terénu.
        /// </summary>
        /// <param name="width">Počet bodů na ose X (šířka mapy).</param>
        /// <param name="height">Počet bodů na ose Z (délka mapy).</param>
        /// <param name="biomeCount">Počet různých biomů.</param>
        /// <param name="algorithm">Zvolený algoritmus generování biomů (None, Voronoi, Worley).</param>
        /// <param name="biomes">Seznam BiomeDefinition definující vlastnosti jednotlivých biomů.</param>
        /// <param name="seed">Seed pro generování (zajišťuje reprodukovatelnost rozmístění biomů).</param>
        /// <returns>Trojrozměrné pole [height, width, biomeCount] s váhami biomů pro každý bod terénu (součet vah = 1 pro každý bod).</returns>
        public static float[,,] GenerateBiomeWeightMap(int width, int height, int biomeCount, BiomeAlgorithm algorithm, List<BiomeDefinition> biomes, int seed)
        {
            float[,,] weightMap = new float[height, width, biomeCount];
            if (biomeCount <= 0) return weightMap;

            
            if (algorithm == BiomeAlgorithm.Voronoi)
            {
                // Algoritmus Voronoi: každému vybranému biomu přiřadíme náhodný střed (seed point) a váhy určujeme podle vzdálenosti od těchto bodů
                Random.InitState(seed + 1);
                List<Vector2Int> biomeCenters = new List<Vector2Int>();
                for (int i = 0; i < biomeCount; i++)
                {
                    int cx = Random.Range(0, width);
                    int cz = Random.Range(0, height);
                    biomeCenters.Add(new Vector2Int(cx, cz));
                }

                for (int z = 0; z < height; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float totalWeight = 0f;
                        float[] weights = new float[biomeCount];

                        for (int i = 0; i < biomeCount; i++)
                        {
                            if (x == biomeCenters[i].x && z == biomeCenters[i].y)
                            {
                                // Bod [x,z] je přesně ve středu biomu i
                                weights[i] = 1f;
                                totalWeight = 1f;
                                // Nastaví ostatní váhy na 0 a ukončí cyklus
                                for (int j = 0; j < biomeCount; j++)
                                {
                                    if (j != i) weights[j] = 0f;
                                }
                                break;
                            }
                            // Euklidovská vzdálenost od středu biomu i
                            float dx = x - biomeCenters[i].x;
                            float dz = z - biomeCenters[i].y;
                            float dist = Mathf.Sqrt(dx * dx + dz * dz);
                            float sigma = 50f; // čím větší, tím měkčí přechod
                            weights[i] = Mathf.Exp(-(dist * dist) / (2f * sigma * sigma));
                            totalWeight += weights[i];
                        }

                        // Normalizace vah tak, aby součet byl 1
                        if (totalWeight > 0f)
                        {
                            for (int i = 0; i < biomeCount; i++)
                            {
                                weights[i] /= totalWeight;
                                weightMap[z, x, i] = weights[i];
                            }
                        }
                        else
                        {
                            // Nemělo by nastat (pokud biomeCount > 0), ale pokud ano, všechny váhy zůstanou 0.
                        }
                    }
                }
            }
            else if (algorithm == BiomeAlgorithm.Worley)
            {
                FastNoiseLite noise = new FastNoiseLite();
                noise.SetSeed(seed + 1);
                noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);
                noise.SetFrequency(0.05f);

                for (int z = 0; z < height; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float baseNoise = noise.GetNoise(x, z);
                        float total = 0f;

                        for (int i = 0; i < biomeCount; i++)
                        {
                            float d = Mathf.Clamp01(1f - Mathf.Abs(baseNoise - (i / (float)biomeCount)));
                            weightMap[z, x, i] = d;
                            total += d;
                        }

                        for (int i = 0; i < biomeCount; i++)
                        {
                            weightMap[z, x, i] /= total;
                        }
                    }
                }
            }

            // Výstup: váhová mapa biomů
            return weightMap;
        }
    }
}
