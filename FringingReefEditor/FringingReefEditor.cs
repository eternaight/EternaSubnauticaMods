using HarmonyLib;
using QModManager.API.ModLoading;
using QModManager.Utility;
using SMLHelper.V2.Handlers;
using System.Reflection;

namespace FringingReefEditor
{
    [QModCore]
    public class FringingReefEditor
    {
        internal static FringingReefConfig Config { get; private set; }

        [QModPatch]
        public static void Patch() {
            var assembly = Assembly.GetExecutingAssembly();

            var modName = ($"eternalight_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {modName}");

            Config = OptionsPanelHandler.RegisterModOptions<FringingReefConfig>();

            Harmony harmony = new Harmony(modName);
            harmony.PatchAll();

            Logger.Log(Logger.Level.Info, "Patched successfully!");
        }
    }
}
