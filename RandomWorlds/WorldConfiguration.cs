using UnityEngine;

namespace RandomWorlds {
    public static class WorldConfiguration {
        public static readonly Int3.Bounds WORLD_BOUNDS = new Int3.Bounds(Int3.zero, new Int3(25, 19, 25));
        public static readonly Vector3 ORIGIN_VOXEL = new Vector3(2048, 3040, 2048);
        public static readonly Int3 WORLD_SIZE_BATCHES = new Int3(26, 20, 26);
        public static readonly Int3 WORLD_SIZE_VOXELS = new Int3(4096, 3200, 4096);
    }
}
