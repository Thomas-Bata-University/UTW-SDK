namespace Terrain
{
    using UnityEngine;

    public enum BiomeType
    {
        Desert,
        Forest,
        Mountains,
        Plains,
        Tundra,
    }
    
    public enum BiomeAlgorithm { None, Voronoi, Worley, FloodFill }
    

    [CreateAssetMenu(menuName = "Procedural Terrain/Biome Definition")]
    public class BiomeDefinition : ScriptableObject
    {
        [Tooltip("Srozumitelný název biomu")]
        public string biomeName;
    
        [Tooltip("Kategorie biomu (enum) - volitelný výčtový typ pro použití v kódu")]
        private BiomeType biomeType;
    
        [Tooltip("Barva spojená s biome (použito pro zobrazení terénu, např. vertex color nebo debug)")]
        public Color color = Color.gray;
        
        public Material biomeMaterial;
    
        [Tooltip("Multiplikátor výšky pro tento biom (ovlivňuje relativní výšky terénu v oblasti biomu)")]
        [Range(0f, 2f)]
        public float heightMultiplier = 1f;

        [Tooltip("Frekvence šumu pro tento biom (ovlivňuje detaily terénu)")]
        [Range(0f, 1f)]
        public float noiseScale = 0.1f;

        [Tooltip("Generovat vegetaci v tomto biomu")]
        public bool generateVegetation = false;
    
        [Tooltip("Celkové pokrytí plochy biomu vegetací (0–1)")]
        [Range(0f, 1f)]
        public float vegetationCoverage = 1.0f;
        [Tooltip("použití shluků vegetace")]
        public bool useClusters;
        [Tooltip("Hustota pokrytí shluky, vyšší hodnota znamená znamená hustší a blíže rozmístěné shluky (0-1)")]
        [Range(0f, 1f)]
        public float clusterDensity = 0.5f;
    
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
        [Range(0f, 1f)]
        public float grassDensity = 1.0f;
    
        [Tooltip("Cesta ke složce s prefaby vegetace pro tento biom")]
        public string vegetationPrefabsFolder = "Assets/Vegetation";
        
        [Tooltip("Generovat města v tomto biomu")]
        public bool allowCityGeneration = false;

        [Tooltip("Cesta ke složce s budovami pro města v tomto biomu")]
        public string cityPrefabsFolder = "";

        [Tooltip("Maximální velikost města (počet buněk)")]
        public int maxCitySize = 10;
        
        [HideInInspector]public GameObject[] grassPrefabs;
        [HideInInspector]public GameObject[] shrubPrefabs;
        [HideInInspector]public GameObject[] treePrefabs;
        [HideInInspector]public GameObject[] cityPrefabs;

    }

}