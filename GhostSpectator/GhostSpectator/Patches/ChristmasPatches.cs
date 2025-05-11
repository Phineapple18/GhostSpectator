using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Features.Extensions;
using HarmonyLib;
using InventorySystem.Items.FlamingoTapePlayer;
using PlayerRoles.PlayableScps.Scp1507;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(Scp1507Spawner), "ValidatePlayer")]
    internal class FlamingoSpawnablePatch
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
}
