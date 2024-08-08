using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminToys;
using InventorySystem.Items;
using Mirror;
using PluginAPI.Core;
using UnityEngine;

namespace GhostSpectator.Extensions
{
    internal static class OtherExtensions
    {
        internal static void DestroyShootingTarget(GhostComponent component, AdminToyBase target)
        {
            NetworkServer.Destroy(target.gameObject);
            component.ShootingTargets.Remove(target);
            Log.Debug($"Destroyed shooting target {target.name} with ID {target.netId}.", Config.Debug, PluginName);
        }

        public static bool IsGhostItem(this ItemBase item)
        {
            return item != null && GhostItemList.Contains(item);
        }

        internal static readonly Dictionary<string, KeyValuePair<string, string>> voiceChats = new()
        {
            { "scp", new("ListenScpChat", "SCPs") },
            { "dead", new("ListenSpectator", "Spectators") },
            { "ghost", new("ListenGhosts", "Ghosts") }
        };

        internal static HashSet<ItemBase> GhostItemList { get; } = new();
        internal static List<Vector3> SpawnPositions { get; private set; } = new();
        internal static List<Bounds> ShootingRanges { get; private set; } = new();
        private static Config Config => Plugin.Singleton.pluginConfig;
        private static string PluginName => Plugin.Singleton.pluginHandler.PluginName;
    }
}
