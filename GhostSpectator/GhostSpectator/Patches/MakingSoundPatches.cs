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
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
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

    [HarmonyPatch(typeof(AudioModule), "ServerSendToNearbyPlayers")]
    internal class FirearmSoundPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label moveNext = generator.DefineLabel();
            newInstructions.FindAll((CodeInstruction i) => i.opcode == OpCodes.Ldloca_S).ElementAt(1).labels.Add(moveNext);
            int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Call && (MethodInfo)i.operand == AccessTools.PropertyGetter(typeof(HashSet<ReferenceHub>.Enumerator), "Current"));
            int offset = 2;

            newInstructions.InsertRange(index + offset, new List<CodeInstruction>
            {
                new(OpCodes.Ldloc_S, 4),
                new(OpCodes.Ldloca_S, 3),
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(HashSet<ReferenceHub>.Enumerator), "Current")),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(FirearmSubcomponentBase), nameof(FirearmSubcomponentBase.Firearm))),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ItemBase), nameof(ItemBase.Owner))),
                new(OpCodes.Call, AccessTools.Method(typeof(FirearmSoundPatch), nameof(MuteShot), new[] { typeof(ReferenceHub), typeof(ReferenceHub) })),
                new(OpCodes.Brtrue_S, moveNext),
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static bool MuteShot(ReferenceHub receiver, ReferenceHub shooter)
        {
            return shooter.IsGhost() && !receiver.IsGhost() && receiver.IsAlive();
        }
    }
}
