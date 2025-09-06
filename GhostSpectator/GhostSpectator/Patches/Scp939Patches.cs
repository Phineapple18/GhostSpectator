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
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp939.Ripples;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(FirearmRippleTrigger), "OnFirearmPlayed")]
    internal class FirearmRipplePatch
    {
        internal static bool Prefix(PlayerRoleBase shooterRole)
        {
            return !(shooterRole.TryGetOwner(out ReferenceHub hub) && hub.IsGhost());
        }
    }

    [HarmonyPatch(typeof(SurfaceRippleTrigger), "LateUpdate")]
    internal class SurfaceRipplePatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label moveNext = generator.DefineLabel();
            newInstructions.FindAll(i => i.opcode == OpCodes.Ldloca_S).ElementAt(5).labels.Add(moveNext);
            int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == AccessTools.Field(typeof(ReferenceHub), nameof(ReferenceHub.playerEffectsController)));
            int offset = -1;

            newInstructions.InsertRange(index + offset, new List<CodeInstruction>
            {
                new(OpCodes.Ldloc_1),
                new(OpCodes.Call, AccessTools.Method(typeof(Ghost), nameof(Ghost.IsGhost), new[] { typeof(ReferenceHub) })),
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
