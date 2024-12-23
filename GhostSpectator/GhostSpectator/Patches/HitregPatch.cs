using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using GhostSpectator.Extensions;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using MEC;
using NorthwoodLib.Pools;
using UnityEngine;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(HitscanHitregModuleBase), "ServerPerformHitscan")]
    internal class HitregPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label goBack = generator.DefineLabel();
            newInstructions.FindAll((CodeInstruction i) => i.opcode == OpCodes.Ldarg_1).ElementAt(0).labels.Add(goBack);
            int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Call && (MethodInfo)i.operand == AccessTools.PropertyGetter(typeof(RaycastHit), "collider"));
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
            if (!collider.TryGetComponent<IDestructible>(out IDestructible destructible))
            {
                return false;
            }
            HitboxIdentity identity = destructible as HitboxIdentity;
            if (identity == null || shooter.IsGhost() || !identity.TargetHub.IsGhost())
            {
                return false;
            }
            identity.SetColliders(false);
            Timing.CallDelayed(Timing.WaitForOneFrame, () => identity.SetColliders(true));
            return true;
        }
    }
}
