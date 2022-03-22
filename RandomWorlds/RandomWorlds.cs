using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using QModManager.API.ModLoading;
using QModManager.Utility;
using SMLHelper.V2.Handlers;
using RandomWorlds.Patches;

namespace RandomWorlds
{
    [QModCore]
    public class RandomWorlds {
        public static bool learningMode = false;
        public static bool ignoreHeader = false;

        public static string ModDirectory {
            get {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        internal static RandomWorldsOptions Config { get; private set; }

        [QModPatch]
        public static void Patch() {
            var assembly = Assembly.GetExecutingAssembly();

            var modName = ($"eternalight_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {modName}");

            Config = OptionsPanelHandler.RegisterModOptions<RandomWorldsOptions>();

            Harmony harmony = new Harmony(modName);
#if RUNTIME_GENERATION
            PatchRuntimePatches(harmony);
#else
            PatchPregeneratePatches(harmony);
#endif
            Logger.Log(Logger.Level.Info, "Patched successfully!");
        }

#if RUNTIME_GENERATION
        private static void PatchRuntimePatches(Harmony harmony) {
            PatchIterator(harmony, typeof(ProtobufSerializer), "<DeserializeObjectsAsync>d__32", typeof(ProtobufSerializerPatch));
            PatchIterator(harmony, typeof(PAXTerrainController), "<LoadAsync>d__32", typeof(PAXTerrainControllerPatch));

            harmony.PatchAll();
        }
#else

        private static void PatchPregeneratePatches(Harmony harmony) {
            PatchIterator(harmony, typeof(PAXTerrainController), "<Initialize>d__25", typeof(PAXTerrainControllerPathPatch));
        }
#endif

        public static void PatchIterator(Harmony harmony, Type originalType, string stateMachineName, Type transpilerType) {
            var iterator = originalType.GetNestedType(stateMachineName, AccessTools.all);
            var iteratorMoveNext = AccessTools.Method(iterator, "MoveNext");
            var transpilerMethod = new HarmonyMethod(AccessTools.Method(transpilerType, "Transpiler"));
            harmony.Patch(iteratorMoveNext, transpiler: transpilerMethod);
            RandomWorldsJournalist.Log(0, $"Successfully patched {transpilerType.Name}");
        }
    }
}
