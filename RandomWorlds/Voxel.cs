using UnityEngine;

namespace RandomWorlds {
    public class Voxel {
        public byte solidType;
        public float signedDistance;
        public Vector3 position;

        public Voxel(Vector3 _position) {
            solidType = 1;
            signedDistance = -1;
            position = _position;
        }
    }
}
