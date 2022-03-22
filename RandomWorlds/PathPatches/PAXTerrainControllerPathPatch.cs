using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RandomWorlds.Patches {
#if !RUNTIME_GENERATION
    class PAXTerrainControllerPathPatch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {

            bool foundMountWorld = false;
            var getPathMethod = AccessTools.Method(typeof(SNUtils), nameof(SNUtils.InsideUnmanaged));

            foreach (CodeInstruction instruction in instructions) {

                if (instruction.Calls(getPathMethod)) {
                    foundMountWorld = true;
                    // pop "Build18" string
                    yield return new CodeInstruction(OpCodes.Pop);
                    MethodInfo getProperPathMethod = AccessTools.Method(typeof(WorldManager), nameof(WorldManager.GetWorldPath));
                    yield return new CodeInstruction(OpCodes.Call, getProperPathMethod);
                } else {
                    yield return instruction;
                }
            }

            if (!foundMountWorld) {
                RandomWorldsJournalist.Log(2, $"Cannot find <{getPathMethod}> in {original.DeclaringType}");
            }
        }
    }
#endif
}
