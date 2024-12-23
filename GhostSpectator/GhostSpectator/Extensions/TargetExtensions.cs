using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminToys;
using Mirror;
using PluginAPI.Core;
using UnityEngine;

namespace GhostSpectator.Extensions
{
    internal static class TargetExtensions
    {
        internal static void DestroyShootingTarget(GhostComponent component, AdminToyBase target)
        {
            NetworkServer.Destroy(target.gameObject);
            component.ShootingTargets.Remove(target);
            Log.Debug($"Destroyed shooting target {target.name} with ID {target.netId}.", Config.Debug, PluginName);
        }

        internal static void HandleVisibility(this HashSet<AdminToyBase> targetList)
        {
            foreach (AdminToyBase target in targetList)
            {
                foreach (Player ply in Player.GetPlayers())
                {
                    if (ply.IsGhost())
                    {
                        target.netIdentity.AddObserver(ply.ReferenceHub.netIdentity.connectionToClient);
                    }
                    else
                    {
                        target.netIdentity.RemoveObserver(ply.ReferenceHub.netIdentity.connectionToClient);
                    }
                }
            }
        }

        internal static List<Bounds> ShootingRanges { get; private set; } = new();
        private static Config Config => Plugin.Singleton.pluginConfig;
        private static string PluginName => Plugin.Singleton.pluginHandler.PluginName;
    }
}
