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
    [HarmonyPatch(typeof(ImpactEffectsModule), "ServerSendImpactDecal")]
    internal class BulletDecalPatch
    {
        internal static bool Prefix(ImpactEffectsModule __instance)
        {
            return !__instance.Firearm.Owner.IsGhost();
        }
    }
}
