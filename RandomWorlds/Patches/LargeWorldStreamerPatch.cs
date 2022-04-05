using HarmonyLib;

namespace RandomWorlds.Patches {

    [HarmonyPatch(typeof(LargeWorldStreamer), nameof(LargeWorldStreamer.FinalizeLoadBatchObjectsAsync))]
    class LargeWorldStreamer_FinalizeLoadBatchObjectsAsyncPatch {
        [HarmonyPrefix]
        public static void Prefix() {
            EntityProvider.EnterBatchObjectsLoad();
        }
    }
}
