using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Extensions;
using HarmonyLib;
using Respawning;

namespace GhostSpectator.Patches
{
    [HarmonyPatch(typeof(RespawnManager), "CheckSpawnable")]
    internal class CheckSpawnablePatch
    {
        internal static void Postfix(ReferenceHub ply, ref bool __result)
        {
            if (ply.IsGhost())
            {
                __result = true;
            }
        }
    }
}
