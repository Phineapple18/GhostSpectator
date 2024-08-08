using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using GhostSpectator.Extensions;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps.Scp049;
using PluginAPI.Core;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(Scp049ResurrectAbility), "CheckBeginConditions")]
    internal class BeginConditionsPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label nextCondition1 = generator.DefineLabel();
            newInstructions.FindAll((CodeInstruction i) => i.opcode == OpCodes.Ldarg_1).ElementAt(2).labels.Add(nextCondition1);
            int index1 = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Ldsfld && (FieldInfo)i.operand == AccessTools.Field(typeof(Scp049ResurrectAbility), "DeadZombies"));
            int offset1= 4;

            newInstructions.InsertRange(index1 + offset1, new List<CodeInstruction>
            {
                new(OpCodes.Brtrue_S, nextCondition1),
                new(OpCodes.Ldsfld, AccessTools.Field(typeof(EventHandlers), nameof(EventHandlers.deadZombies))),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Callvirt, AccessTools.Method(typeof(HashSet<ReferenceHub>), nameof(HashSet<ReferenceHub>.Contains), new[] { typeof(ReferenceHub) }))
            });

            Label nextCondition2 = generator.DefineLabel();
            newInstructions.FindAll((CodeInstruction i) => i.opcode == OpCodes.Ldarg_0).ElementAt(2).labels.Add(nextCondition2);
            int index2 = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Call && (MethodInfo)i.operand == AccessTools.Method(typeof(Scp049ResurrectAbility), "IsSpawnableSpectator"));
            int offset2 = 1;

            newInstructions.InsertRange(index2 + offset2, new List<CodeInstruction>
            {
                new(OpCodes.Brtrue_S, nextCondition2),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Call, AccessTools.Method(typeof(BeginConditionsPatch), nameof(IsDead), new[] { typeof(ReferenceHub) }))
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static bool IsDead(ReferenceHub hub)
        {
            return Player.Get(hub).TemporaryData.Contains(GhostExtensions.dataName);
        }
    }

    [HarmonyPatch(typeof(Scp049ResurrectAbility), "ServerValidateAny")]
    internal class ValidateAnyPatch
	{
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label nextCondition = generator.DefineLabel();
            newInstructions.FindAll((CodeInstruction i) => i.opcode == OpCodes.Ldarg_0).ElementAt(3).labels.Add(nextCondition);
            int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Call && (MethodInfo)i.operand == AccessTools.Method(typeof(Scp049ResurrectAbility), "IsSpawnableSpectator"));
            int offset = 1;

            newInstructions.InsertRange(index + offset, new List<CodeInstruction>
            {
                new(OpCodes.Brtrue_S, nextCondition),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Call, AccessTools.Method(typeof(ValidateAnyPatch), nameof(IsDead), new[] { typeof(ReferenceHub) }))
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static bool IsDead(ReferenceHub hub)
        {
            return Player.Get(hub).TemporaryData.Contains(GhostExtensions.dataName);
        }
    }
}
