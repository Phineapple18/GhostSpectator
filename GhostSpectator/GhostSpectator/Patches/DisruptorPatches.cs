using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Extensions;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(DisruptorHitregModule), "ServerProcessObstacleHit")]
    internal class DisruptorObstaclePatch
    {
        internal static bool Prefix(DisruptorHitregModule __instance, ref float __result)
        {
            if (__instance.Firearm.Owner.IsGhost())
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(DisruptorHitregModule), "ServerPerformSingle")]
    internal class DisruptorSinglePatch
    {
        internal static bool Prefix(DisruptorHitregModule __instance, out float targetDamage)
        {
            targetDamage = 0f;
            return !__instance.Firearm.Owner.IsGhost();
        }
    }
}
