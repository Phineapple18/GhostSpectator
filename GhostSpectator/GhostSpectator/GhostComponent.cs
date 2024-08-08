using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminToys;
using CustomPlayerEffects;
using GhostSpectator.Extensions;
using InventorySystem.Items;
using MEC;
using NWAPIPermissionSystem;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using PluginAPI.Core;
using UnityEngine;

namespace GhostSpectator
{
    [DisallowMultipleComponent]
    public class GhostComponent : MonoBehaviour, IInteractionBlocker
    {
        public void Awake()
        {
            player = Player.Get(ReferenceHub.GetHub(base.transform.root.gameObject));
            Log.Debug($"Created a {this.GetType().Name} for player {player.Nickname}.", config.Debug, pluginName);
        }

        public void OnEnable()
        {
            int reviveNum = Scp049ResurrectAbility.GetResurrectionsNumber(player.ReferenceHub);
            player.ReferenceHub.roleManager.ServerSetRole(this.RoleType, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.AssignInventory);
            if (reviveNum > 0)
            {
                Scp049ResurrectAbility.RegisterPlayerResurrection(player.ReferenceHub, reviveNum);
                Log.Debug($"Re-registered resurrection number ({reviveNum}) for player {player.Nickname}.", config.Debug, pluginName);
            }
            player.PlayerInfo.IsRoleHidden = true;
            player.PlayerInfo.IsNicknameHidden = true;
            player.Health = config.GhostHealth;
            player.ReferenceHub.interCoordinator.AddBlocker(this);
            player.Position = OtherExtensions.SpawnPositions.ElementAt(new System.Random().Next(OtherExtensions.SpawnPositions.Count));
            Timing.CallDelayed(0.5f, delegate ()
            {
                player.ReferenceHub.transform.localScale = 0.1f * Vector3.one;
                foreach (Player ply in Player.GetPlayers())
                {
                    Plugin.setSize.Invoke(null, new object[] { player.ReferenceHub.networkIdentity, ply.Connection });
                }
            });
            if (player.CheckPermission("gs.noclip"))
            {
                FpcNoclip.PermitPlayer(player.ReferenceHub);
                Log.Debug($"Granted noclip permit for player {player.Nickname}.", config.Debug, pluginName);
            }
            foreach (var permission in OtherExtensions.voiceChats)
            {
                if (player.CheckPermission($"gs.autolisten.{permission.Key}"))
                {
                    player.TemporaryData.Add(permission.Value.Key, "1");
                    Log.Debug($"Enabled autolistening to {permission.Value.Value} for player {player.Nickname}.", config.Debug, pluginName);
                }
            }          
            if (config.TeleportItem != ItemType.None)
            {
                ghostItem = player.AddItem(config.TeleportItem);
                OtherExtensions.GhostItemList.Add(player.GetGhostComponent().ghostItem);
                Log.Debug($"Ghost item {config.TeleportItem} has been given to player {player.Nickname}.", config.Debug, pluginName);
            }
            if (!string.IsNullOrWhiteSpace(translation.SpawnMessage))
            {
                string message = translation.SpawnMessage.Replace("%colour%", config.GhostColor).Replace("%teleportitem%", config.TeleportItem.ToString());
                player.SendBroadcast(message, config.SpawnmessageDuration, Broadcast.BroadcastFlags.Normal, true);
            }
            player.TemporaryData.StoredData[GhostExtensions.dataName] = "spawned";
            Log.Debug($"Enabled {this.GetType().Name} for player {player.Nickname}.", config.Debug, pluginName);
        }

        public void Update()
        {
            player.CustomInfo = $"<color={config.GhostColor}>{player.DisplayNickname.Replace("#855439", "#944710")}\n{translation.GhostNickname}</color>";
            player.EffectsManager.EnableEffect<Scp207>();
            player.EffectsManager.EnableEffect<Ghostly>();
        }

        public void OnDisable()
        {
            player.TemporaryData.StoredData[GhostExtensions.dataName] = "despawning";
            player.PlayerInfo.IsRoleHidden = false;
            player.PlayerInfo.IsNicknameHidden = false;
            player.CustomInfo = string.Empty;
            player.Health = player.MaxHealth;
            player.EffectsManager.DisableAllEffects();
            player.ClearInventory();
            if (player.CheckPermission("gs.noclip"))
            {
                FpcNoclip.UnpermitPlayer(player.ReferenceHub);
                Log.Debug($"Revoked noclip permit for player {player.Nickname}.", config.Debug, pluginName);
            }
            foreach (var permission in OtherExtensions.voiceChats)
            {
                player.TemporaryData.Remove(permission.Key);
                Log.Debug($"Disabled listening to {permission.Value.Value} for player {player.Nickname}.", config.Debug, pluginName);
            }
            if (ghostItem != null)
            {
                OtherExtensions.GhostItemList.Remove(player.GetGhostComponent().ghostItem);
                player.GetGhostComponent().ghostItem = null;
                Log.Debug($"Destroyed ghost item for player {player.Nickname}.", config.Debug, pluginName);
            }
            DuelExtensions.AbortDuel(player, DuelPartner);
            DuelExtensions.TryAbortDuelPreparation(player, out _);
            DuelExtensions.TryRemoveDuelRequest(player, out _, false);
            foreach (AdminToyBase target in this.ShootingTargets)
            {
                OtherExtensions.DestroyShootingTarget(this, target);
            }
            Log.Debug($"Disabled {this.GetType().Name} for player {player.Nickname}.", config.Debug, pluginName);
        }

        private Player player;

        private ItemBase ghostItem;

        private readonly Config config = Plugin.Singleton.pluginConfig;

        private readonly Translation translation = Plugin.Singleton.pluginTranslation;

        private readonly string pluginName = Plugin.Singleton.pluginHandler.PluginName;

        public BlockedInteraction BlockedInteractions => BlockedInteraction.BeDisarmed | BlockedInteraction.GeneralInteractions | BlockedInteraction.GrabItems;
        public bool CanBeCleared => !base.enabled;
        public Player DuelPartner { get; internal set; }
        private RoleTypeId RoleType { get; } = RoleTypeId.Tutorial;
        public HashSet<AdminToyBase> ShootingTargets { get; } = new();
    }
}
