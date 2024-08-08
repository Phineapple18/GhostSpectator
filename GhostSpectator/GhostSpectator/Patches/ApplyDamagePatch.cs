using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection.Emit;

using GhostSpectator.Extensions;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerStatsSystem;
using PluginAPI.Core;
using System.Reflection;

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

            newInstructions.InsertRange(index, new List<CodeInstruction>
            {
                new(OpCodes.Call, AccessTools.Method(typeof(ApplyDamagePatch), nameof(SkipProcessing), new[] { typeof(StandardDamageHandler), typeof(ReferenceHub) })),
                new(OpCodes.Brtrue, skip),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldarg_1)
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static bool SkipProcessing(StandardDamageHandler standardHandler, ReferenceHub hub)
        {
            if (standardHandler is not AttackerDamageHandler attackHandler || Server.FriendlyFire)
            {
                return false;
            }
            if (!(attackHandler.Attacker.Hub.IsGhost() && hub.IsGhost()))
            {
                return false;
            }
            Player attacker = Player.Get(attackHandler.Attacker.Hub);
            Player target = Player.Get(hub);
            return attacker.GetGhostComponent().DuelPartner == target && target.GetGhostComponent().DuelPartner == attacker;
        }
    }
}
