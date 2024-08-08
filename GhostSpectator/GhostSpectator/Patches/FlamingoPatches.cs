using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection.Emit;

using GhostSpectator.Extensions;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;

#if CHRISTMAS
using Christmas.Scp2536.Gifts;
using InventorySystem.Items.FlamingoTapePlayer;
using PlayerRoles.PlayableScps.Scp1507;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(Scp1507Spawner), "ValidatePlayer")]
    internal class FlamingoCheckSpawnablePatch
    {
        internal static void Postfix(ReferenceHub candidate, ref bool __result)
        {
            if (candidate.IsGhost())
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(TapeItem), nameof(TapeItem.ServerProcessCmd))]
    internal class UseTapePatch
    {
        internal static bool Prefix(TapeItem __instance)
        {
            return !__instance.Owner.IsGhost();
        }
    }   

    [HarmonyPatch(typeof(Tape), nameof(Tape.CanBeGranted))]
    internal class GrantTapePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label nextCondition = generator.DefineLabel();
            newInstructions.FindAll((CodeInstruction i) => i.opcode == OpCodes.Ldloc_0).ElementAt(0).labels.Add(nextCondition);
            int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Isinst);
            int offset = 1;

            newInstructions.InsertRange(index + offset, new List<CodeInstruction>
            {
                new(OpCodes.Brtrue_S, nextCondition),
                new(OpCodes.Ldloc_S, 4),
                new(OpCodes.Call, AccessTools.Method(typeof(GhostExtensions), nameof(GhostExtensions.IsGhost), new[] { typeof(ReferenceHub) })),
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
} 
#endif