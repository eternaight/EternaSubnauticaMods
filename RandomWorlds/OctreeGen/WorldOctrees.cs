using System.IO;
using UnityEngine;

namespace RandomWorlds.OctreeGen {
    public class WorldOctrees {
        private Array3<EternalOctree> octrees;

        private Int3 size = new Int3(128, 100, 128);
        private IVoxelGrid grid;

        public WorldOctrees(IVoxelGrid voxelGrid) {
            grid = voxelGrid;
            octrees = new Array3<EternalOctree>(size.x, size.y, size.z);
            foreach (Int3 id in Int3.Range(size)) {
                octrees.Set(id, new EternalOctree(id));
            }
        }

        public void WriteSerializedOctree(BinaryWriter w, Int3 index) {
            var octree = octrees.Get(index);
            octree.ApplyVoxelGrid(grid);
            octree.WriteCompiled(w);
        }
    }
}
