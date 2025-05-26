using UnityEngine;
using UnityEditor;
using Terrain;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Editor.MapGenerator
{
    public class BiomeEditWindow : EditorWindow
    {
        private BiomeDefinition selectedBiome;
        private UnityEditor.Editor editor;

        private Vector2 scrollPos;
        private GameObject newTreePrefab;
        private GameObject newShrubPrefab;
        private GameObject newGrassPrefab;

        public static void Open()
        {
            var window = GetWindow<BiomeEditWindow>(false, "Upravit Biom");
            window.minSize = new Vector2(500, 700);
            window.maxSize = new Vector2(800, 1200);
            window.wantsMouseMove = true;
            window.Show();
            window.Focus();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            try
            {
                EditorGUILayout.LabelField("Vyber biom k úpravě:", EditorStyles.boldLabel);
                BiomeDefinition newBiome = (BiomeDefinition)EditorGUILayout.ObjectField(selectedBiome, typeof(BiomeDefinition), false);

                if (newBiome != selectedBiome)
                {
                    selectedBiome = newBiome;
                    if (selectedBiome != null)
                    {
                        if (editor != null)
                            DestroyImmediate(editor);
                        editor = UnityEditor.Editor.CreateEditor(selectedBiome);
                        editor?.OnInspectorGUI();
                    }
                }

                if (selectedBiome == null)
                {
                    EditorGUILayout.HelpBox("Vyberte biom, který chcete upravit.", MessageType.Info);
                    return;
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Úprava: " + selectedBiome.biomeName, EditorStyles.boldLabel);

                editor?.OnInspectorGUI();

                EditorGUILayout.Space(10);
                DrawPrefabSection("Stromy", ref newTreePrefab, "Trees", ref selectedBiome.treePrefabs, "tree_");
                DrawPrefabSection("Keře", ref newShrubPrefab, "Shrubs", ref selectedBiome.shrubPrefabs, "shrub_");
                DrawPrefabSection("Tráva", ref newGrassPrefab, "Grasses", ref selectedBiome.grassPrefabs, "grass_");

                EditorGUILayout.Space(10);
                if (GUILayout.Button("Uložit změny"))
                {
                    EditorUtility.SetDirty(selectedBiome);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.Space();
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Smazat biom"))
                {
                    if (EditorUtility.DisplayDialog("Potvrdit smazání", "Opravdu chcete smazat biom '" + selectedBiome.biomeName + "'?", "Smazat", "Zrušit"))
                    {
                        DeleteBiome(selectedBiome);
                        selectedBiome = null;
                        if (editor != null)
                        {
                            DestroyImmediate(editor);
                            editor = null;
                        }
                        GUIUtility.ExitGUI();
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawPrefabSection(string label, ref GameObject newPrefab, string subfolder, ref GameObject[] targetArray, string namePrefix)
        {
            EditorGUILayout.LabelField(label + " (ve složce)", EditorStyles.boldLabel);

            string fullPath = Path.Combine(selectedBiome.vegetationPrefabsFolder, subfolder);
            Directory.CreateDirectory(fullPath);

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { fullPath });
            var prefabs = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
                .Where(go => go != null)
                .ToList();

            for (int i = 0; i < prefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);
                if (GUILayout.Button("Smazat", GUILayout.Width(60)))
                {
                    string path = AssetDatabase.GetAssetPath(prefabs[i]);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.Refresh();
                    targetArray = RefreshPrefabList(fullPath);
                    EditorUtility.SetDirty(selectedBiome);
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            newPrefab = (GameObject)EditorGUILayout.ObjectField("Přidat nový prefab", newPrefab, typeof(GameObject), false);
            if (newPrefab != null && GUILayout.Button("Přidat do složky"))
            {
                string sourcePath = AssetDatabase.GetAssetPath(newPrefab);
                string fileName = Path.GetFileName(sourcePath);
                string renamedFile = namePrefix + fileName;
                string destPath = Path.Combine(fullPath, renamedFile);

                if (!File.Exists(destPath))
                {
                    AssetDatabase.CopyAsset(sourcePath, destPath);
                    AssetDatabase.Refresh();
                    targetArray = RefreshPrefabList(fullPath);
                    EditorUtility.SetDirty(selectedBiome);
                }
                newPrefab = null;
            }
        }

        private GameObject[] RefreshPrefabList(string folder)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            var prefabs = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
                .Where(go => go != null)
                .ToArray();
            return prefabs;
        }

        private void DeleteBiome(BiomeDefinition biome)
        {
            string path = AssetDatabase.GetAssetPath(biome);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            if (!string.IsNullOrEmpty(biome.vegetationPrefabsFolder) && Directory.Exists(biome.vegetationPrefabsFolder))
            {
                string rootFolder = Path.GetDirectoryName(biome.vegetationPrefabsFolder);
                if (!string.IsNullOrEmpty(rootFolder) && Directory.Exists(rootFolder))
                {
                    FileUtil.DeleteFileOrDirectory(rootFolder);
                    FileUtil.DeleteFileOrDirectory(rootFolder + ".meta");
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
