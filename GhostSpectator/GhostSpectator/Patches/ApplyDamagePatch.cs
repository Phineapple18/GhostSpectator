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
using PlayerStatsSystem;
using PluginAPI.Core;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(StandardDamageHandler), nameof(StandardDamageHandler.ApplyDamage))]
    internal class ApplyDamagePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label skip = generator.DefineLabel();
            newInstructions.FindAll((CodeInstruction i) => i.opcode == OpCodes.Ldarg_1).ElementAt(4).labels.Add(skip);
            int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == AccessTools.Method(typeof(StandardDamageHandler), "ProcessDamage"));
            int offset = -2;

            List<CodeInstruction> codeInstructions = new()
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Call, AccessTools.Method(typeof(ApplyDamagePatch), nameof(SkipProcessing), new[] { typeof(StandardDamageHandler), typeof(ReferenceHub) })),
                new(OpCodes.Brtrue, skip)
            };
            newInstructions.InsertRange(index + offset, codeInstructions);

            newInstructions[index + offset + codeInstructions.Count].MoveLabelsTo(newInstructions[index + offset]);
            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static bool SkipProcessing(StandardDamageHandler standardHandler, ReferenceHub hub)
        {
            if (Server.FriendlyFire || standardHandler is not AttackerDamageHandler attackHandler)
            {
                return false;
            }
            if (!(attackHandler.Attacker.Hub.IsGhost() && hub.IsGhost()))
            {
                return false;
            }
            Player attacker = Player.Get(attackHandler.Attacker.Hub);
            Player target = Player.Get(hub);
            return attacker.GetComponent<GhostComponent>().DuelPartner == target && target.GetComponent<GhostComponent>().DuelPartner == attacker;
        }
    }
}
