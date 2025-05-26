using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public static class MeshTerrainGenerator
    {
        public static Mesh GenerateTerrainMesh(float[,] heightMap, float[,,] biomeWeightMap, List<BiomeDefinition> biomes)
        {
            int height = heightMap.GetLength(0);
            int width = heightMap.GetLength(1);

            Vector3[] vertices = new Vector3[width * height];
            int[] triangles = new int[(width - 1) * (height - 1) * 6];
            Color[] colors = new Color[width * height];

            int vertIndex = 0;
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    float y = heightMap[z, x];
                    vertices[vertIndex] = new Vector3(x, y, z);

                    Color blendedColor;
                    if (biomeWeightMap == null || biomes == null || biomes.Count == 0)
                    {
                        blendedColor = Color.gray;
                    }
                    else
                    {
                        blendedColor = Color.black;
                        for (int i = 0; i < biomes.Count; i++)
                        {
                            if (biomes[i] != null)
                            {
                                blendedColor += biomeWeightMap[z, x, i] * biomes[i].color;
                            }
                        }
                    }
                    colors[vertIndex] = blendedColor;

                    vertIndex++;
                }
            }
        

            int triIndex = 0;
            for (int z = 0; z < height - 1; z++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int i = z * width + x;
                    triangles[triIndex++] = i;
                    triangles[triIndex++] = i + width;
                    triangles[triIndex++] = i + width + 1;
                    triangles[triIndex++] = i;
                    triangles[triIndex++] = i + width + 1;
                    triangles[triIndex++] = i + 1;
                }
            }
        
            Vector2[] uvs = new Vector2[width * height];
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    uvs[index] = new Vector2((float)x / (width - 1), (float)z / (height - 1));
                }
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
