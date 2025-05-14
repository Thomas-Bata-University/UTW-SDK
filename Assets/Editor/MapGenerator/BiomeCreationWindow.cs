using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Terrain;  // for BiomeDefinition and BiomeType

namespace Editor.MapGenerator
{
    /// <summary>
    /// Editor window for creating a new BiomeDefinition with specified properties and vegetation.
    /// </summary>
    public class BiomeCreationWindow : EditorWindow
    {
        // Biome properties inputs
        private string biomeName = "NewBiome";
        private BiomeType biomeType = BiomeType.Plains;
        private Color color = Color.gray;
        private Material biomeMaterial;
        private float heightMultiplier = 1f;
        private float noiseScale = 0.1f;

        // Vegetation options
        private bool generateVegetation = false;
        private float vegetationCoverage = 1.0f;
        private bool includeShrubs = true;
        private bool includeTrees = true;
        private float treeMinDistance = 5f;
        private float shrubMinDistance = 2f;
        private float grassDensity = 1.0f;  // grass options remain for compatibility but not exposed
        // Note: includeGrass is omitted from GUI (always false for new biomes to disable grass generation)

        // Lists for selected vegetation prefabs (only shrubs and trees are used in GUI)
        private List<GameObject> selectedShrubPrefabs = new();
        private List<GameObject> selectedTreePrefabs = new();

        // Callback to notify when a new biome is created
        private System.Action<BiomeDefinition> onBiomeCreated;

        /// <summary>
        /// Opens the BiomeCreationWindow as a popup.
        /// </summary>
        public static void Open(System.Action<BiomeDefinition> onCreateCallback)
        {
            var window = GetWindow<BiomeCreationWindow>(true, "Nový Biom");
            window.minSize = new Vector2(400, 600);
            window.onBiomeCreated = onCreateCallback;
            window.ShowUtility();  // Show as a utility window (modal-like behavior)
        }

        private void OnGUI()
        {
            // Window title
            GUILayout.Label("Vytvořit nový biom", EditorStyles.boldLabel);

            // Biome basic properties
            biomeName = EditorGUILayout.TextField("Název biomu", biomeName);
            biomeType = (BiomeType)EditorGUILayout.EnumPopup("Typ biomu", biomeType);
            color = EditorGUILayout.ColorField("Barva", color);
            biomeMaterial = (Material)EditorGUILayout.ObjectField("Materiál", biomeMaterial, typeof(Material), false);
            heightMultiplier = EditorGUILayout.Slider("Výškový multiplikátor", heightMultiplier, 0.5f, 2f);
            noiseScale = EditorGUILayout.FloatField("Škála šumu", noiseScale);

            GUILayout.Space(10);
            // Vegetation generation toggle
            generateVegetation = EditorGUILayout.Toggle("Generovat vegetaci", generateVegetation);
            if (generateVegetation)
            {
                // Vegetation parameters (coverage and toggles for shrubs/trees)
                vegetationCoverage = EditorGUILayout.Slider("Pokrytí vegetací", vegetationCoverage, 0f, 1f);
                includeShrubs = EditorGUILayout.Toggle("Keře", includeShrubs);
                includeTrees = EditorGUILayout.Toggle("Stromy", includeTrees);
                treeMinDistance = EditorGUILayout.FloatField("Rozestup stromů", treeMinDistance);
                shrubMinDistance = EditorGUILayout.FloatField("Rozestup keřů", shrubMinDistance);
                // Note: Grass options are not shown in GUI
            }

            // Vegetation prefab selection (only if vegetation generation is enabled)
            if (generateVegetation)
            {
                GUILayout.Space(10);
                GUILayout.Label("Vyberte prefaby vegetace", EditorStyles.boldLabel);
                // Shrub prefabs
                GUILayout.Label("Keře", EditorStyles.miniBoldLabel);
                DrawPrefabPicker(selectedShrubPrefabs);
                // Tree prefabs
                GUILayout.Label("Stromy", EditorStyles.miniBoldLabel);
                DrawPrefabPicker(selectedTreePrefabs);
            }

            GUILayout.Space(10);
            // Show warnings for invalid input or duplicate names
            bool invalidName = string.IsNullOrWhiteSpace(biomeName) ||
                                biomeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                                biomeName.IndexOfAny(new char[]{'/', '\\'}) >= 0;
            bool nameExists = AssetDatabase.IsValidFolder($"Assets/Biomes/{biomeName}");
            if (invalidName)
            {
                EditorGUILayout.HelpBox("Zadejte platný název biomu (nesmí být prázdný ani obsahovat neplatné znaky).", MessageType.Error);
            }
            else if (nameExists)
            {
                EditorGUILayout.HelpBox($"Biom s názvem \"{biomeName}\" již existuje. Zvolte jiný název.", MessageType.Error);
            }

            // Create button (disabled if name is invalid or already exists)
            EditorGUI.BeginDisabledGroup(invalidName || nameExists);
            if (GUILayout.Button("Vytvořit biom"))
            {
                CreateBiomeAsset();
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Helper method to draw a list UI for selecting multiple prefab assets (with drag & drop support).
        /// </summary>
        private void DrawPrefabPicker(List<GameObject> prefabList)
        {
            int removeIndex = -1;
            // Display each selected prefab with a remove button
            for (int i = 0; i < prefabList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                prefabList[i] = (GameObject)EditorGUILayout.ObjectField(prefabList[i], typeof(GameObject), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (removeIndex >= 0)
            {
                prefabList.RemoveAt(removeIndex);
            }

            // Button to add a new empty slot for selecting a prefab
            if (GUILayout.Button("Přidat prefab"))
            {
                prefabList.Add(null);
            }

            // Drag-and-drop area for prefabs
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 30.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Přetáhni prefab(y) sem", EditorStyles.helpBox);
            if (dropArea.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is GameObject go)
                            {
                                prefabList.Add(go);
                            }
                        }
                    }
                    Event.current.Use();
                }
            }
        }

        /// <summary>
        /// Creates the BiomeDefinition asset and associated folders, copies selected prefabs, and invokes the callback.
        /// </summary>
        private void CreateBiomeAsset()
        {
            // Prepare folder paths for the new biome
            string biomeFolder = $"Assets/Biomes/{biomeName}";
            string textureFolder = Path.Combine(biomeFolder, "Textures");
            string vegetationFolder = Path.Combine(biomeFolder, "Vegetation");
            string shrubsFolder = Path.Combine(vegetationFolder, "Shrubs");
            string treesFolder = Path.Combine(vegetationFolder, "Trees");

            // Create directories for the biome (if they don't already exist)
            Directory.CreateDirectory(biomeFolder);
            Directory.CreateDirectory(textureFolder);
            Directory.CreateDirectory(vegetationFolder);
            Directory.CreateDirectory(shrubsFolder);
            Directory.CreateDirectory(treesFolder);

            // Create a new BiomeDefinition asset instance and assign properties
            BiomeDefinition asset = ScriptableObject.CreateInstance<BiomeDefinition>();
            asset.biomeName       = biomeName;
            asset.biomeType       = biomeType;
            asset.color           = color;
            asset.biomeMaterial   = biomeMaterial;
            asset.heightMultiplier = heightMultiplier;
            asset.noiseScale      = noiseScale;
            asset.generateVegetation = generateVegetation;
            asset.vegetationCoverage = vegetationCoverage;
            asset.includeGrass    = false;       // Grass disabled in new biome by default
            asset.includeShrubs   = includeShrubs;
            asset.includeTrees    = includeTrees;
            asset.treeMinDistance = treeMinDistance;
            asset.shrubMinDistance = shrubMinDistance;
            asset.grassDensity    = grassDensity;
            asset.vegetationPrefabsFolder = vegetationFolder;  // root folder for this biome's vegetation

            // Save the BiomeDefinition as an asset file
            string assetPath = $"{biomeFolder}/{biomeName}Definition.asset";
            AssetDatabase.CreateAsset(asset, assetPath);

            // Copy selected vegetation prefabs into the biome's Vegetation subfolders
            CopyPrefabsToFolder(selectedShrubPrefabs, shrubsFolder);
            CopyPrefabsToFolder(selectedTreePrefabs, treesFolder);
            // (Grass prefabs are not processed since grass is not supported in GUI)

            // Finalize asset database changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the new asset in the Editor (so user can see it in Inspector)
            Selection.activeObject = asset;
            // Invoke callback to notify the main window
            onBiomeCreated?.Invoke(asset);
            // Close this popup window
            Close();
        }

        /// <summary>
        /// Copies a list of prefab assets into the target folder, prefixing filenames by category.
        /// </summary>
        private void CopyPrefabsToFolder(List<GameObject> prefabs, string targetFolder)
        {
            if (prefabs == null) return;
            // Ensure target folder exists
            Directory.CreateDirectory(targetFolder);
            // Determine a prefix based on the folder name (e.g., "shrubs" or "trees")
            string prefix = Path.GetFileName(targetFolder).ToLower();
            // Copy each unique prefab asset to the target folder
            foreach (GameObject prefab in prefabs.Distinct())
            {
                if (prefab == null) continue;
                string sourcePath = AssetDatabase.GetAssetPath(prefab);
                string originalName = Path.GetFileNameWithoutExtension(sourcePath);
                string fileName = $"{prefix}_{originalName}.prefab";
                string destPath = Path.Combine(targetFolder, fileName);
                // Copy the asset file to the new location (will not overwrite existing files)
                AssetDatabase.CopyAsset(sourcePath, destPath);
            }
        }
    }
}
