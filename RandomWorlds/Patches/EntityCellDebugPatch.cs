using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RandomWorlds.Patches {

#if RUNTIME_GENERATION

    [HarmonyPatch(typeof(EntityCell), nameof(EntityCell.AwakeAsync))]
    class EntityCellDebugPatch {

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {

            var copyMethodInfo = AccessTools.Method(typeof(EntitySpawnManager), nameof(EntitySpawnManager.OnCellRootAwoken));
            bool foundNewobjInstruction = false;

            foreach (CodeInstruction instruction in instructions) {
                if (instruction.opcode == OpCodes.Newobj) {
                    foundNewobjInstruction = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, copyMethodInfo);
                }
                yield return instruction;
            }

            if (!foundNewobjInstruction) {
                RandomWorldsJournalist.Log(2, $"Failed to patch <newobj> instruction in EntityCell.AwakeAsync.");
            }
        }
    }
#endif
}