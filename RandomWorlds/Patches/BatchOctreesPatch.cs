using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using WorldStreaming;

namespace RandomWorlds.Patches {
    
    [HarmonyPatch(typeof(BatchOctrees), nameof(BatchOctrees.LoadOctrees))]
    class BatchOctreesPatch {
        [HarmonyPrefix]
        public static bool Prefix(BatchOctrees __instance, ref bool __result) {
            if (WorldManager.OverrideBatchExists(__instance.id)) {

                Int3 int3 = __instance.id * __instance.arraySize;
                foreach (Int3 p in Int3.Range(__instance.arraySize)) {
                    if (__instance.streamer.octreeBounds.Contains(p + int3)) {
                        Octree octree = __instance.octrees.Get(p);


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
