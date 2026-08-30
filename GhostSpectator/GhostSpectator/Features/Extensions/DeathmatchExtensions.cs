using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CustomPlayerEffects;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;
using Utils.NonAllocLINQ;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Features.Extensions
{
    internal static class DeathmatchExtensions
    {
        internal static void JoinDeathmatch(this Player player)
        {
            if (!player.IsGhost())
            {
                GhostExtensions.SpawnGhost(player, false);
            }
            DeathmatchPlayers.Add(player);
            PendingDeathmatch.Add(player, Timing.RunCoroutine(PrepareForDeathmatch(player)));
        }

        internal static void LeaveDeathmatch(this Player player, bool isRespawn)
        {
            DeathmatchPlayers.Remove(player);
            if (PendingDeathmatch.ContainsKey(player))
            {
                Timing.KillCoroutines(PendingDeathmatch[player]);
                PendingDeathmatch.Remove(player);
            }
            if (!isRespawn)
            {
                player.Position = Config.SpawnPositions?.ElementAt(random.Next(Config.SpawnPositions.Count)) ?? GhostExtensions.DeafultSpawn;
                player.DisableEffect<FogControl>();
                player.ClearInventory();
                player.GetGhostComponent().ghostItem = player.AddItem(ItemType.Lantern);
                GhostExtensions.GhostItemList.Add(player.GetGhostComponent().ghostItem);
            }
        }

        internal static IEnumerator<float> PrepareForDeathmatch(Player player)
        {
            Timing.CallDelayed(Timing.WaitForOneFrame, () => player.Position = spawnposition);
            GhostExtensions.GhostItemList.Remove(player.GetGhostComponent().ghostItem);
            player.ClearInventory();
            player.EnableEffect<Ensnared>(1, 8f);
            player.EnableEffect<Invisible>(1, 8f);
            player.EnableEffect<Blindness>(100, 7f);
            player.EnableEffect<SpawnProtected>(1, 15f);
            player.SendHint(Translation.DeathmatchPrepare, 3);
            Log.Debug($"Preparing player {player.Nickname} for deathmatch.", Config.Debug);
            yield return Timing.WaitForSeconds(3f);
            int i = 5;
            while (i > 0)
            {
                player.SendHint(i.ToString());
                i--;
                yield return Timing.WaitForSeconds(1f);
            }
            player.EnableEffect<FogControl>(Config.FogType);
            player.SendHint(Translation.DeathmatchJoined);
            Config.DeathmatchItems.ForEach<ItemType>(i =>
            {
                player.AddItem(i);
            });
            PendingDeathmatch.Remove(player);
            Log.Debug($"Player {player.Nickname} has joined deathmatch.", Config.Debug);
            yield break;
        }

        public static bool IsInDeathmatch(this ReferenceHub hub)
        {
            return Player.Get(hub).IsInDeathmatch();
        }

        public static bool IsInDeathmatch(this Player player)
        {
            return player != null && DeathmatchPlayers.Contains(player) || PendingDeathmatch.ContainsKey(player);
        }


        private static Vector3 spawnposition = Door.List.First(d => d.DoorName == DoorName.Lcz173Gate).Position + 2 * Vector3.up;
        public static bool DeathmatchEnabled = false;

        public static Dictionary<Player, CoroutineHandle> PendingDeathmatch { get; } = new();
        internal static List<Player> DeathmatchPlayers { get; } = new();
        private static Config Config => MainClass.Instance.pluginConfig;
        private static Translation Translation => MainClass.Instance.pluginTranslation;

        private static readonly System.Random random = new();
    }
}
