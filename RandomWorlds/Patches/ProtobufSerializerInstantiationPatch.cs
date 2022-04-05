using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace RandomWorlds.Patches {

    [HarmonyPatch(typeof(ProtobufSerializer))]
    [HarmonyPatch("DeserializeIntoGameObject", new Type[] {typeof(Stream), typeof(ProtobufSerializer.GameObjectData), typeof(UniqueIdentifier), typeof(bool), typeof(bool), typeof(UnityEngine.Transform), typeof(int) })]
    class ProtobufSerializer_DeserializeIntoGameObjectPatch {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> originalInstructions) {

            MethodInfo deserializeComponentMethod = AccessTools.Method(typeof(ProtobufSerializer), nameof(ProtobufSerializer.Deserialize), new Type[] { typeof(Stream), typeof(object), typeof(Type), typeof(bool) });

            MethodInfo deserializeLoopMethod = null;
            MethodInfo deserializeComponentHeaderMethod = null;
            var mi = typeof(ProtobufSerializer).GetMethods().Where(method => method.Name == "Deserialize");
            foreach (var method in mi) {
                if (method.IsGenericMethod) {
                    deserializeLoopMethod = method.MakeGenericMethod(typeof(ProtobufSerializer.LoopHeader));
                    deserializeComponentHeaderMethod = method.MakeGenericMethod(typeof(ProtobufSerializer.ComponentHeader));
                }
            }

            MethodInfo setIsEnabled = AccessTools.Method(typeof(ProtobufSerializer), nameof(ProtobufSerializer.SetIsEnabled));

            bool foundLoopHeader = false;
            bool foundComponentHeader = false;
            bool foundComponent = false;
            bool foundSetEnabled = false;
            MethodInfo fillLoop = AccessTools.Method(typeof(EntityProvider), nameof(EntityProvider.FillComponentCount));
            MethodInfo fillComponent = AccessTools.Method(typeof(EntityProvider), nameof(EntityProvider.FillComponentHeader));
            MethodInfo processComponent = AccessTools.Method(typeof(EntityProvider), nameof(EntityProvider.ProcessComponent));

            var popInstruction = new CodeInstruction(OpCodes.Pop);
            var callSkip = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ProtobufSerializer), nameof(ProtobufSerializer.SkipDeserialize)));

            foreach (CodeInstruction instruction in originalInstructions) {
                if (instruction.Calls(deserializeLoopMethod)) 
                {
                    foundLoopHeader = true;
                    // pop verbose
                    yield return popInstruction;

                    // pop loopHeader
                    yield return new CodeInstruction(OpCodes.Call, fillLoop);

                    // pop ProtobufSerializer & stream
                    yield return callSkip;
                } else if (instruction.Calls(deserializeComponentHeaderMethod)) {
                    foundComponentHeader = true;

                    // pop verbose
                    yield return popInstruction;

                    // push component index
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 6);

                    // pop componentHeader & index
                    yield return new CodeInstruction(OpCodes.Call, fillComponent);

                    // pop ProtobufSerializer & stream
                    yield return callSkip;
                } else if (instruction.Calls(deserializeComponentMethod)) {
                    foundComponent = true;

                    // Pop: ProtobufSerializer, Stream, object, Type, bool

                    // pop bool
                    yield return popInstruction;
                    // pop object
                    yield return popInstruction;
                    // pop type
                    yield return popInstruction;

                    // pop ProtobufSerializer & stream
                    yield return callSkip;
                } else if (instruction.Calls(setIsEnabled)) 
                {
                    foundSetEnabled = true;
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 11);
                    yield return new CodeInstruction(OpCodes.Call, processComponent);
                } 
                else 
                {
                    yield return instruction;
                }
            }

            if (!foundLoopHeader) {
                RandomWorldsJournalist.Log(2, "Could not find <call Deserialize<LoopHeader>> in ProtobufSerializer.DeserializeIntoGameObject");
            }
            if (!foundComponentHeader) {
                RandomWorldsJournalist.Log(2, "Could not find <call Deserialize<ComponentHeader>> in ProtobufSerializer.DeserializeIntoGameObject");
            }
            if (!foundComponent) {
                RandomWorldsJournalist.Log(2, "Could not find <call Deserialize<Component>> in ProtobufSerializer.DeserializeIntoGameObject");
            }
            if (!foundSetEnabled) {
                RandomWorldsJournalist.Log(2, "Could not find <call SetIsActive> in ProtobufSerializer.DeserializeIntoGameObject");
            }
        }
    }
}
