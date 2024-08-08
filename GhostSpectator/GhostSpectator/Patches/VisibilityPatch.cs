using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using CustomPlayerEffects;
using GhostSpectator.Extensions;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.Spectating;
using PlayerRoles.Visibility;


namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
    internal class VisibilityPatch
	{
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            
            int index = newInstructions.FindIndex((CodeInstruction i) => i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == AccessTools.Method(typeof(VisibilityController), nameof(VisibilityController.ValidateVisibility)));
            int offset = 6;

            newInstructions.InsertRange(index + offset, new List<CodeInstruction>
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloc_S, 5),
                new(OpCodes.Ldloca_S, 7),
                new(OpCodes.Call, AccessTools.Method(typeof(VisibilityPatch), nameof(IsGhostInvisible), new[] { typeof(ReferenceHub), typeof(ReferenceHub), typeof(bool).MakeByRefType() }))
            });

            for (int i = 0; i < newInstructions.Count; i++)
            {
                yield return newInstructions[i];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static void IsGhostInvisible(ReferenceHub observer, ReferenceHub observed, ref bool isInvisible)
        {
            if (observed.IsGhost())
            {
                if (observed.playerEffectsController.TryGetEffect<Invisible>(out Invisible invisible) && invisible.Intensity > 0)
                {
                    isInvisible = true;
                    return;
                }
                if (observer.IsGhost() || observer.GetRoleId() == RoleTypeId.Overwatch)
                {
                    isInvisible = false;
                    return;
                }
                if (observer.IsAlive())
                {
                    isInvisible = true;
                    return;
                }
                if (observer.GetRoleId() == RoleTypeId.Filmmaker)
                {
                    isInvisible = !Plugin.Singleton.pluginConfig.FilmmakerSeeGhosts;
                    return;
                }
                ReferenceHub spectated = ReferenceHub.AllHubs.FirstOrDefault(p => p.IsSpectatedBy(observer));
                isInvisible = !(spectated.IsGhost() || Plugin.Singleton.pluginConfig.AlwaysSeeGhosts);
            }
        }
    }
}
