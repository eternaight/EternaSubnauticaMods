using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReefEditorMod {

    [HarmonyPatch(typeof(PlayerController), "Start")]
    class PlayerControllerPatcher {
        static void Postfix(PlayerController __instance) {
            __instance.activeController = __instance.underWaterController;
        }
    }

    [HarmonyPatch(typeof(Player), "IsUnderwaterForSwimming")]
    class PlayerPatcher {
        static bool Prefix(ref bool __result) => true;
    }
}
