using HarmonyLib;

namespace FringingReefEditor {
    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    class Player_AwakePatch {
        [HarmonyPostfix]
        public static void Postfix(Player __instance) {
            __instance.gameObject.AddComponent<EditorRelay>();
        }
    }
}
