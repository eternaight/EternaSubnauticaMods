using HarmonyLib;
using QModManager.API.ModLoading;
using QModManager.Utility;
using RandomWorlds.Patches;
using SMLHelper.V2.Handlers;
using System;
using System.IO;
using System.Reflection;

namespace RandomWorlds {
    [QModCore]
    public class RandomWorlds {
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
            PatchRasterization(harmony);
            harmony.PatchAll();

            SNWorldBlueprint.Apply();

            Logger.Log(Logger.Level.Info, "Patched successfully!");
        }

        private static void PatchRasterization(Harmony harmony) {
            PatchIterator(harmony, typeof(ProtobufSerializer), "<DeserializeObjectsAsync>d__32", typeof(ProtobufSerializerPatch));
            PatchIterator(harmony, typeof(PAXTerrainController), "<LoadAsync>d__32", typeof(PAXTerrainController_LoadAsyncPatch));
        }

        private static void PatchIterator(Harmony harmony, Type originalType, string stateMachineName, Type transpilerType) {
            var iterator = originalType.GetNestedType(stateMachineName, AccessTools.all);
            var iteratorMoveNext = AccessTools.Method(iterator, "MoveNext");
            var transpilerMethod = new HarmonyMethod(AccessTools.Method(transpilerType, "Transpiler"));
            harmony.Patch(iteratorMoveNext, transpiler: transpilerMethod);
            RandomWorldsJournalist.Log(0, $"Successfully patched {transpilerType.Name}");
        }

        public static string GetWorldPath() {
            var path = Path.Combine(RandomWorlds.ModDirectory, "World");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
