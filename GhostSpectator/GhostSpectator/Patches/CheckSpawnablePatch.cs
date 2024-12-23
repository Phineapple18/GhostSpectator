using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Extensions;
using HarmonyLib;
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
}
