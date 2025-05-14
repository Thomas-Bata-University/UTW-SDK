using Core;
using UnityEngine;

namespace Utilities
{
    public class SimplexNoiseGenerator : MonoBehaviour
    {
        public float[,] GenerateHeightNoise(
            int meshSize, 
            int gridRes,
            int seed,
            float scale,
            int octaves,
            float persistence,
            float lacunarity)
        {
            int w = gridRes + 1, h = gridRes + 1;
            float[,] noiseMap = new float[w, h];
            
            FastNoiseLite noise = new FastNoiseLite(seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            
            float step = meshSize / (float)gridRes;

            for (int z = 0; z < h; z++)
            for (int x = 0; x < w; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                float maxValue = 0;

                Vector2 pos = new Vector2(x * step, z * step);
                
                float warpX = noise.GetNoise(pos.x * 0.2f, pos.y * 0.2f) * 2f;
                float warpY = noise.GetNoise(pos.x * 0.2f + 1000, pos.y * 0.2f + 1000) * 2f;
                pos += new Vector2(warpX, warpY);

                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = pos.x / scale * frequency;
                    float sampleZ = pos.y / scale * frequency;
                    
                    float noiseValue = noise.GetNoise(sampleX, sampleZ);
                    noiseHeight += noiseValue * amplitude;
                    maxValue += amplitude;
                    
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                
                noiseMap[x, z] = Mathf.Clamp01((noiseHeight / maxValue + 1) / 2);
            }

            return noiseMap;
        }
    }
}