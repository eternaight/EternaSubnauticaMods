using HarmonyLib;

namespace RandomWorlds.Patches {

#if RUNTIME_GENERATION
    [HarmonyPatch(typeof(LargeWorldStreamer), nameof(LargeWorldStreamer.FinalizeLoadBatchObjectsAsync))]
    class LargeWorldStreamerPatch {
        [HarmonyPrefix]
        public static void Prefix() {
            EntitySpawnManager.EnterBatchObjectsLoad();
        }
    }
#endif
}
