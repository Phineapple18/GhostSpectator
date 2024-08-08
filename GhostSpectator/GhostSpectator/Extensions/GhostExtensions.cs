using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlayerRoles;
using PluginAPI.Core;

namespace GhostSpectator.Extensions
{
    public static class GhostExtensions
    {
        public static void Spawn(Player player)
        {
            player.TemporaryData.Add(dataName, "spawning");
            try
            {
                player.GetGhostComponent().enabled = true;
            }
            catch (Exception)
            {
                player.GameObject.AddComponent<GhostComponent>();
            }
            Log.Debug($"Player {player.Nickname} has been turned into Ghost.", Config.Debug, PluginName);
        }

        public static void Despawn(Player player, bool toSpectator = true)
        {
            player.GetGhostComponent().enabled = false;
            if (toSpectator)
            {
                player.SetRole(RoleTypeId.Spectator);
            }
            player.TemporaryData.Remove(dataName);
            Log.Debug($"Player {player.Nickname} is no longer a Ghost{(toSpectator ? " and changed role to Spectator" : "")}.", Config.Debug, PluginName);
        }

        public static bool IsGhost(this ReferenceHub hub)
        {
            return Player.Get(hub).IsGhost();
        }

        public static bool IsGhost(this Player player)
        {
            return player != null && player.TemporaryData.StoredData.Any(kvp => kvp.Key == dataName && (string)kvp.Value == "spawned");
        }

        public static bool IsGhostSpawning(this Player player)
        {
            return player != null && player.TemporaryData.StoredData.Any(kvp => kvp.Key == dataName && (string)kvp.Value == "spawning");
        }

        public static bool IsGhostDespawning(this Player player)
        {
            return player != null && player.TemporaryData.StoredData.Any(kvp => kvp.Key == dataName && (string)kvp.Value == "despawning");
        }

        public static GhostComponent GetGhostComponent(this Player player)
        {
            return player.GameObject.GetComponent<GhostComponent>();
        }

        public const string dataName = "IsGhostSpectator";

        public static IEnumerable<Player> GhostPlayerList => Player.GetPlayers().Where(p => p.IsGhost());
        private static Config Config => Plugin.Singleton.pluginConfig;
        private static string PluginName => Plugin.Singleton.pluginHandler.PluginName;
    }
}
