using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection.Emit;

using GhostSpectator.Extensions;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using NorthwoodLib.Pools;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(StandardHitregBase), "PlaceBulletholeDecal")]
    internal class BulletDecalPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label ret = generator.DefineLabel();
            int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Ldarga_S);

            newInstructions.InsertRange(index, new List<CodeInstruction>
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(StandardHitregBase), "Hub")),
                new(OpCodes.Call, AccessTools.Method(typeof(GhostExtensions), nameof(GhostExtensions.IsGhost), new[] { typeof(ReferenceHub) })),
                new(OpCodes.Brtrue, ret)
            });

            newInstructions[newInstructions.Count - 1].WithLabels(ret);
            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
