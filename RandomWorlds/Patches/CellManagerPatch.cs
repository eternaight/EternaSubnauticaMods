using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace RandomWorlds.Patches {

#if RUNTIME_GENERATION

    [HarmonyPatch(typeof(CellManager), nameof(CellManager.LoadCacheBatchCellsFromStream))]
    class CellManagerPatch {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {

            MethodInfo deserializeFileHeader = null;
            MethodInfo deserializeCellHeader = null;
            var mi = typeof(ProtobufSerializer).GetMethods().Where(method => method.Name == "Deserialize");
            foreach (var method in mi) {
                if (method.IsGenericMethod) {
                    deserializeFileHeader = method.MakeGenericMethod(typeof(CellManager.CellsFileHeader));
                    deserializeCellHeader = method.MakeGenericMethod(typeof(CellManager.CellHeaderEx));
                }
            }

            var popInstruction = new CodeInstruction(OpCodes.Pop);
            var callSkip = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ProtobufSerializer), nameof(ProtobufSerializer.SkipDeserialize)));

            foreach (CodeInstruction instruction in instructions) {
                if (instruction.Calls(deserializeFileHeader) && !RandomWorlds.learningMode) {
                    // pop verbose
                    yield return popInstruction;
                    // pop target
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CellManagerPatch), nameof(CellManagerPatch.ConsumeStream)));
                    // pop stream
                    yield return popInstruction;
                    // pop protobufserializer
                    yield return popInstruction;
                }
                else {
                    yield return instruction;
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(BatchCells cells, Stream stream) {

            var cfh = new CellManager.CellsFileHeader(); 
            EntitySpawnManager.FillCellsFileHeader(cfh, cells.batch);

            var chex = new CellManager.CellHeaderEx();
            for (int i = 0; i < cfh.numCells; i++) {
                EntitySpawnManager.FillCellHeader(chex, i);
                var cellId = BatchCells.GetCellId(chex.cellId, chex.level, cfh.version);
                var cell = cells.Add(cellId, chex.level);
                cell.Initialize();

                using (UWE.PooledObject<ProtobufSerializer> proxy = ProtobufSerializerPool.GetProxy()) {
                    using (MemoryStream mem = new MemoryStream()) {
                        proxy.Value.SerializeStreamHeader(mem);
                        cell.ReadSerialDataFromStream(mem, (int)mem.Length);
                    }
                }
            }
        }

        private static void ConsumeStream(CellManager.CellsFileHeader cfh) {
            cfh.numCells = 0;
        }
    }

    [HarmonyPatch(typeof(CellManager), nameof(CellManager.TryLoadCacheBatchCells))]
    class CellManagerPatchTwo {
        [HarmonyPrefix]
        public static bool Prefix(BatchCells cells, ref bool __result) {
            
            if (EntitySpawnManager.PrecomputeBatchEntities(cells.batch)) {
                CellManager.LoadCacheBatchCellsFromStream(cells, null);
                __result = true;
            } else {
                __result = false;
            }
            
            return false;
        }
    }
#endif
}
