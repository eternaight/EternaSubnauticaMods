using HarmonyLib;
using UnityEngine;

namespace RandomWorlds.Patches {
    [HarmonyPatch(typeof(LargeWorld))]
    [HarmonyPatch("InitializeBiomeMap")]
    class LargeWorldBiomeMapPatch {
        [HarmonyPrefix]
        public static bool Prefix(LargeWorld __instance) {
            __instance.biomeMapLegend = LargeWorld.LoadBiomeMapLegend(__instance.legendColorsPath, __instance.biomesCSVPath);
            __instance.biomeMap = WorldManager.generator.GetBiomemap(__instance.biomeMapLegend, out __instance.biomeMapWidth, out __instance.biomeMapHeight);
            __instance.biomeDownFactor = __instance.land.data.sizeX / __instance.biomeMapWidth;
            Debug.LogFormat("biome map downsample factor: {0}", __instance.biomeDownFactor);
            return false;
        }
    }
}
