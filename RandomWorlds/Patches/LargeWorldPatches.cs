using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace RandomWorlds.Patches {
    [HarmonyPatch(typeof(LargeWorld), nameof(LargeWorld.InitializeBiomeMap))]
    class LargeWorld_InitializeBiomeMapPatch {
        [HarmonyPrefix]
        public static bool Prefix(LargeWorld __instance) {
            __instance.biomeMapLegend = LargeWorld.LoadBiomeMapLegend(__instance.legendColorsPath, __instance.biomesCSVPath);
            __instance.biomeMap = SurfaceBiomeProvider.GetInstance().GetBiomemap(__instance.biomeMapLegend, out __instance.biomeMapWidth, out __instance.biomeMapHeight);
            __instance.biomeDownFactor = __instance.land.data.sizeX / __instance.biomeMapWidth;
            Debug.LogFormat("biome map downsample factor: {0}", __instance.biomeDownFactor);
            return false;
        }
    }

    [HarmonyPatch(typeof(LargeWorld), nameof(LargeWorld.LoadBiomeMapLegend))]
    class LargeWorld_LoadBiomeMapLegendPatch {
        [HarmonyPostfix]
        public static void Postfix(Dictionary<Int3, BiomeProperties> __result) {
            SurfaceBiomeProvider.GetInstance().ModifyBiomemapLegend(__result);
        }
    }
}
