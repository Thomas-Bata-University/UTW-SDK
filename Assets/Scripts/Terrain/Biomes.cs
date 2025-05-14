namespace Terrain
{
    using UnityEngine;
    using System.Collections.Generic;

    public enum BiomeType
    {
        Desert,    // Poušť
        Forest,    // Les
        Mountains, // Hory
        Plains,    // Planiny
        Tundra,    // Tundra
        // ... další typy biomů lze přidat zde
    }
    
    public enum BiomeAlgorithm { None, Voronoi, Worley }
    

    [CreateAssetMenu(menuName = "Procedural Terrain/Biome Definition")]
    public class BiomeDefinition : ScriptableObject
    {
        [Tooltip("Srozumitelný název biomu")]
        public string biomeName;
    
        [Tooltip("Kategorie biomu (enum) - volitelný výčtový typ pro použití v kódu")]
        public BiomeType biomeType;
    
        [Tooltip("Barva spojená s biome (použito pro zobrazení terénu, např. vertex color nebo debug)")]
        public Color color = Color.gray;
        
        public Material biomeMaterial;
    
        [Tooltip("Multiplikátor výšky pro tento biom (ovlivňuje relativní výšky terénu v oblasti biomu)")]
        public float heightMultiplier = 1f;

        [Tooltip("Frekvence šumu pro tento biom (ovlivňuje detaily terénu)")]
        public float noiseScale = 0.1f;

        [Tooltip("Generovat vegetaci v tomto biomu")]
        public bool generateVegetation = false;
    
        [Tooltip("Celkové pokrytí plochy biomu vegetací (0–1)")]
        public float vegetationCoverage = 1.0f;
    
        [Tooltip("Zahrnout trávu")]
        public bool includeGrass = true;
        [Tooltip("Zahrnout keře")]
        public bool includeShrubs = true;
        [Tooltip("Zahrnout stromy")]
        public bool includeTrees = true;
    
        [Tooltip("Minimální rozestup mezi stromy")]
        public float treeMinDistance = 5f;
        [Tooltip("Minimální rozestup mezi keři")]
        public float shrubMinDistance = 2f;
    
        [Tooltip("Hustota trávy (trsy na jednotku plochy při plném pokrytí)")]
        public float grassDensity = 1.0f;
    
        [Tooltip("Cesta ke složce s prefaby vegetace pro tento biom")]
        public string vegetationPrefabsFolder = "Assets/Vegetation/BiomXYZ";
        
        public GameObject[] grassPrefabs;
        public GameObject[] shrubPrefabs;
        public GameObject[] treePrefabs;

        // V budoucnu lze přidat metody pro logiku biomu, ale primárně tento ScriptableObject slouží jako kontejner dat.
    }

}