using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using GhostSpectator.Extensions;
using HarmonyLib;
using InventorySystem.Items;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Scp939.Ripples;

namespace GhostSpectator.Patches
{
	[HarmonyPatch(typeof(Scp939AmnesticCloudInstance), nameof(Scp939AmnesticCloudInstance.OnStay))]
	internal class AmnesticCloudPatch
	{
		internal static bool Prefix(ReferenceHub player)
		{
            return !player.IsGhost();
        }
    }

    [HarmonyPatch(typeof(FirearmRippleTrigger), "OnServerSoundPlayed")]
    internal class FirearmRipplePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label ret = generator.DefineLabel();
            int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == AccessTools.PropertyGetter(typeof(ItemBase), "Owner"));

            newInstructions.InsertRange(index, new List<CodeInstruction>
            {
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ItemBase), nameof(ItemBase.Owner))),
                new(OpCodes.Call, AccessTools.Method(typeof(GhostExtensions), nameof(GhostExtensions.IsGhost), new[] { typeof(ReferenceHub) })),
                new(OpCodes.Brtrue_S, ret),
                new(OpCodes.Ldarg_1)
            });

            newInstructions[newInstructions.Count - 1].WithLabels(ret);
            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }

    [HarmonyPatch(typeof(SurfaceRippleTrigger), "LateUpdate")]
    internal class SurfaceRipplePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label moveNext = generator.DefineLabel();
            newInstructions.FindAll((CodeInstruction i) => i.opcode == OpCodes.Ldloca_S).ElementAt(5).labels.Add(moveNext);
			int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == AccessTools.Field(typeof(ReferenceHub), nameof(ReferenceHub.playerEffectsController)));
            int offset = -1;

            newInstructions.InsertRange(index + offset, new List<CodeInstruction>
            {
                new(OpCodes.Ldloc_1),
                new(OpCodes.Call, AccessTools.Method(typeof(GhostExtensions), nameof(GhostExtensions.IsGhost), new[] { typeof(ReferenceHub) })),
                new(OpCodes.Brtrue, moveNext)
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
