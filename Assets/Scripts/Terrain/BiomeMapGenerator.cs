using System.Collections.Generic;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Terrain
{
    public static class BiomeMapGenerator
    {
        public static float[,,] GenerateBiomeWeightMap(int width, int height, int biomeCount, BiomeAlgorithm algorithm, List<BiomeDefinition> biomes, int seed)
        {
            float[,,] weightMap = new float[height, width, biomeCount];
            if (biomeCount <= 0) return weightMap;

            
            if (algorithm == BiomeAlgorithm.Voronoi)
            {
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
                                weights[i] = 1f;
                                totalWeight = 1f;
                                for (int j = 0; j < biomeCount; j++)
                                {
                                    if (j != i) weights[j] = 0f;
                                }
                                break;
                            }
                            float dx = x - biomeCenters[i].x;
                            float dz = z - biomeCenters[i].y;
                            float dist = Mathf.Sqrt(dx * dx + dz * dz);
                            float sigma = 50f;
                            weights[i] = Mathf.Exp(-(dist * dist) / (2f * sigma * sigma));
                            totalWeight += weights[i];
                        }

                        if (totalWeight > 0f)
                        {
                            for (int i = 0; i < biomeCount; i++)
                            {
                                weights[i] /= totalWeight;
                                weightMap[z, x, i] = weights[i];
                            }
                        }
                    }
                }
            }
            else if (algorithm == BiomeAlgorithm.Worley)
            {
                Random.InitState(seed + 1);
                List<Vector2Int> biomeCenterPositions = new List<Vector2Int>();

                for (int biomeIndex = 0; biomeIndex < biomeCount; biomeIndex++)
                {
                    int posX = Random.Range(0, width);
                    int posZ = Random.Range(0, height);
                    biomeCenterPositions.Add(new Vector2Int(posX, posZ));
                }

                FastNoiseLite distortionNoise = new FastNoiseLite();
                distortionNoise.SetSeed(seed + 42);
                distortionNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                distortionNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2);
                distortionNoise.SetFrequency(0.05f);

                float distortionAmplitude = 30f;
                float gaussianSigma = 70f;

                for (int z = 0; z < height; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float[] biomeWeights = new float[biomeCount];
                        float totalWeightSum = 0f;

                        for (int biomeIndex = 0; biomeIndex < biomeCount; biomeIndex++)
                        {
                            Vector2Int centerPos = biomeCenterPositions[biomeIndex];

                            float deltaX = x - centerPos.x;
                            float deltaZ = z - centerPos.y;
                            float euclideanDistance = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

                            float distortionValue = distortionNoise.GetNoise(x, z);
                            float distortedDistance = euclideanDistance + (distortionValue * distortionAmplitude);
                            
                            float weight = Mathf.Exp(-(distortedDistance * distortedDistance) / (2f * gaussianSigma * gaussianSigma));
                            biomeWeights[biomeIndex] = weight;
                            totalWeightSum += weight;
                        }

                        for (int biomeIndex = 0; biomeIndex < biomeCount; biomeIndex++)
                        {
                            weightMap[z, x, biomeIndex] = biomeWeights[biomeIndex] / totalWeightSum;
                        }
                    }
                }
            }

            else if (algorithm == BiomeAlgorithm.FloodFill)
            {
                List<Vector2Int> biomeSeeds = new List<Vector2Int>();
                System.Random rand = new System.Random(seed);
                HashSet<Vector2Int> used = new HashSet<Vector2Int>();

                for (int i = 0; i < biomeCount; i++)
                {
                    int x, y;
                    Vector2Int pos;
                    do
                    {
                        x = rand.Next(0, width);
                        y = rand.Next(0, height);
                        pos = new Vector2Int(x, y);
                    } while (used.Contains(pos));

                    used.Add(pos);
                    biomeSeeds.Add(pos);
                }

                float sigma = 40f;
                for (int z = 0; z < height; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float totalWeight = 0f;
                        float[] weights = new float[biomeCount];

                        for (int i = 0; i < biomeCount; i++)
                        {
                            Vector2Int biomeSeed = biomeSeeds[i];
                            float dx = x - biomeSeed.x;
                            float dz = z - biomeSeed.y;
                            float dist = Mathf.Sqrt(dx * dx + dz * dz);
                
                            float w = Mathf.Exp(-(dist * dist) / (2f * sigma * sigma));
                            weights[i] = w;
                            totalWeight += w;
                        }

                        for (int i = 0; i < biomeCount; i++)
                        {
                            weightMap[z, x, i] = weights[i] / totalWeight;
                        }
                    }
                }
            }

            return weightMap;
        }
    }
}
