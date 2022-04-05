using HarmonyLib;
using WorldStreaming;

namespace RandomWorlds.Patches {
    
    class BatchOctrees_LoadOctreesPatch {
        public static bool Prefix(BatchOctrees __instance, ref bool __result) {

            var terrainProvider = TerrainProvider.GetInstance();
            
            if (terrainProvider.CanProvideBatch(__instance.id)) {

                Int3 globalOctreeStart = __instance.id * __instance.arraySize;
                foreach (Int3 localOctreeIndex in Int3.Range(__instance.arraySize)) {
                    if (__instance.streamer.octreeBounds.Contains(localOctreeIndex + globalOctreeStart)) {
                        Octree octree = __instance.octrees.Get(localOctreeIndex);
                        terrainProvider.ProvideOctree(octree, __instance.allocator, __instance.id, localOctreeIndex);
                        octree.UnloadChildren(__instance.streamer.minLod, __instance.allocator);
                    }
                }

                __result = true;
                return false;
            }

            return true;
        }
    }
}
