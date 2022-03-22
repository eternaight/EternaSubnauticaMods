using System.Reflection;
using ECCLibrary;
using HarmonyLib;
using QModManager.API.ModLoading;
using UnityEngine;
using Logger = QModManager.Utility.Logger;

namespace EternalCreatureTest {
    [QModCore]
    public class QMod
    {
        [QModPatch]
        public static void Patch() {
            var assembly = Assembly.GetExecutingAssembly();

            var creatureBundle = ECCHelpers.LoadAssetBundleFromAssetsFolder(assembly, "eternalCompanion.bundle");
            var companion = new EternalCompanion("EternalCompanion", "Eternal Companion", "your gf", creatureBundle.LoadAsset<GameObject>("eternal companion"), creatureBundle.LoadAsset<Texture2D>("eternal companion grad"));
            companion.Patch();

            var modName = ($"eternalight_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {modName}");
            Harmony harmony = new Harmony(modName);
            harmony.PatchAll();

            Logger.Log(Logger.Level.Info, "Patched successfully!");
        }
    }
}
