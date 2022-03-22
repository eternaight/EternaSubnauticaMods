using HarmonyLib;
using QModManager.API.ModLoading;
using QModManager.Utility;
using SMLHelper.V2.Handlers;
using System.Reflection;

namespace ThirdPersonView
{
    [QModCore]
    public class ThirdPersonView
    {
        internal static ThirdPersonViewConfig Config { get; private set; }

        [QModPatch]
        public static void Patch() {
            var assembly = Assembly.GetExecutingAssembly();

            var modName = ($"eternalight_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {modName}");

            var harmony = new Harmony(modName);
            harmony.PatchAll();

            Config = OptionsPanelHandler.Main.RegisterModOptions<ThirdPersonViewConfig>();

            Logger.Log(Logger.Level.Info, "Patched successfully!");
        }
    }
}
