using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using Respawning.Waves;
using UnityEngine;

namespace GhostSpectator.Features.Extensions
{
    public static class Ghost
    {
        public static void Spawn(Player player, bool deathPosition)
        {
            try
            {
                player.GetGhostComponent().enabled = true;
            }
            catch (NullReferenceException)
            {
                player.GameObject.AddComponent<GhostComponent>();
            }
            if (deathPosition && EventHandler.deathPositions.TryGetValue(player, out Vector3 position))
            {
                position += Vector3.up;
                Log.Debug($"Found death position of player {player.Nickname}.", Config.Debug);
            }
            else
            {
                position = Config.SpawnPositions != null ? Config.SpawnPositions.ElementAt(random.Next(Config.SpawnPositions.Count)) : DeafultSpawn;
            }
            Timing.CallDelayed(0.1f, () => player.Position = position);
            Log.Debug($"Player {player.Nickname} has been turned into Ghost.", Config.Debug);
        }

        public static void Despawn(Player player, RoleTypeId newRole = RoleTypeId.Spectator, bool forceSpectator = true)
        {
            GhostComponent component = player.GetGhostComponent();
            component.enabled = false;
            if (newRole == RoleTypeId.Spectator)
            {
                component.DeadTime += player.RoleBase.ActiveTime;
            }
            else
            {
                component.DeadTime = 0f;
                component.PreviousTeam = player.ReferenceHub.GetFaction().GetSpawnableTeam();
            }
            if (forceSpectator)
            {
                player.SetRole(RoleTypeId.Spectator);
            }
            component.State = GhostState.Despawned;
            Log.Debug($"Player {player.Nickname} is no longer a Ghost{(forceSpectator ? " and has changed role to Spectator" : "")}.", Config.Debug);
        }

        public static GhostComponent GetGhostComponent(this Player player)
        {
            return player.ReferenceHub.GetGhostComponent();
        }

        public static GhostComponent GetGhostComponent(this ReferenceHub hub)
        {
            return hub.GetComponent<GhostComponent>();
        }

        public static bool TryGetGhostComponent(this Player player, out GhostComponent component)
        {
            if (player != null && player.ReferenceHub.TryGetComponent(out component))
            {
                return true;
            }
            component = null;
            return false;
        }

        public static bool TryGetGhostComponent(this ReferenceHub hub, out GhostComponent component)
        {
            if (hub != null && hub.TryGetComponent(out component))
            {
                return true;
            }
            component = null;
            return false;
        }

        public static bool IsGhost(this Player player)
        {
            return player?.ReferenceHub.IsGhost() ?? false;
        }

        public static bool IsGhost(this ReferenceHub hub)
        {
            return hub != null && hub.TryGetComponent(out GhostComponent component) && component.State == GhostState.Spawned;
        }

        public static bool IsGhostSpawning(this Player player)
        {
            return player?.ReferenceHub.IsGhostSpawning() ?? false;
        }

        public static bool IsGhostSpawning(this ReferenceHub hub)
        {
            return hub != null && hub.TryGetComponent(out GhostComponent component) && component.State == GhostState.Spawning;
        }

        public static bool IsGhostDespawning(this Player player)
        {
            return player?.ReferenceHub.IsGhostDespawning() ?? false;
        }

        public static bool IsGhostDespawning(this ReferenceHub hub)
        {
            return hub != null && hub.TryGetComponent(out GhostComponent component) && component.State == GhostState.Despawning;
        }

        public static IEnumerable<Player> List => Player.List.Where(p => p.IsGhost());
        internal static Vector3 DeafultSpawn { get; } = new(9f, 302f, 1f);

        private static Config Config => MainClass.Instance.pluginConfig;
        private static readonly System.Random random = new();
    }
}
