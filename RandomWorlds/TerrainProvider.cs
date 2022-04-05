using System.Collections.Generic;
using UnityEngine;
using WorldStreaming;

namespace RandomWorlds {
    public class TerrainProvider {
        private static TerrainProvider main;
        private readonly List<ITerrainApplicator>[][][] terrainApplicators;

        public static TerrainProvider GetInstance() {
            if (main is null) new TerrainProvider();
            return main;
        }

        private TerrainProvider() {
            
            main = this;
            var worldSize = WorldConfiguration.WORLD_SIZE_BATCHES;
            
            terrainApplicators = new List<ITerrainApplicator>[worldSize.x][][];
            for (int i = 0; i < worldSize.z; i++) {
                terrainApplicators[i] = new List<ITerrainApplicator>[worldSize.y][];
                for (int j = 0; j < worldSize.y; j++) {
                    terrainApplicators[i][j] = new List<ITerrainApplicator>[worldSize.z];
                }
            }
        }

        public void SubscribeApplicator(ITerrainApplicator newApplicator) {
            var bounds = newApplicator.GetBatchBounds();
            foreach (Int3 v in bounds) {
                if (terrainApplicators[v.x][v.y][v.z] is null) {
                    terrainApplicators[v.x][v.y][v.z] = new List<ITerrainApplicator>();
                }
                terrainApplicators[v.x][v.y][v.z].Add(newApplicator);
            }
        }

        // (Raster override pipeline) This method provides raster workspace in place of the rasterized octrees read from file
        public void ProvideRasterWorkspace(Voxeland.RasterWorkspace ws, Int3 voxelWorldOrigin, int downsamples) {
            var batchId = voxelWorldOrigin / 160;
            var batchApplicators = main.terrainApplicators[batchId.x][batchId.y][batchId.z];
            
            if (batchApplicators is null) return;
            
            foreach (Int3 v in Int3.Range(ws.size)) {
                var voxelPos = (voxelWorldOrigin + voxelWorldOrigin * (1 << downsamples)).ToVector3();
                var voxel = new Voxel(voxelPos);
                batchApplicators.ForEach(mod => mod.Apply(voxel));
                ws.typesGrid[v.x, v.y, v.z] = voxel.solidType;
                ws.densityGrid[v.x, v.y, v.z] = VoxelandData.OctNode.EncodeDensity(voxel.signedDistance);
            }
        }

        // (Octree override pipeline) This method is a mask for the call of ProvideOctree 
        public bool CanProvideBatch(Int3 batchId) {
            return false;
        }

        // (Octree override pipeline) This method fills octee with voxel data, replaces reading an octree from file
        public void ProvideOctree(Octree octree, UWE.LinearArrayHeap<byte> allocator, Int3 batchId, Int3 localOctreeIndex) {

        }
    }
}
