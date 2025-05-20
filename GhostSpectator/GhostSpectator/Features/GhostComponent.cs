using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Random = System.Random;

using AdminToys;
using CustomPlayerEffects;
using GhostSpectator.Features.Extensions;
using InventorySystem.Items;
using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Wrappers;
using LabApi.Features.Permissions;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using PlayerStatsSystem;
using Respawning.Waves;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Features
{
    [DisallowMultipleComponent]
    public class GhostComponent : MonoBehaviour, IInteractionBlocker
    {
        public void Awake()
        {
            player = Player.Get(base.transform.root.gameObject);
            Log.Debug($"Created a {this.GetType().Name} for player {player.Nickname}.", config.Debug);
        }

        public void OnEnable()
        {
            State = GhostState.Spawning;
            int reviveNum = Scp049ResurrectAbility.GetResurrectionsNumber(player.ReferenceHub);
            if (player.Role == RoleTypeId.Spectator)
            {
                DeadTime += player.RoleBase.ActiveTime;
            }
            else
            {
                PreviousTeam = player.ReferenceHub.GetFaction().GetSpawnableTeam();
            }
            player.SetRole(RoleType, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.AssignInventory);
            if (reviveNum > 0)
            {
                Scp049ResurrectAbility.RegisterPlayerResurrection(player.ReferenceHub, reviveNum);
                Log.Debug($"Re-registered resurrection number ({reviveNum}) for player {player.Nickname}.", config.Debug);
            }
            player.InfoArea &= ~PlayerInfoArea.Role & ~PlayerInfoArea.Nickname;
            player.MaxHealth = config.GhostHealth;
            player.Heal(config.GhostHealth);
            player.Position = new(0f, 500f, 0f);
            Timing.CallDelayed(0.1f, () => player.Position = config.SpawnPositions.ElementAt(random.Next(config.SpawnPositions.Count)));
            player.ReferenceHub.interCoordinator.AddBlocker(this);
            ghostItem = player.AddItem(GhostItemType);
            Other.GhostItemList.Add(ghostItem);
            if (player.HasPermissions("gs.noclip"))
            {
                FpcNoclip.PermitPlayer(player.ReferenceHub);
                Log.Debug($"Granted noclip permit to player {player.Nickname}.", config.Debug);
            }
            foreach (string permission in Other.voiceChats)
            {
                if (player.HasPermissions($"gs.autolisten.{permission}"))
                {
                    VoiceChats.Add(permission);
                    Log.Debug($"Enabled autolistening to {permission} for {player.Nickname}.", config.Debug);
                }
            }
            if (config.SsSettingsEnabled && config.SendSettingsOnSpawn)
            {
                SSGhostSpectator.Singleton.ActivateForHub(player.ReferenceHub);
            }
            foreach (Player ply in Ghost.List)
            {
                ply.GetGhostComponent().Toys.ForEach(toy =>
                {
                    toy.netIdentity.AddObserver(player.ReferenceHub.networkIdentity.connectionToClient);
                });
            }
            if (!string.IsNullOrWhiteSpace(translation.SpawnMessage))
            {
                string message = translation.SpawnMessage.Replace("%colour%", config.GhostColor);
                player.SendBroadcast(message, config.SpawnmessageDuration, Broadcast.BroadcastFlags.Normal, true);
            }
            State = GhostState.Spawned;
            Log.Debug($"Enabled {this.GetType().Name} for player {player.Nickname}.", config.Debug);
        }

        public void Update()
        {
            player.CustomInfo = $"<color={config.GhostColor}>{player.DisplayName.Replace("#855439", "#944710")}\n{translation.GhostNickname}</color>";
            player.StaminaRemaining = player.ReferenceHub.playerStats.GetModule<StaminaStat>().MaxValue;
            player.EnableEffect<Ghostly>();
            player.DisableEffect<PitDeath>();
        }

        public void OnDisable()
        {
            State = GhostState.Despawning;
            player.InfoArea |= PlayerInfoArea.Role | PlayerInfoArea.Nickname;
            player.CustomInfo = string.Empty;
            player.DisableAllEffects();
            player.ClearInventory();
            Other.GhostItemList.Remove(ghostItem);
            ghostItem = null;
            if (player.HasPermissions("gs.noclip"))
            {
                FpcNoclip.UnpermitPlayer(player.ReferenceHub);
                Log.Debug($"Revoked noclip permit of player {player.Nickname}.", config.Debug);
            }
            foreach (string permission in Other.voiceChats)
            {
                VoiceChats.Remove(permission);
                Log.Debug($"Disabled listening to {permission} for player {player.Nickname}.", config.Debug);
            }
            Toys.ForEach(toy => Toy.Destroy(player, toy));
            foreach (Player ply in Ghost.List)
            {
                ply.GetGhostComponent().Toys.ForEach(toy =>
                {
                    toy.netIdentity.observers.Remove(player.ReferenceHub.networkIdentity.connectionToClient.connectionId);
                    player.ReferenceHub.networkIdentity.connectionToClient.RemoveFromObserving(toy.netIdentity, false);
                });
            }
            Duel.Abort(player, DuelPartner);
            Duel.TryAbortPrepare(player, out _);
            Duel.TryRemoveRequest(player, out _, false);
            SSGhostSpectator.Singleton?.DeactivateForHub(player.ReferenceHub);
            Log.Debug($"Disabled {this.GetType().Name} for player {player.Nickname}.", config.Debug);
        }

        private Player player;
        private Item ghostItem;

        private readonly Config config = MainClass.Instance.pluginConfig;
        private readonly Translation translation = MainClass.Instance.pluginTranslation;
        private readonly Random random = new();

        public BlockedInteraction BlockedInteractions => BlockedInteraction.GeneralInteractions | BlockedInteraction.BeDisarmed | BlockedInteraction.GrabItems;
        public bool CanBeCleared => !base.enabled;
        internal float DeadTime { get; set; }
        public Player DuelPartner { get; internal set; }
        private ItemType GhostItemType { get; } = ItemType.Lantern;
        internal GhostState State { get; set; }
        internal Team PreviousTeam { get; set; }
        private RoleTypeId RoleType { get; } = RoleTypeId.Tutorial;
        public HashSet<AdminToyBase> Toys { get; } = new();
        internal HashSet<string> VoiceChats { get; } = new();
    }

    internal enum GhostState
    {
        Spawning,
        Spawned,
        Despawning,
        Despawned
    }
}
