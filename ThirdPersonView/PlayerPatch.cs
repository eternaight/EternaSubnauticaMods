using HarmonyLib;

namespace ThirdPersonView {
    [HarmonyPatch(typeof(Player), nameof(Player.SetScubaMaskActive))]
    class Player_SetScubaMaskActivePatch {
        [HarmonyPrefix]
        public static bool Prefix(Player __instance) {
            var comp = __instance.GetComponentInChildren<ThirdPersonCameraControl>();
            return comp is null;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    class Player_UpdatePatch {

        [HarmonyPostfix]
        public static void Postfix(Player __instance) {
            ThirdPersonCameraControl.InsideTightSpace = (__instance.IsInBase() || __instance.IsInSubmarine());
        }
    }
}
