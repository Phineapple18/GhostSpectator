using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Extensions;
using HarmonyLib;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.IsInIdleRange), new[] { typeof(ReferenceHub) })]
    internal class TeslaIdleRangePatch
    {
        internal static void Postfix(ReferenceHub player, ref bool __result)
        {
            if (player.IsGhost())
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.PlayerInRange))]
    internal class TeslaPlayerInRangePatch
    {
        internal static void Postfix(ReferenceHub player, ref bool __result)
        {
            if (player.IsGhost())
            {
                __result = false;
            }
        }
    }
}
