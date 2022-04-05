using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace RandomWorlds.Patches {
    class ProtobufSerializerPatch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> originalInstructions, MethodBase original) {
            MethodInfo deserializeLoopMethod = null;
            MethodInfo deserializeGameObjectMethod = null;
            var mi = typeof(ProtobufSerializer).GetMethods().Where(method => method.Name == "Deserialize");
            foreach(var method in mi) {
                if (method.IsGenericMethod) {
                    deserializeLoopMethod = method.MakeGenericMethod(typeof(ProtobufSerializer.LoopHeader));
                    deserializeGameObjectMethod = method.MakeGenericMethod(typeof(ProtobufSerializer.GameObjectData));
                }
            }

            bool foundLoopHeader = false;
            bool foundGoData = false;

            var loopHeaderMod = AccessTools.Method(typeof(EntityProvider), nameof(EntityProvider.FillGameObjectCount));
            var gameObjectDataMod = AccessTools.Method(typeof(EntityProvider), nameof(EntityProvider.FillDataForGameObject));

            var popInstruction = new CodeInstruction(OpCodes.Pop);
            var callSkip = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ProtobufSerializer), nameof(ProtobufSerializer.SkipDeserialize)));

            foreach (CodeInstruction instruction in originalInstructions) {
                if (instruction.Calls(deserializeLoopMethod)) 
                {
                    foundLoopHeader = true;
                    // pop verbose
                    yield return popInstruction;
                    // Pop loop header by filling it with data
                    yield return new CodeInstruction(OpCodes.Call, loopHeaderMod);
                    // Pop ProtobufSerializer & stream by skipping deserialization
                    yield return callSkip;
                    // should be done!
                } 
                else if (instruction.Calls(deserializeGameObjectMethod)) 
                {
                    foundGoData = true;
                    var iteratorType = typeof(ProtobufSerializer).GetNestedType("<DeserializeObjectsAsync>d__32", AccessTools.all);
                    // pop verbose
                    yield return popInstruction;

                    //var goDataField = AccessTools.Field(iteratorType, "<gameObjectData>5__3");
                    var goIndexField = AccessTools.Field(iteratorType, "<i>5__4");

                    // push go index
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, goIndexField);
                    // pop goData & index 
                    yield return new CodeInstruction(OpCodes.Call, gameObjectDataMod);

                    // Pop ProtobufSerializer & stream by skipping deserialization
                    yield return callSkip;
                    // should be done!
                } else 
                {
                    yield return instruction;
                }
            }

            if (!foundLoopHeader) {
                RandomWorldsJournalist.Log(2, "Could not find <call Deserialize<LoopHeader>> in ProtobufSerializer.DeserializeObjectsAsync");
            }
            if (!foundGoData) {
                RandomWorldsJournalist.Log(2, "Could not find <call Deserialize<GameObjectData>> in ProtobufSerializer.DeserializeObjectsAsync");
            }
        }
    }

    [HarmonyPatch(typeof(ProtobufSerializer), nameof(ProtobufSerializer.TryDeserializeStreamHeader))]
    class ProtobufSerializer_TryDeserializeStreamHeaderPatch {

        public static bool ignoreHeader;
        
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result) {
            if (ignoreHeader) {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
