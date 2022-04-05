using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace RandomWorlds.Patches {

    [HarmonyPatch(typeof(CellManager), nameof(CellManager.LoadCacheBatchCellsFromStream))]
    class CellManager_LoadCacheBatchCellsFromStreamPatch {
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
                if (instruction.Calls(deserializeFileHeader)) {
                    // pop verbose
                    yield return popInstruction;
                    // pop target
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CellManager_LoadCacheBatchCellsFromStreamPatch), "ConsumeStream"));
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

        private static void ConsumeStream(CellManager.CellsFileHeader cfh) {
            cfh.numCells = 0;
        }

        [HarmonyPostfix]
        public static void Postfix(BatchCells cells, Stream stream) {

            var cfh = new CellManager.CellsFileHeader(); 
            EntityProvider.ProvideCellFileHeader(cfh, cells.batch);

            var chex = new CellManager.CellHeaderEx();
            for (int i = 0; i < cfh.numCells; i++) {
                EntityProvider.ProvideCellHeader(chex, i);
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
    }

    [HarmonyPatch(typeof(CellManager), nameof(CellManager.TryLoadCacheBatchCells))]
    class CellManager_TryLoadCacheBatchCellsPatch {
        [HarmonyPrefix]
        public static bool Prefix(BatchCells cells, ref bool __result) {
            __result = EntityProvider.PrecomputeCellMask(cells.batch);
            if (__result) CellManager.LoadCacheBatchCellsFromStream(cells, null);
            return false;
        }
    }
}
