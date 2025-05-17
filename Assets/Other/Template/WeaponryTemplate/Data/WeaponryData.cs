using UnityEngine;

namespace Other.Template.WeaponryTemplate.Data {
    public class WeaponryData : DefaultData {

        public bool useBarrel = true;
        public int barrelMass = 1000;
        public Mesh barrelMesh;
        public int barrelNumOfMat = 1;
        public Material[] barrelMaterials;
        public int barrelNumOfCol = 1;
        public Mesh[] barrelColliders;

        public bool useMantlet = false;
        public int mantletMass = 1000;
        public Mesh mantletMesh;
        public int mantletNumOfMat = 1;
        public Material[] mantletMaterials;
        public int mantletNumOfCol = 1;
        public Mesh[] mantletColliders;

    }
} //END