using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;
using Object = UnityEngine.Object;

using CustomPlayerEffects;
using GhostSpectator.Features;
using GhostSpectator.Features.Extensions;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp173Events;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.Spectating;
using Respawning.Waves;
using UnityEngine;
using Utils.NonAllocLINQ;
using VoiceChat;

namespace GhostSpectator
{
    public class EventHandler : CustomEventsHandler
    {
        public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
        {
            if (ev.OldRole == RoleTypeId.Scp0492 && (ev.NewRole.RoleTypeId == RoleTypeId.Spectator || ev.Player.IsGhostSpawning()) && deadZombies.Add(ev.Player.ReferenceHub))
            {
                Log.Debug($"Added player {ev.Player.Nickname} to dead zombies list.", Config.Debug);
                return;
            }
            if (!(ev.OldRole == RoleTypeId.Spectator && ev.Player.IsGhostSpawning() || (ev.Player.IsGhostDespawning() || ev.Player.IsGhost()) && ev.NewRole.RoleTypeId == RoleTypeId.Spectator))
            {
                if (deadZombies.Remove(ev.Player.ReferenceHub))
                {
                    Log.Debug($"Removed player {ev.Player.Nickname} from dead zombies list.", Config.Debug);
                }
                deathPositions.Remove(ev.Player);
                if (ev.Player.TryGetGhostComponent(out GhostComponent component) && component.DeadTime != 0f)
                {
                    component.DeadTime = 0f;
                    component.PreviousTeam = ev.Player.ReferenceHub.GetFaction().GetSpawnableTeam();
                    Log.Debug($"Reset dead time for player {ev.Player.Nickname}.", Config.Debug);
                }
            }
            if (ev.Player.IsGhost())
            {
                Ghost.Despawn(ev.Player, ev.NewRole.RoleTypeId, false);
            }
        }

        public override void OnPlayerCuffing(PlayerCuffingEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerDamagingShootingTarget(PlayerDamagingShootingTargetEventArgs ev)
        {
            Player ghost = Ghost.List.FirstOrDefault(p => p.GetGhostComponent().Toys.Contains(ev.ShootingTarget.Base));
            if (ghost != null && !(ev.Player.IsGhost() && ev.Player == ghost))
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerDamagingWindow(PlayerDamagingWindowEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerDeath(PlayerDeathEventArgs ev)
        {
            deathPositions[ev.Player] = ev.OldPosition;
            if (Config.AutoGhostSpawn)
            {
                Timing.CallDelayed(1f, () => Ghost.Spawn(ev.Player, Config.SpawnAtDeathPos));
            }
        }

        public override void OnPlayerDroppingAmmo(PlayerDroppingAmmoEventArgs ev)
        {
            if (ev.Player.IsGhost() && !ev.Player.HasPermissions("gs.item"))
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerDroppingItem(PlayerDroppingItemEventArgs ev)
        {
            if (!ev.Player.IsGhost())
            {
                return;
            }
            if (ev.Item.IsGhostItem())
            {
                ev.IsAllowed = false;
                if ((ev.Item as LanternItem).IsEmitting)
                {
                    if (!ev.Player.HasPermissions("gs.teleport.player"))
                    {
                        ev.Player.SendHint(Translation.NoPermission);
                        return;
                    }
                    IEnumerable<Player> validPlayers = Player.List.Where(p => p.IsAlive && !(p.IsGhost() || p.Role == RoleTypeId.Scp079 || Config.RoleTeleportBlacklist.Contains(p.Role)));
                    if (validPlayers.IsEmpty())
                    {
                        ev.Player.SendHint(Translation.TeleportPlayerFail);
                        Log.Debug($"Player {ev.Player.Nickname} failed to teleport due to missing valid players.", Config.Debug);
                        return;
                    }
                    Player target = validPlayers.ElementAt(random.Next(validPlayers.Count()));
                    ev.Player.Position = target.Position + Vector3.up;
                    ev.Player.SendHint(Translation.TeleportPlayerSuccess.Replace("%playernick%", target.Nickname), 5f);
                    Log.Debug($"Player {ev.Player.Nickname} was successfully teleported to player {target.Nickname}.", Config.Debug);
                    return;
                }
                if (!ev.Player.HasPermissions("gs.teleport.room"))
                {
                    ev.Player.SendHint(Translation.NoPermission);
                    return;
                }
                if (Warhead.IsDetonated)
                {
                    ev.Player.SendHint(Translation.TeleportRoomFail);
                    Log.Debug($"Player {ev.Player.Nickname} failed to teleport, because the warhead is already detonated.", Config.Debug);
                    return;
                }
                IEnumerable<Room> rooms = Room.List.Where(r => r.Name is RoomName.Unnamed or RoomName.Outside);
                Room room = rooms.ElementAt(random.Next(rooms.Count()));
                ev.Player.Position = room.Position + Vector3.up;
                Log.Debug($"Player {ev.Player.Nickname} was successfully teleported to a random room.", Config.Debug);
                return;
            }
            if (!ev.Player.HasPermissions("gs.item"))
            {
                if (ev.Throw)
                {
                    ev.IsAllowed = false;
                    return;
                }
                ev.Player.RemoveItem(ev.Item);
                Log.Debug($"Removed item {ev.Item.Type} from inventory of player {ev.Player.Nickname}.", Config.Debug);
            }
        }

        public override void OnPlayerDying(PlayerDyingEventArgs ev)
        {
            if (ev.Attacker.IsGhost() && ev.Player.IsGhost())
            {
                Duel.Finish(ev.Attacker, ev.Player);
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerUpdatingEffect(PlayerEffectUpdatingEventArgs ev)
        {
            if (ev.Player.IsGhost() && (ev.Effect is PitDeath || (ev.Effect is Ghostly or NightVision) && ev.Intensity == 0))
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerEnteringHazard(PlayerEnteringHazardEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerEnteringPocketDimension(PlayerEnteringPocketDimensionEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerFlippingCoin(PlayerFlippingCoinEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (!(ev.Attacker.IsGhost() || ev.Player.IsGhost()))
            {
                return;
            }
            if (ev.Attacker.IsGhost() ^ ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
                return;
            }
            ev.IsAllowed = ev.Attacker.GetGhostComponent().DuelPartner == ev.Player && ev.Player.GetGhostComponent().DuelPartner == ev.Attacker;
        }

        public override void OnPlayerIdlingTesla(PlayerIdlingTeslaEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerLeavingPocketDimension(PlayerLeavingPocketDimensionEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.Player.Position = Config.SpawnPositions != null ? Config.SpawnPositions.ElementAt(random.Next(Config.SpawnPositions.Count)) : Ghost.DeafultSpawn;
                Log.Debug($"Player {ev.Player.Nickname} exited safely Pocket Dimension as a Ghost.", Config.Debug);
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            if (ev.Player.TryGetGhostComponent(out GhostComponent ghostComponent))
            {
                Object.Destroy(ghostComponent);
                Log.Debug($"Destroyed GhostComponent for player {ev.Player.Nickname}.", Config.Debug);
            }
            if (deadZombies.Remove(ev.Player.ReferenceHub))
            {
                Log.Debug($"Removed player {ev.Player.Nickname} from dead zombies list.", Config.Debug);
            }
            deathPositions.Remove(ev.Player);
        }

        public override void OnPlayerPlacingBlood(PlayerPlacingBloodEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerPlacingBulletHole(PlayerPlacingBulletHoleEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerReceivingVoiceMessage(PlayerReceivingVoiceMessageEventArgs ev)
        {
            if (ev.Player.TryGetGhostComponent(out GhostComponent component) && component.State == GhostState.Spawned)
            {
                if (ev.Sender.IsSCP && component.VoiceChats.Contains("scp"))
                {
                    ev.Message.Channel = VoiceChatChannel.RoundSummary;
                    ev.IsAllowed = true;
                    return;
                }
                if (ev.Sender.Role == RoleTypeId.Spectator && component.VoiceChats.Contains("spectator"))
                {
                    ev.Message.Channel = VoiceChatChannel.RoundSummary;
                    ev.IsAllowed = true;
                    return;
                }
                if (ev.Sender.IsGhost() && component.VoiceChats.Contains("ghost") && Vector3.Distance(ev.Sender.Position, ev.Player.Position) > Config.HearDistance)
                {
                    ev.Message.Channel = VoiceChatChannel.RoundSummary;
                    ev.IsAllowed = true;
                }
            }
        }

        public override void OnPlayerShotWeapon(PlayerShotWeaponEventArgs ev)
        {
            if (ev.Player.IsGhost() && ev.FirearmItem.Base.Modules.ToList().TryGetFirst(m => m is IPrimaryAmmoContainerModule, out ModuleBase module))
            {
                ev.Player.AddAmmo((module as IPrimaryAmmoContainerModule).AmmoType, 1);
            }
        }

        public override void OnPlayerThrowingProjectile(PlayerThrowingProjectileEventArgs ev)
        {
            if (ev.Player.IsGhost() && !ev.Player.HasPermissions("gs.item"))
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerTriggeringTesla(PlayerTriggeringTeslaEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerUncuffing(PlayerUncuffingEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerUsingIntercom(PlayerUsingIntercomEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerUsingItem(PlayerUsingItemEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnPlayerValidatedVisibility(PlayerValidatedVisibilityEventArgs ev)
        {
            if (ev.Target.IsGhost())
            {
                if (ev.Target.TryGetEffect(out Invisible invisible) && invisible.Intensity > 0)
                {
                    ev.IsVisible = false;
                    return;
                }
                if (ev.Player.IsGhost() || ev.Player.Role == RoleTypeId.Overwatch)
                {
                    ev.IsVisible = true;
                    return;
                }
                if (ev.Player.IsAlive || ev.Player.Role == RoleTypeId.None)
                {
                    ev.IsVisible = false;
                    return;
                }
                if (ev.Player.Role == RoleTypeId.Filmmaker)
                {
                    ev.IsVisible = MainClass.Instance.pluginConfig.FilmmakerSeeGhosts;
                    return;
                }
                if (ReferenceHub.TryGetHubNetID((ev.Player.RoleBase as SpectatorRole).SyncedSpectatedNetId, out ReferenceHub spectated))
                {
                    ev.IsVisible = spectated.IsGhost() || MainClass.Instance.pluginConfig.AlwaysSeeGhosts;
                }
            }
        }

        public override void OnScp049ResurrectedBody(Scp049ResurrectedBodyEventArgs ev)
        {
            if (Vector3.Distance(ev.Player.Position, ev.Target.Position) > 5)
            {
                ev.Target.Position = ev.Player.Position + Vector3.up * 0.5f;
            }
        }

        public override void OnScp096AddingTarget(Scp096AddingTargetEventArgs ev)
        {
            if (ev.Target.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnScp173AddingObserver(Scp173AddingObserverEventArgs ev)
        {
            if (ev.Target.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnScp914ProcessingInventoryItem(Scp914ProcessingInventoryItemEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnScp914ProcessingPlayer(Scp914ProcessingPlayerEventArgs ev)
        {
            if (ev.Player.IsGhost())
            {
                ev.IsAllowed = false;
            }
        }

        public override void OnServerExplosionSpawning(ExplosionSpawningEventArgs ev)
        {
            if (ev.Player.IsGhost() && ev.ExplosionType == ExplosionType.Disruptor)
            {
                ev.DestroyDoors = false;
                ev.IsAllowed = false;
            }
        }

        public override void OnServerRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (Player player in Ghost.List)
            {
                Ghost.Despawn(player, player.Role, false);
            }
            Log.Debug("Despawned all Ghosts due to round end.", Config.Debug);
        }

        public override void OnServerWaitingForPlayers()
        {
            Timing.KillCoroutines(Duel.AllPending.Keys.ToArray());
            Duel.AllPending.Clear();
            Duel.Requests.Clear();
        }

        public override void OnWarheadDetonated(WarheadDetonatedEventArgs ev)
        {
            if (Config.DespawnOnDetonation)
            {
                foreach (Player player in Ghost.List)
                {
                    if (!player.HasPermissions("gs.warhead"))
                    {
                        Ghost.Despawn(player);
                    }
                }
                Log.Debug("Despawned all Ghosts, who don't have permission, due to warhead detonation.", Config.Debug);
            }
        }

        internal static HashSet<ReferenceHub> deadZombies = new();
        internal static Dictionary<Player, Vector3> deathPositions = new();
        private readonly System.Random random = new();

        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
