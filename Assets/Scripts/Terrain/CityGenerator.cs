using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Terrain
{
    public class CityLSystem
    {
        public string Generate(int iterations, string axiom, Dictionary<char, string> rules)
        {
            string current = axiom;
            for (int i = 0; i < iterations; i++)
            {
                var next = new System.Text.StringBuilder();
                foreach (char c in current)
                {
                    next.Append(rules.ContainsKey(c) ? rules[c] : c.ToString());
                }
                current = next.ToString();
            }
            return current;
        }
    }

    public static class CityGenerator
    {
        public static GameObject GenerateCity(Vector3 startPosition, Quaternion orientation, GameObject parent, float cellSize, int iterations, List<GameObject> buildingPrefabs)
        {
            int gridSize = 20;
            bool[,] occupied = new bool[gridSize, gridSize];
            Vector2Int center = new Vector2Int(gridSize / 2, gridSize / 2);
            Vector2Int cursor = center;

            string axiom = "F";
            var rules = new Dictionary<char, string> { { 'F', "F[+F]F[-F]F" } };
            var path = new CityLSystem().Generate(iterations, axiom, rules);
            Stack<Vector2Int> stack = new();

            foreach (char c in path)
            {
                if (c == 'F')
                {
                    TryPlaceBuilding(cursor, cellSize, parent, buildingPrefabs, occupied);
                    cursor += Vector2Int.up;
                }
                else if (c == '+') cursor += Vector2Int.right;
                else if (c == '-') cursor += Vector2Int.left;
                else if (c == '[') stack.Push(cursor);
                else if (c == ']') cursor = stack.Pop();
            }

            parent.transform.position = new Vector3(startPosition.x - center.x * cellSize, startPosition.y, startPosition.z - center.y * cellSize);
            return parent;
        }

        private static void PlaceBuilding(Vector3 from, Vector3 to, GameObject parent, List<GameObject> prefabs)
        {
            if (prefabs.Count == 0) return;

            var prefab = prefabs[Random.Range(0, prefabs.Count)];
            var bounds = prefab.GetComponent<Renderer>()?.bounds;
            if (bounds == null) return;

            Vector3 size = bounds.Value.size;
            Vector3 center = from + Vector3.up * (size.y / 2f);

            if (Physics.CheckBox(center, size / 2f))
            {
                return;
            }

            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.position = from;
            obj.transform.rotation = Quaternion.LookRotation(to - from);
            obj.transform.SetParent(parent.transform, true);
        }


        public static Vector2Int FindCityCenter(float[,,] biomeMap, int biomeIndex)
        {
            int height = biomeMap.GetLength(0);
            int width = biomeMap.GetLength(1);
            float maxWeight = 0f;
            Vector2Int bestPos = new(0, 0);

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    float weight = biomeMap[z, x, biomeIndex];
                    if (weight > maxWeight)
                    {
                        maxWeight = weight;
                        bestPos = new Vector2Int(x, z);
                    }
                }
            }

            return bestPos;
        }
        
        private static bool TryPlaceBuilding(Vector2Int gridPos, float cellSize, GameObject parent, List<GameObject> prefabs, bool[,] occupied)
        {
            if (prefabs.Count == 0 || occupied[gridPos.y, gridPos.x]) return false;

            var prefab = prefabs[Random.Range(0, prefabs.Count)];
            Vector3 worldPos = new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);

            var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.position = worldPos;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.SetParent(parent.transform, true);

            occupied[gridPos.y, gridPos.x] = true;
            return true;
        }
    }
}