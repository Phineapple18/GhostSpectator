using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GhostSpectator.Extensions;
using InventorySystem.Items.ToggleableLights.Lantern;
using MapGeneration;
using MEC;
using NWAPIPermissionSystem;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Respawning.Waves;
using UnityEngine;

namespace GhostSpectator
{
    internal class EventHandlers
    {
        [PluginEvent(ServerEventType.PlayerCoinFlip)]
        internal bool OnPlayerCoinFlip(PlayerCoinFlipEvent ev)
        {
            return !ev.Player.IsGhost();
        }

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        internal void OnPlayerChangeRole(PlayerChangeRoleEvent ev)
        {
            if (ev.Player.Role == RoleTypeId.Scp0492 && (ev.NewRole == RoleTypeId.Spectator || ev.Player.IsGhostSpawning()) && deadZombies.Add(ev.Player.ReferenceHub))
            {
                Log.Debug($"Added player {ev.Player.Nickname} to dead zombies list.", config.Debug, pluginName);
                return;
            }
            if (!(ev.Player.Role == RoleTypeId.Spectator && ev.Player.IsGhostSpawning() || (ev.Player.IsGhostDespawning() || ev.Player.IsGhost()) && ev.NewRole == RoleTypeId.Spectator))
            {
                if (deadZombies.Remove(ev.Player.ReferenceHub))
                {
                    Log.Debug($"Removed player {ev.Player.Nickname} from dead zombies list.", config.Debug, pluginName);
                }
                if (ev.Player.TryGetComponent<GhostComponent>(out GhostComponent component) && component.DeadTime != 0f)
                {
                    component.DeadTime = 0f;
                    component.PreviousTeam = ev.Player.ReferenceHub.GetFaction().GetSpawnableTeam();
                    Log.Debug($"Reset dead time for player {ev.Player.Nickname}.", config.Debug, pluginName);
                }
            }
            if (ev.Player.IsGhost())
            {
                GhostExtensions.Despawn(ev.Player, ev.NewRole, false);
            }
        }

        [PluginEvent(ServerEventType.PlayerDamage)]
        internal bool OnPlayerDamage(PlayerDamageEvent ev)
		{
            if (!(ev.Player.IsGhost() || ev.Target.IsGhost()))
			{
                return true;
			}
            if (ev.Player.IsGhost() ^ ev.Target.IsGhost())
		    {
                return false;
			}
            return ev.Player.GetComponent<GhostComponent>().DuelPartner == ev.Target && ev.Target.GetComponent<GhostComponent>().DuelPartner == ev.Player;
        }

        [PluginEvent(ServerEventType.PlayerDamagedShootingTarget)]
        internal bool OnPlayerDamagedShootingTarget(PlayerDamagedShootingTargetEvent ev)
        {
            return !ev.Player.IsGhost() || ev.Player.GetComponent<GhostComponent>().ShootingTargets.Contains(ev.ShootingTarget);
        }

        [PluginEvent(ServerEventType.PlayerDamagedWindow)]
        internal bool OnPlayerDamagedWindow(PlayerDamagedWindowEvent ev)
		{
			return !ev.Player.IsGhost();
		}        

        [PluginEvent(ServerEventType.PlayerDropItem)]
        internal bool OnPlayerDropItem(PlayerDropItemEvent ev)
		{
            if (ev.Player.IsGhost())
            {
                if (ev.Item.IsGhostItem())
                {
                    if ((ev.Item as LanternItem).IsEmittingLight)
                    {
                        IEnumerable<Player> validPlayers = Player.GetPlayers().Where(p => p.IsAlive && !(p.IsGhost() || p.Role == RoleTypeId.Scp079 || config.RoleTeleportBlacklist.Contains(p.Role)));
                        if (validPlayers.IsEmpty())
                        {
                            ev.Player.ReceiveHint(translation.TeleportPlayerFail);
                            Log.Debug($"Player {ev.Player.Nickname} failed to teleport due to missing valid players.", config.Debug, pluginName);
                        }
                        else
                        {
                            Player target = validPlayers.ElementAt(random.Next(validPlayers.Count()));
                            ev.Player.Position = target.Position + Vector3.up;
                            ev.Player.ReceiveHint(translation.TeleportPlayerSuccess.Replace("%playernick%", target.Nickname), 5f);
                            Log.Debug($"Player {ev.Player.Nickname} was successfully teleported to player {target.Nickname}.", config.Debug, pluginName);
                        }
                    }
                    else
                    {
                        if (Warhead.IsDetonated)
                        {
                            ev.Player.ReceiveHint(translation.TeleportRoomFail, 5f);
                            Log.Debug($"Player {ev.Player.Nickname} failed to teleport, because the warhead is already detonated.", config.Debug, pluginName);
                        }
                        else
                        {
                            IEnumerable<RoomIdentifier> rooms = RoomIdentifier.AllRoomIdentifiers.Where(r => r.Name is RoomName.Unnamed or RoomName.Outside);
                            RoomIdentifier room = rooms.ElementAt(random.Next(rooms.Count()));
                            ev.Player.Position = room.transform.position + Vector3.up;
                            Log.Debug($"Player {ev.Player.Nickname} was successfully teleported to a random room.", config.Debug, pluginName);
                        }

                    }
                    return false;
                }
                if (!ev.Player.CheckPermission("gs.item"))
				{
                    ev.Player.RemoveItem(ev.Item);
                    Log.Debug($"Removed item {ev.Item.ItemTypeId} from inventory of player {ev.Player.Nickname}.", config.Debug, pluginName);
                }
            }
            return true;
		}

        [PluginEvent(ServerEventType.PlayerDying)]
        internal bool OnPlayerDying(PlayerDyingEvent ev)
        {
            if (ev.Attacker.IsGhost() && ev.Player.IsGhost())
            {
                DuelExtensions.FinishDuel(ev.Attacker, ev.Player);
                return false;
            }
            return true;
        }

        [PluginEvent(ServerEventType.PlayerEnterPocketDimension)]
		internal bool OnPlayerEnterPocketDimension(PlayerEnterPocketDimensionEvent ev)
		{
			return !ev.Player.IsGhost();
		}

		[PluginEvent(ServerEventType.PlayerExitPocketDimension)]
		internal bool OnPlayerExitPocketDimension(PlayerExitPocketDimensionEvent ev)
		{
			if (ev.Player.IsGhost())
			{
                ev.Player.Position = OtherExtensions.SpawnPositions.ElementAt(random.Next(OtherExtensions.SpawnPositions.Count));
                Log.Debug($"Player {ev.Player.Nickname} exited safely Pocket Dimension as a Ghost.", config.Debug, pluginName);
                return false;
			}
			return true;
		}

        [PluginEvent(ServerEventType.PlayerHandcuff)]
        internal bool OnPlayerHandcuff(PlayerHandcuffEvent ev)
        {
            return !ev.Player.IsGhost();
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        internal void OnPlayerLeft(PlayerLeftEvent ev)
        {
            if (ev.Player.TryGetComponent<GhostComponent>(out GhostComponent ghostComponent))
            {
                UnityEngine.Object.Destroy(ghostComponent);
                Log.Debug($"Destroyed GhostComponent for player {ev.Player.Nickname}.", config.Debug, pluginName);
            }
            if (deadZombies.Remove(ev.Player.ReferenceHub))
            {
                Log.Debug($"Removed player {ev.Player.Nickname} from dead zombies list.", config.Debug, pluginName);
            }
        }

        [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
        internal bool OnPlayerRemoveHandcuffs(PlayerRemoveHandcuffsEvent ev)
		{
            return !ev.Player.IsGhost();
        }

        [PluginEvent(ServerEventType.PlayerSpawn)]
        internal void OnPlayerSpawn(PlayerSpawnEvent ev)
        {
            if (ev.Role == RoleTypeId.Scp0492 && ev.Player.TemporaryData.Contains("ZombiePosition"))
            {
                Tuple<Vector3> position = (Tuple<Vector3>)ev.Player.TemporaryData.StoredData["ZombiePosition"];
                ev.Player.Position = position.Item1 + Vector3.up;
                Log.Debug($"Corrected zombie position for player {ev.Player.Nickname}.", config.Debug, pluginName);
            }
            ev.Player.TemporaryData.Remove("ZombiePosition");
        }

        [PluginEvent(ServerEventType.PlayerThrowItem)]
		internal bool OnPlayerThrowItem(PlayerThrowItemEvent ev)
		{
			return !ev.Player.IsGhost() || ev.Player.CheckPermission("gs.item") && !ev.Item.IsGhostItem();
        }

		[PluginEvent(ServerEventType.PlayerThrowProjectile)]
		internal bool OnPlayerThrowProjectile(PlayerThrowProjectileEvent ev)
		{
			return !ev.Thrower.IsGhost() || ev.Thrower.CheckPermission("gs.item") && !ev.Item.IsGhostItem();
        }

        [PluginEvent(ServerEventType.PlayerUseItem)]
        internal bool OnPlayerUseItem(PlayerUseItemEvent ev)
        {
            return !ev.Player.IsGhost();
        }

        [PluginEvent(ServerEventType.PlayerUsingIntercom)]
		internal bool OnPlayerUsingIntercom(PlayerUsingIntercomEvent ev)
		{
			return !ev.Player.IsGhost();
		}

        [PluginEvent(ServerEventType.RoundEnd)]
        internal void OnRoundEnd(RoundEndEvent ev)
        {
            foreach (Player player in GhostExtensions.GhostPlayerList)
            {
                GhostExtensions.Despawn(player, player.Role, false);
            }
            Log.Debug("Despawned all Ghosts due to round end.", config.Debug, pluginName);
        }

        [PluginEvent(ServerEventType.Scp049ResurrectBody)]
        internal void OnScp049ResurrectBody(Scp049ResurrectBodyEvent ev)
        {
            ev.Target.TemporaryData.Add("ZombiePosition", new Tuple<Vector3>(ev.Body.CenterPoint.position));
        }

        [PluginEvent(ServerEventType.Scp096AddingTarget)]
		internal bool OnScp096AddingTarget(Scp096AddingTargetEvent ev)
		{
			return !ev.Target.IsGhost();
		}

		[PluginEvent(ServerEventType.Scp173NewObserver)]
		internal bool OnScp173NewObserver(Scp173NewObserverEvent ev)
		{
			return !ev.Target.IsGhost();
		}

		[PluginEvent(ServerEventType.Scp914ProcessPlayer)]
		internal bool OnScp914ProcessPlayer(Scp914ProcessPlayerEvent ev)
		{
			return !ev.Player.IsGhost();
		}

		[PluginEvent(ServerEventType.Scp914UpgradeInventory)]
		internal bool OnScp914UpgradeInventory(Scp914UpgradeInventoryEvent ev)
		{
			return !ev.Player.IsGhost();
		}

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        internal void OnWaitingForPlayers()
        {
            Timing.KillCoroutines(DuelExtensions.DuelCoroutines.Keys.ToArray());
            DuelExtensions.DuelCoroutines.Clear();
            DuelExtensions.DuelRequests.Clear();
        }

        [PluginEvent(ServerEventType.WarheadDetonation)]
		internal void OnWarheadDetonation()
		{
			if (config.DespawnOnDetonation)
			{
				foreach (Player player in from p in GhostExtensions.GhostPlayerList where !p.CheckPermission("gs.warhead") select p)
				{
                    GhostExtensions.Despawn(player);
				}
				Log.Debug("Despawned all Ghosts, who don't have permission, due to warhead detonation.", config.Debug, pluginName);
			}
		}

        internal static HashSet<ReferenceHub> deadZombies = new();

        private readonly System.Random random = new();
        
        private readonly Config config = Plugin.Singleton.pluginConfig;

        private readonly Translation translation = Plugin.Singleton.pluginTranslation;

        private readonly string pluginName = Plugin.Singleton.pluginHandler.PluginName;
    }
}
