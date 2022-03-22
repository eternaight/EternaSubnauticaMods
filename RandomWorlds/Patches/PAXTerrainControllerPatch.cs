using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RandomWorlds.Patches {
    class PAXTerrainControllerPatch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {

            bool foundLoadTiles = false;
            const string mountResultName = "<mountResult>5__2";
            var mountResultField = AccessTools.Field(original.DeclaringType, mountResultName);

            foreach (CodeInstruction instruction in instructions) {

                if (instruction.LoadsField(mountResultField) && !foundLoadTiles) {
                    foundLoadTiles = true;
                    MethodInfo setupWorldMethod = AccessTools.Method(typeof(WorldManager), nameof(WorldManager.InitializeRuntime));
                    yield return new CodeInstruction(OpCodes.Call, setupWorldMethod);
                    yield return instruction;
                }
                else {
                    yield return instruction;
                }
            }

            if (!foundLoadTiles) {
                RandomWorldsJournalist.Log(2, $"Cannot find <{mountResultName}> in {original.DeclaringType}");
            }
        }
    }
}
