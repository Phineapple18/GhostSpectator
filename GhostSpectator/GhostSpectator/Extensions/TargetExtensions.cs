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
        internal static AdminToyBase CreateShootingTarget(Player player, AdminToyBase targetBase, ArraySegment<string> arguments)
        {
            AdminToyBase target = UnityEngine.Object.Instantiate<AdminToyBase>(targetBase);
            target.netIdentity.visible = Visibility.ForceHidden;
            target.OnSpawned(player.ReferenceHub, arguments);
            target.netIdentity.AddObserver(player.ReferenceHub.netIdentity.connectionToClient);
            player.GetComponent<GhostComponent>().ShootingTargets.Add(target);
            return target;
        }

        internal static void DestroyShootingTarget(Player player, AdminToyBase target)
        {
            target.netIdentity.visible = Visibility.Default;
            NetworkServer.Destroy(target.gameObject);
            player.GetComponent<GhostComponent>().ShootingTargets.Remove(target);
            Log.Debug($"Destroyed shooting target {target.name} with ID {target.netId}.", Config.Debug, PluginName);
        }

        internal static List<Bounds> ShootingRanges { get; private set; } = new();
        private static Config Config => Plugin.Singleton.pluginConfig;
        private static string PluginName => Plugin.Singleton.pluginHandler.PluginName;
    }
}
