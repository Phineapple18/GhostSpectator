using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using GhostSpectator.Features.Extensions;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps.Scp049;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(Scp049ResurrectAbility), "CheckBeginConditions")]
    internal class BeginConditionsPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label nextCondition1 = generator.DefineLabel();
            newInstructions.FindAll(i => i.opcode == OpCodes.Ldarg_1).ElementAt(2).labels.Add(nextCondition1);
            int index1 = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldsfld && (FieldInfo)i.operand == AccessTools.Field(typeof(Scp049ResurrectAbility), "DeadZombies"));
            int offset1 = 5;

            newInstructions.InsertRange(index1 + offset1, new List<CodeInstruction>
            {
                new(OpCodes.Ldsfld, AccessTools.Field(typeof(EventHandler), nameof(EventHandler.deadZombies))),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Callvirt, AccessTools.Method(typeof(HashSet<ReferenceHub>), nameof(HashSet<ReferenceHub>.Contains), new[] { typeof(ReferenceHub) })),
                new(OpCodes.Brtrue_S, nextCondition1)
            });

            Label nextCondition2 = generator.DefineLabel();
            newInstructions.FindAll(i => i.opcode == OpCodes.Ldarg_0).ElementAt(2).labels.Add(nextCondition2);
            int index2 = newInstructions.FindIndex(i => i.opcode == OpCodes.Call && (MethodInfo)i.operand == AccessTools.Method(typeof(Scp049ResurrectAbility), "IsSpawnableSpectator"));
            int offset2 = 2;

            newInstructions.InsertRange(index2 + offset2, new List<CodeInstruction>
            {
                new(OpCodes.Ldloc_0),
                new(OpCodes.Call, AccessTools.Method(typeof(Other), nameof(Other.IsDead), new[] { typeof(ReferenceHub) })),
                new(OpCodes.Brtrue_S, nextCondition2)
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }

    [HarmonyPatch(typeof(Scp049ResurrectAbility), "ServerValidateAny")]
    internal class ValidateAnyPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label nextCondition = generator.DefineLabel();
            newInstructions.FindAll(i => i.opcode == OpCodes.Ldarg_0).ElementAt(3).labels.Add(nextCondition);
            int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Call && (MethodInfo)i.operand == AccessTools.Method(typeof(Scp049ResurrectAbility), "IsSpawnableSpectator"));
            int offset = 1;

            newInstructions.InsertRange(index + offset, new List<CodeInstruction>
            {
                new(OpCodes.Brtrue_S, nextCondition),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Call, AccessTools.Method(typeof(Other), nameof(Other.IsDead), new[] { typeof(ReferenceHub) }))
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
