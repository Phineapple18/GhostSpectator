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
using PlayerRoles;
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

    [HarmonyPatch(typeof(FirearmRippleTrigger), "OnFirearmPlayed")]
    internal class FirearmRipplePatch
    {
        internal static bool Prefix(PlayerRoleBase shooterRole)
        {
            if (shooterRole.TryGetOwner(out ReferenceHub hub) && hub.IsGhost())
            {
                return false;
            }
            return true;
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
