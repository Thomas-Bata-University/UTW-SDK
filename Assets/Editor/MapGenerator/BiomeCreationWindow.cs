using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Terrain;

namespace Editor.MapGenerator
{
    public class BiomeCreationWindow : EditorWindow
    {
        private string biomeName = "NewBiome";
        private Color color = Color.gray;
        private Material biomeMaterial;
        private float heightMultiplier = 1f;
        private float noiseScale = 0.1f;

        private bool generateVegetation = false;
        private float vegetationCoverage = 1.0f;
        private bool useClusters = false;
        private float clusterDensity = 0.5f;

        private bool includeGrass = false;
        private float grassDensity = 1.0f;
        private bool includeShrubs = true;
        private bool includeTrees = true;
        private float treeMinDistance = 5f;
        private float shrubMinDistance = 2f;

        private List<GameObject> selectedShrubPrefabs = new();
        private List<GameObject> selectedTreePrefabs = new();
        private List<GameObject> selectedGrassPrefabs = new();

        private System.Action<BiomeDefinition> onBiomeCreated;

        private Vector2 scrollPos;

        public static void Open(System.Action<BiomeDefinition> onCreateCallback)
        {
            var window = GetWindow<BiomeCreationWindow>(true, "Nový Biom");
            window.minSize = new Vector2(500, 700);
            window.onBiomeCreated = onCreateCallback;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayout.Label("Vytvořit nový biom", EditorStyles.boldLabel);

            biomeName = EditorGUILayout.TextField("Název biomu", biomeName);
            color = EditorGUILayout.ColorField("Barva", color);
            biomeMaterial = (Material)EditorGUILayout.ObjectField("Materiál", biomeMaterial, typeof(Material), false);
            heightMultiplier = EditorGUILayout.Slider("Výškový multiplikátor", heightMultiplier, 0f, 2f);
            noiseScale = EditorGUILayout.Slider("Škála šumu", noiseScale, 0f, 1f);

            GUILayout.Space(10);
            generateVegetation = EditorGUILayout.Toggle("Generovat vegetaci", generateVegetation);
            if (generateVegetation)
            {
                vegetationCoverage = EditorGUILayout.Slider("Pokrytí vegetací", vegetationCoverage, 0f, 1f);
                includeGrass = EditorGUILayout.Toggle("Tráva", includeGrass);
                if (includeGrass)
                    grassDensity = EditorGUILayout.Slider("Hustota trávy", grassDensity, 0f, 1f);

                includeShrubs = EditorGUILayout.Toggle("Keře", includeShrubs);
                shrubMinDistance = EditorGUILayout.FloatField("Rozestup keřů", shrubMinDistance);
                includeTrees = EditorGUILayout.Toggle("Stromy", includeTrees);
                treeMinDistance = EditorGUILayout.FloatField("Rozestup stromů", treeMinDistance);

                useClusters = EditorGUILayout.Toggle("Použít clustery", useClusters);
                if (useClusters)
                    clusterDensity = EditorGUILayout.Slider("Hustota clusterů", clusterDensity, 0f, 1f);

                GUILayout.Space(10);
                GUILayout.Label("Tráva", EditorStyles.boldLabel);
                DrawPrefabPicker(selectedGrassPrefabs);
                GUILayout.Label("Keře", EditorStyles.boldLabel);
                DrawPrefabPicker(selectedShrubPrefabs);
                GUILayout.Label("Stromy", EditorStyles.boldLabel);
                DrawPrefabPicker(selectedTreePrefabs);
            }

            GUILayout.Space(10);
            bool invalidName = string.IsNullOrWhiteSpace(biomeName) || biomeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
            bool nameExists = AssetDatabase.IsValidFolder($"Assets/Biomes/{biomeName}");

            if (invalidName)
                EditorGUILayout.HelpBox("Neplatný název biomu.", MessageType.Error);
            else if (nameExists)
                EditorGUILayout.HelpBox("Biom s tímto názvem již existuje.", MessageType.Error);

            EditorGUI.BeginDisabledGroup(invalidName || nameExists);
            if (GUILayout.Button("Vytvořit biom"))
            {
                CreateBiomeAsset();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();
        }

        private void DrawPrefabPicker(List<GameObject> prefabList)
        {
            int removeIndex = -1;
            for (int i = 0; i < prefabList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefabList[i] = (GameObject)EditorGUILayout.ObjectField(prefabList[i], typeof(GameObject), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                    removeIndex = i;
                EditorGUILayout.EndHorizontal();
            }
            if (removeIndex >= 0)
                prefabList.RemoveAt(removeIndex);

            if (GUILayout.Button("Přidat prefab"))
                prefabList.Add(null);
        }

        private void CreateBiomeAsset()
        {
            string biomeFolder = $"Assets/Biomes/{biomeName}";
            string vegetationFolder = Path.Combine(biomeFolder, "Vegetation");
            string shrubsFolder = Path.Combine(vegetationFolder, "Shrubs");
            string treesFolder = Path.Combine(vegetationFolder, "Trees");
            string grassesFolder = Path.Combine(vegetationFolder, "Grasses");

            Directory.CreateDirectory(biomeFolder);
            Directory.CreateDirectory(vegetationFolder);
            Directory.CreateDirectory(shrubsFolder);
            Directory.CreateDirectory(treesFolder);
            Directory.CreateDirectory(grassesFolder);

            BiomeDefinition asset = ScriptableObject.CreateInstance<BiomeDefinition>();
            asset.biomeName = biomeName;
            asset.color = color;
            asset.biomeMaterial = biomeMaterial;
            asset.heightMultiplier = heightMultiplier;
            asset.noiseScale = noiseScale;

            asset.generateVegetation = generateVegetation;
            asset.vegetationCoverage = vegetationCoverage;
            asset.includeGrass = includeGrass;
            asset.grassDensity = grassDensity;
            asset.includeShrubs = includeShrubs;
            asset.shrubMinDistance = shrubMinDistance;
            asset.includeTrees = includeTrees;
            asset.treeMinDistance = treeMinDistance;
            asset.useClusters = useClusters;
            asset.clusterDensity = clusterDensity;
            asset.vegetationPrefabsFolder = vegetationFolder;

            string assetPath = Path.Combine(biomeFolder, biomeName + "Definition.asset");
            AssetDatabase.CreateAsset(asset, assetPath);

            CopyPrefabsToFolder(selectedShrubPrefabs, shrubsFolder, "shrub_", out asset.shrubPrefabs);
            CopyPrefabsToFolder(selectedTreePrefabs, treesFolder, "tree_", out asset.treePrefabs);
            CopyPrefabsToFolder(selectedGrassPrefabs, grassesFolder, "grass_", out asset.grassPrefabs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
            onBiomeCreated?.Invoke(asset);
            Close();
        }

        private void CopyPrefabsToFolder(List<GameObject> prefabs, string targetFolder, string prefix, out GameObject[] resultArray)
        {
            Directory.CreateDirectory(targetFolder);
            List<GameObject> copied = new();
            foreach (var prefab in prefabs.Distinct())
            {
                if (prefab == null) continue;
                string sourcePath = AssetDatabase.GetAssetPath(prefab);
                string name = Path.GetFileName(sourcePath);
                string newName = prefix + name;
                string destPath = Path.Combine(targetFolder, newName);
                AssetDatabase.CopyAsset(sourcePath, destPath);
                GameObject loaded = AssetDatabase.LoadAssetAtPath<GameObject>(destPath);
                if (loaded != null) copied.Add(loaded);
            }
            resultArray = copied.ToArray();
        }
    }
}
