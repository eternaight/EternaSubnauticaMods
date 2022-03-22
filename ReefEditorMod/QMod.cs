using System.Reflection;
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

            var modName = ($"eternalight_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {modName}");
            Harmony harmony = new Harmony(modName);
            harmony.PatchAll(assembly);
            Logger.Log(Logger.Level.Info, "Patched successfully!");
        }
    }
}
