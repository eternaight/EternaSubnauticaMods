using HarmonyLib;
using System.Collections.Generic;
using QModManager.Utility;
using WorldStreaming;
using UnityEngine;

namespace RandomWorlds.Patches {

#if RUNTIME_GENERATION
    [HarmonyPatch(typeof(MeshBuilder))]
    [HarmonyPatch("IVoxeland.RasterizeVoxels")]

    class MeshBuilderPatch {
        
        [HarmonyPostfix]
        public static void Postfix(Voxeland.RasterWorkspace ws,
                                  int wx0,
                                  int wy0,
                                  int wz0,
                                  int downsampleLevels) {

            Int3 voxelWorldOrigin = new Int3(wx0, wy0, wz0);
            WorldManager.generator.FillRasterWorkspace(ws, voxelWorldOrigin, downsampleLevels);
        }
    }
#endif
}
