using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Features.Extensions;
using HarmonyLib;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp049;
using static PlayerRoles.PlayableScps.Scp049.Scp049ResurrectAbility;
using PlayerRoles.Ragdolls;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(Scp049ResurrectAbility), "IsSpawnableSpectator")]
    internal class SpawnableSpectatorPatch
    {
        internal static void Postfix(ReferenceHub hub, ref bool __result)
        {
            if (hub.IsGhost() || hub.IsGhostDespawning() || hub.IsGhostSpawning())
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp049ResurrectAbility), "CheckBeginConditions")]
    internal class BeginConditionsPatchNEW
    {
        internal static void Postfix(Scp049ResurrectAbility __instance, BasicRagdoll ragdoll, ref ResurrectError __result)
        {
            ReferenceHub hub = ragdoll.Info.OwnerHub;
            if (ragdoll.Info.RoleType == RoleTypeId.Scp0492 && hub != null && __result == ResurrectError.TargetNull
            && EventHandler.deadZombies.Contains(hub))
            {
                __result = __instance.CheckMaxResurrections(hub);
            }
        }
    }
}
