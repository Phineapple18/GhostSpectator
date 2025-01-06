using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Extensions;
using HarmonyLib;
using PlayerRoles;
using PluginAPI.Core;
using Respawning.Waves;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(WaveSpawner), nameof(WaveSpawner.CanBeSpawned))]
    internal class CheckSpawnablePatch
    {
        internal static void Postfix(ReferenceHub player, ref bool __result)
        {
            if (player.IsGhost())
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(WaveSpawner), "CalculatePriority")]
    internal class SpawnPriorityPatch
    {
        internal static void Postfix(ReferenceHub hub, Team targetTeam, ref float __result)
        {
            Player player = Player.Get(hub);
            if (player.TryGetComponent<GhostComponent>(out GhostComponent component))
            {
                if (player.Role == RoleTypeId.Spectator && component.DeadTime == 0f)
                {
                    return;
                }
                float result = player.IsGhost() ? -__result + player.RoleBase.ActiveTime / 15f : __result;
                result += component.DeadTime / 15f;
                if (component.PreviousTeam == targetTeam)
                {
                    result += 3f;
                }
                __result = result;
            }
        }
    }
}
