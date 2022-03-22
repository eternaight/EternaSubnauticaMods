using RandomWorlds.OctreeGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomWorlds {
    public static class WorldSerializer {

        private const int treesPerBatch = 5;
        private static Int3 nodes = new Int3(128, 100, 128);

        public static void SerializeOctrees(WorldOctrees octrees) {
            foreach (Int3 batchId in Int3.Range(Int3.FloorDiv(nodes, 5))) {

                if (BatchWorth(batchId)) {
                    var path = Path.Combine(WorldManager.GetCompiledOctreesPath(), $"compiled-batch-{batchId.x}-{batchId.y}-{batchId.z}.optoctrees");
                    using (BinaryWriter w = new BinaryWriter(FileUtils.CreateFile(path))) {
                        w.Write(4);
                        foreach (Int3 ocid in Int3.Range(5)) {
                            octrees.WriteSerializedOctree(w, batchId * 5 + ocid);
                        }
                    }
                }
            }
        }

        private static bool CheckRoot(Int3 id) {
            return id.x >= 0 && id.y >= 0 && id.z >= 0 && id.x < nodes.x && id.y < nodes.y && id.z < nodes.z;
        }

        private static bool BatchWorth(Int3 id) {
            return WorldManager.batchBounds.Contains(id);
        }

        private static void WriteBoringOctree(BinaryWriter w, bool solid) {
            w.Write(Convert.ToUInt16(1));
            w.Write(Convert.ToByte(solid ? 20 : 0));
            w.Write(Convert.ToByte(0));
            w.Write(Convert.ToUInt16(0));
        }
    }
}
