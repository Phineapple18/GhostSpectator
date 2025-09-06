using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using GhostSpectator.Features.Extensions;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Wrappers;
using MEC;
using NorthwoodLib.Pools;
using PlayerStatsSystem;
using UnityEngine;

namespace GhostSpectator.Patches
{
    [HarmonyPatch("FriendlyFireHandler", "IsFriendlyFire")]
    internal class FriendlyFirePatch
    {
        internal static void Postfix(ReferenceHub damagedPlayer, DamageHandlerBase handler, ref bool __result)
        {
            if (__result && (handler as AttackerDamageHandler).Attacker.Hub.IsGhost() && damagedPlayer.IsGhost())
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(AttackerDamageHandler), "ProcessDamage")]
    internal class ProcessDamagePatch
    {
        internal static bool Prefix(AttackerDamageHandler __instance, ReferenceHub ply)
        {
            return !(!Server.FriendlyFire && __instance.Attacker.Hub.IsGhost() && ply.IsGhost()
                   && __instance.Attacker.Hub.GetGhostComponent().DuelPartner.ReferenceHub == ply
                   && ply.GetGhostComponent().DuelPartner.ReferenceHub == __instance.Attacker.Hub);
        }
    }

    [HarmonyPatch(typeof(HitscanHitregModuleBase), "ServerAppendPrescan")]
    internal class HitregPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label goBack = generator.DefineLabel();
            newInstructions.FindAll(i => i.opcode == OpCodes.Ldarg_1).ElementAt(0).labels.Add(goBack);
            int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Call && (MethodInfo)i.operand == AccessTools.PropertyGetter(typeof(RaycastHit), "collider"));
            int offset = -1;

            List<CodeInstruction> codeInstructions = new()
            {
                new(OpCodes.Ldloca_S, 1),
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(RaycastHit), "collider")),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(HitscanHitregModuleBase), "Owner")),
                new(OpCodes.Call, AccessTools.Method(typeof(HitregPatch), nameof(RecastRay), new[] { typeof(Collider), typeof(ReferenceHub) })),
                new(OpCodes.Brtrue, goBack)
            };
            newInstructions.InsertRange(index + offset, codeInstructions);

            newInstructions[index + offset + codeInstructions.Count].MoveLabelsTo(newInstructions[index + offset]);
            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static bool RecastRay(Collider collider, ReferenceHub shooter)
        {
            if (shooter.IsGhost() || !collider.TryGetComponent(out HitboxIdentity identity) || !identity.TargetHub.IsGhost())
            {
                return false;
            }
            identity.SetColliders(false);
            Timing.CallDelayed(Timing.WaitForOneFrame, () => identity.SetColliders(true));
            return true;
        }
    }
}
