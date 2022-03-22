using HarmonyLib;
using UnityEngine;

namespace RandomWorlds.MiscPatches {
    [HarmonyPatch(typeof(RandomStart), nameof(RandomStart.GetRandomStartPoint))]
    class RandomStartPatch {
        [HarmonyPrefix]
        public static bool Prefix(ref Vector3 __result) {
            __result = WorldManager.GetStartPoint();

            return false;
        }
    }
}
