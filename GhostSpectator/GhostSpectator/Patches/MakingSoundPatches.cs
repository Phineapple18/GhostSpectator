using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using GhostSpectator.Extensions;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.Thirdperson;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(AnimatedCharacterModel), "PlayFootstep")]
    internal class FootstepSoundPatch
    {
        internal static bool Prefix(AnimatedCharacterModel __instance)
        {
            return !__instance.OwnerHub.IsGhost();
        }
    }

    [HarmonyPatch(typeof(FirearmExtensions), nameof(FirearmExtensions.ServerSendAudioMessage))]
    internal class FirearmSoundPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label moveNext = generator.DefineLabel();
            newInstructions.FindAll((CodeInstruction i) => i.opcode == OpCodes.Ldloca_S).ElementAt(4).labels.Add(moveNext);
            int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == AccessTools.Field(typeof(ReferenceHub), nameof(ReferenceHub.roleManager)));
            int offset = -1;

            newInstructions.InsertRange(index + offset, new List<CodeInstruction>
            {
                new(OpCodes.Ldloc_1),
                new(OpCodes.Ldloc_S, 5),
                new(OpCodes.Call, AccessTools.Method(typeof(FirearmSoundPatch), nameof(OverrideGunShot), new[] { typeof(ReferenceHub), typeof(ReferenceHub) })),
                new(OpCodes.Brtrue_S, moveNext)
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static bool OverrideGunShot(ReferenceHub shooter, ReferenceHub hub)
        {
            return shooter.IsGhost() && hub.GetRoleId() == RoleTypeId.Scp939;
        }
    }
}
