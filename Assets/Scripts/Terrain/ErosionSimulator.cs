namespace Terrain
{
    public class ErosionSimulator
    {
        public static void ApplyThermalErosion(float[,] heightMap, int iterations, float talus)
        {
            int height = heightMap.GetLength(0);
            int width = heightMap.GetLength(1);

            int[][] offsets = new[] { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 } };

            for (int iter = 0; iter < iterations; iter++)
            {
                float[][] deltaMap = new float[height][];
                for (int index = 0; index < height; index++)
                {
                    deltaMap[index] = new float[width];
                }

                for (int z = 1; z < height - 1; z++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        float currentHeight = heightMap[z, x];

                        for (int i = 0; i < 4; i++)
                        {
                            int nx = x + offsets[i][0];
                            int nz = z + offsets[i][1];
                            float neighborHeight = heightMap[nz, nx];

                            float diff = currentHeight - neighborHeight;

                            if (diff > talus)
                            {
                                float amountToMove = (diff - talus) * 0.5f;
                                deltaMap[z][x] -= amountToMove;
                                deltaMap[nz][nx] += amountToMove;
                            }
                        }
                    }
                }

                for (int z = 0; z < height; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        heightMap[z, x] += deltaMap[z][x];
                    }
                }
            }
        }
        
        public static void ApplyHydraulicErosion(float[,] heightMap, int iterations, float rainAmount, float evaporationRate)
{
    int height = heightMap.GetLength(0);
    int width = heightMap.GetLength(1);

    float[][] waterMap = new float[height][];
    for (int index = 0; index < height; index++)
    {
        waterMap[index] = new float[width];
    }

    float[][] sedimentMap = new float[height][];
    for (int index = 0; index < height; index++)
    {
        sedimentMap[index] = new float[width];
    }

    int[][] offsets = new[] { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { 0, 1 } };

    for (int iter = 0; iter < iterations; iter++)
    {
        for (int z = 1; z < height - 1; z++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                // 1. Dešťová voda
                waterMap[z][x] += rainAmount;

                // 2. Odvodnění do sousedů (simplified)
                float currentHeight = heightMap[z, x] + waterMap[z][x];
                float totalDelta = 0f;
                float[] flow = new float[4];

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + offsets[i][0];
                    int nz = z + offsets[i][1];
                    float neighborHeight = heightMap[nz, nx] + waterMap[nz][nx];
                    float delta = currentHeight - neighborHeight;

                    if (delta > 0)
                    {
                        flow[i] = delta;
                        totalDelta += delta;
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    if (flow[i] > 0)
                    {
                        int nx = x + offsets[i][0];
                        int nz = z + offsets[i][1];
                        float amount = (flow[i] / totalDelta) * waterMap[z][x] * 0.5f;

                        waterMap[z][x] -= amount;
                        waterMap[nz][nx] += amount;

                        float sediment = amount * 0.05f;
                        sedimentMap[z][x] -= sediment;
                        sedimentMap[nz][nx] += sediment;
                    }
                }

                // 3. Odpařování vody
                waterMap[z][x] *= (1f - evaporationRate);

                // 4. Uložení sedimentu do výškové mapy
                heightMap[z, x] += sedimentMap[z][x];
                sedimentMap[z][x] = 0f;
            }
        }
    }
}


    }
}