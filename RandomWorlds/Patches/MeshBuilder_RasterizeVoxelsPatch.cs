using HarmonyLib;
using WorldStreaming;

namespace RandomWorlds.Patches {

    [HarmonyPatch(typeof(MeshBuilder))]
    [HarmonyPatch("IVoxeland.RasterizeVoxels")]
    class MeshBuilder_RasterizeVoxelsPatch {
        
        [HarmonyPostfix]
        public static void Postfix(Voxeland.RasterWorkspace ws,
                                  int wx0,
                                  int wy0,
                                  int wz0,
                                  int downsampleLevels) {
            TerrainProvider.GetInstance().ProvideRasterWorkspace(ws, new Int3(wx0, wy0, wz0), downsampleLevels);
        }
    }
}
