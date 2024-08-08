using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Extensions;
using HarmonyLib;
using PlayerStatsSystem;

namespace GhostSpectator.Patches
{
    [HarmonyPatch("FriendlyFireHandler", "IsFriendlyFire")]
    internal class FriendlyFirePatch
    {
        internal static void Postfix(ReferenceHub damagedPlayer, DamageHandlerBase handler, ref bool __result)
        {
            AttackerDamageHandler attackHandler = handler as AttackerDamageHandler;
            if (__result && attackHandler.Attacker.Hub.IsGhost() && damagedPlayer.IsGhost())
            {
                __result = false;
            }
        }
    }
}
