using System.Collections.Generic;

namespace Other.Template.HullTemplate.Data {
    public enum HullSize {
        SUPER_LIGHT,
        LIGHT,
        MEDIUM,
        HEAVY,
        SUPER_HEAVY
    }

    public class HullSizeInfo {
        public HullSize Size { get; }
        public string DisplayName { get; }
        public float LengthInMeters { get; }
        public string PrefabName { get; }

        public HullSizeInfo(HullSize size, string displayName, float length, string prefabName) {
            Size = size;
            DisplayName = displayName;
            LengthInMeters = length;
            PrefabName = prefabName;
        }

        public static readonly List<HullSizeInfo> All = new() {
            new HullSizeInfo(HullSize.SUPER_LIGHT,   "Superlight – 3m",     3f,  "SuperLight"),
            new HullSizeInfo(HullSize.LIGHT,         "Light – 4.5m",        4.5f,"Light"),
            new HullSizeInfo(HullSize.MEDIUM,        "Medium – 6.5m",       6.5f,"Medium"),
            new HullSizeInfo(HullSize.HEAVY,         "Heavy – 7.5m",        7.5f,"Heavy"),
            new HullSizeInfo(HullSize.SUPER_HEAVY,   "Superheavy – 10m",    10f, "SuperHeavy")
        };

        public static HullSizeInfo Get(HullSize size) {
            return All.Find(s => s.Size == size);
        }

        public static string[] GetDisplayNames() {
            return All.ConvertAll(s => s.DisplayName).ToArray();
        }
        
    }
}