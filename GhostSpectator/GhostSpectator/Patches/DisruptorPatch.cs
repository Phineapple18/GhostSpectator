using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Features.Extensions;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(DisruptorHitregModule), "ServerApplyObstacleDamage")]
    internal class DisruptorPatch
    {
        internal static bool Prefix(DisruptorHitregModule __instance)
        {
            if (__instance.Firearm.Owner.IsGhost())
            {
                return false;
            }
            return true;
        }
    }
}
