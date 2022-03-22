using System.Collections.Generic;
using System.IO;

namespace RandomWorlds.OctreeGen {
    public class EternalOctree {
        private readonly SortedList<int, EternalOctreeNode> nodes;
        private Int3 _globalOctreeId;

        public EternalOctree(Int3 globalOctreeId) {
            _globalOctreeId = globalOctreeId;
            nodes = new SortedList<int, EternalOctreeNode>();
            nodes.Add(1, new EternalOctreeNode());
        }

        public void WriteCompiled(BinaryWriter w) {
            w.Write((ushort)nodes.Count);

            foreach (EternalOctreeNode node in nodes.Values) {
                w.Write(node.type);
                w.Write(node.density);
                w.Write((ushort)nodes.IndexOfKey(node.firstChildHash));
            }
        }

        public void ApplyVoxelGrid(IVoxelGrid grid) {
            Int3 voxelStart = _globalOctreeId * 32;
            ApplyBottomUp(grid, voxelStart.x, voxelStart.y, voxelStart.z, 16, 1);
        }
        private bool ApplyBottomUp(IVoxelGrid grid, int x, int y, int z, int halfside, int nodeHash) {
            var node = nodes[nodeHash];
            if (halfside == 0) {
                // it's a leaf
                if (grid.GetVoxelMask(x, y, z)) {
                    var octNode = grid.GetVoxel(x, y, z);
                    node.type = octNode.type;
                    node.density = octNode.density;
                }
                return true;
            }

            // if no children, acquire them
            if (node.firstChildHash == 0) {
                AcquireChildren(nodeHash, node);
            }
            // apply bottom up to children
            bool childrenFlat = true;
            bool childrenSame = true;
            EternalOctreeNode firstChild = nodes[node.firstChildHash];
            for (int b = 0; b < 8; b++) {
                childrenFlat &= ApplyBottomUp(grid, x + ChildDX[b] * halfside, y + ChildDY[b] * halfside, z + ChildDZ[b] * halfside, halfside/2, node.firstChildHash + b);
                var childNode = nodes[node.firstChildHash + b];
                if (firstChild.type != childNode.type || firstChild.density != childNode.density) {
                    childrenSame = false;
                }
            }
            // if children are identical, prune
            if (childrenFlat && childrenSame) {
                node.type = firstChild.type;
                node.density = firstChild.density;
                PruneChildren(node);
            }

            return childrenFlat && childrenSame;
        }

        private void AcquireChildren(int nodeHash, EternalOctreeNode node) {
            node.firstChildHash = nodeHash * 8;
            for (int b = 0; b < 8; b++) {
                nodes.Add(node.firstChildHash + b, new EternalOctreeNode());
            }
        }
        private void PruneChildren(EternalOctreeNode node) {
            for (int b = 0; b < 8; b++) {
                nodes.Remove(node.firstChildHash + b);
            }
            node.firstChildHash = 0;
        }

        private static int HeightOfHash(int nodeHash) {
            if (nodeHash == 1) return 0;
            if (nodeHash < 0x10) return 1;      //10_000 = 0x10
            if (nodeHash < 0x80) return 2;      //10_000_000 = 0x80
            if (nodeHash < 0x400) return 3;     //10_000_000_000 = 0x400
            if (nodeHash < 0x2000) return 4;    //10_000_000_000_000 = 0x2000
            if (nodeHash < 0x10000) return 5;   //10_000_000_000_000_000 = 0x10000

            throw new System.ArgumentOutOfRangeException("nodeHash");
        }
        

        private class EternalOctreeNode {
            public byte type;
            public byte density;
            public int firstChildHash;
        }

        private static readonly int[] ChildDX = new int[8] { 0, 0, 0, 0, 1, 1, 1, 1 };
        private static readonly int[] ChildDY = new int[8] { 0, 0, 1, 1, 0, 0, 1, 1 };
        private static readonly int[] ChildDZ = new int[8] { 0, 1, 0, 1, 0, 1, 0, 1 };
    }
}
