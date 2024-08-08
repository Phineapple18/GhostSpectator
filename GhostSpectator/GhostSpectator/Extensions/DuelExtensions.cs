using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MEC;
using PluginAPI.Core;

namespace GhostSpectator.Extensions
{
    internal static class DuelExtensions
    {
        internal static void AbandonDuel(this Player player, Player opponent)
        {
            player.GetGhostComponent().DuelPartner = null;
            opponent.GetGhostComponent().DuelPartner = null;
            player.Health = Config.GhostHealth;
            opponent.Health = Config.GhostHealth;
            opponent.ReceiveHint(Translation.DuelAbandoned.Replace("%playernick%", player.Nickname), 5);
            Log.Debug($"Duel has been abandoned by player {player.Nickname}.", Config.Debug, PluginName);
        }

        internal static void AbortDuel(Player player, Player opponent)
        {
            if (opponent != null)
            {
                player.GetGhostComponent().DuelPartner = null;
                opponent.GetGhostComponent().DuelPartner = null;
                player.ReceiveHint(Translation.DuelAborted.Replace("%playernick%", opponent.Nickname), 5);
                opponent.ReceiveHint(Translation.DuelAborted.Replace("%playernick%", player.Nickname), 5);
                Log.Debug($"Duel has been aborted for players {player.Nickname} and {opponent.Nickname}.", Config.Debug, PluginName);
            }
        }        

        internal static void AcceptDuel(this Player receiver, Player sender, List<Player> allSenders)
        {
            allSenders.Remove(sender);
            DuelRequests.Remove(sender);
            foreach (Player player in allSenders)
            {
                RejectDuel(receiver, player);
            }
            string coroutineName = $"duel_{sender.Nickname}({sender.PlayerId})_{receiver.Nickname}({receiver.PlayerId})";
            Log.Debug($"Player {receiver.Nickname} has accepted duel request from player {sender.Nickname}.", Config.Debug, PluginName);
            DuelCoroutines.Add(Timing.RunCoroutine(PrepareDuel(sender, receiver, coroutineName), coroutineName), new(sender, receiver));
        }

        internal static void FinishDuel(Player winner, Player loser)
        {
            winner.GetGhostComponent().DuelPartner = null;
            loser.GetGhostComponent().DuelPartner = null;
            winner.Health = Config.GhostHealth;
            loser.Health = Config.GhostHealth;
            winner.ReceiveHint(Translation.DuelWon.Replace("%playernick%", loser.Nickname), 5);
            loser.ReceiveHint(Translation.DuelLost.Replace("%playernick%", winner.Nickname), 5);
            Log.Debug($"Player {winner.Nickname} won a duel against player {loser.Nickname}.", Config.Debug, PluginName);
        }

        private static IEnumerator<float> PrepareDuel(Player player1, Player player2, string coroutineName)
        {
            player1.ReceiveHint(Translation.DuelPrepare, 5);
            player2.ReceiveHint(Translation.DuelPrepare, 5);
            Log.Debug($"Preparing duel for players {player1.Nickname} and {player2.Nickname}.", Config.Debug, PluginName);
            yield return Timing.WaitForSeconds(5f);
            int i = 5;
            while (i > 0)
            {
                player1.ReceiveHint(i.ToString());
                player2.ReceiveHint(i.ToString());
                i--;
                yield return Timing.WaitForSeconds(1f);
            }
            player1.GetGhostComponent().DuelPartner = player2;
            player2.GetGhostComponent().DuelPartner = player1;
            player1.ReceiveHint(Translation.DuelStarted, 5);
            player2.ReceiveHint(Translation.DuelStarted, 5);
            DuelCoroutines.Remove(DuelCoroutines.Keys.First(c => c.Tag == coroutineName));
            Log.Debug($"Started duel for players {player1.Nickname} and {player2.Nickname}.", Config.Debug, PluginName);
            yield break;
        }

        internal static void RejectDuel(this Player receiver, Player sender)
        {
            DuelRequests.Remove(sender);
            sender.ReceiveHint(Translation.DuelRequestRejected.Replace("%playernick%", receiver.Nickname), 5);
            Log.Debug($"Player {receiver.Nickname} rejected duel request from player {sender.Nickname}.", Config.Debug, PluginName);
        }

        internal static void RequestDuel(this Player sender, Player receiver, Player existingReceiver = null)
        {
            int randInt = new Random().Next();
            receiver.ReceiveHint(Translation.DuelRequestReceived.Replace("%playernick%", sender.Nickname).Replace("%time%", Config.DuelRequestTime.ToString()), 7);
            if (existingReceiver == null)
            {
                DuelRequests.Add(sender, new(receiver, randInt));
                Log.Debug($"Players {sender.Nickname} and {receiver.Nickname} have been added to duel request list.", Config.Debug, PluginName);
            }
            else
            {
                DuelRequests[sender] = new(receiver, randInt);
                existingReceiver.ReceiveHint(Translation.DuelRequestCancelled.Replace("%playernick%", sender.Nickname), 5);
                Log.Debug($"Player {receiver.Nickname} has replaced existing receiver {existingReceiver.Nickname} for sender {sender.Nickname}.", Config.Debug, PluginName);
            }
            Timing.CallDelayed(Config.DuelRequestTime, delegate ()
            {
                if (DuelRequests.Any(kvp => kvp.Key == sender && kvp.Value.Item1 == receiver && kvp.Value.Item2 == randInt) && !sender.HasPendingDuel())
                {
                    DuelRequests.Remove(sender);
                    sender.ReceiveHint(Translation.DuelRequestExpired.Replace("%playernick%", receiver.Nickname), 5);
                    Log.Debug($"Duel request from player {sender.Nickname} to player {receiver.Nickname} has expired.", Config.Debug, PluginName);
                }
            });
        }

        internal static bool TryAbortDuelPreparation(Player player, out string nickname)
        {
            if (!player.HasPendingDuel())
            {
                nickname = string.Empty;
                return false;
            }
            CoroutineHandle pendingDuel = DuelCoroutines.Keys.First(c => c.Tag.Contains($"{player.Nickname}({player.PlayerId})"));
            Tuple<Player, Player> duelPair = DuelCoroutines[pendingDuel];
            Player opponent = player == duelPair.Item1 ? duelPair.Item2 : duelPair.Item1;
            opponent.ReceiveHint(Translation.DuelAbandoned.Replace("%playernick%", player.Nickname), 5);
            Timing.KillCoroutines(pendingDuel.Tag);
            DuelCoroutines.Remove(pendingDuel);
            nickname = opponent.Nickname;
            Log.Debug($"Duel preparation has been aborted for players {player.Nickname} and {opponent.Nickname}.", Config.Debug, PluginName);
            return true;
        }

        internal static bool TryRemoveDuelRequest(Player player, out string nickname, bool onlySentRequest = true)
        {
            nickname = string.Empty;
            bool result = false;
            if (DuelRequests.ContainsKey(player))
            {
                Player opponent = DuelRequests[player].Item1;
                DuelRequests.Remove(player);
                opponent.ReceiveHint(Translation.DuelRequestCancelled.Replace("%playernick%", player.Nickname), 5);
                nickname = opponent.Nickname;
                result = true;
                Log.Debug($"Duel request from player {player.Nickname} has been removed.", Config.Debug, PluginName);
            }
            if (!onlySentRequest && DuelRequests.Values.Any(p => p.Item1 == player))
            {
                List<Player> allSenders = (from kvp in DuelRequests where kvp.Value.Item1 == player select kvp.Key).ToList();
                foreach (Player sender in allSenders)
                {
                    DuelRequests.Remove(sender);
                    sender.ReceiveHint(Translation.DuelRequestCancelled.Replace("%playernick%", player.Nickname), 5);
                    Log.Debug($"Duel request from player {sender.Nickname} to player {player.Nickname} has been removed.", Config.Debug, PluginName);
                }
                result = true;
            }
            return result;
        }

        public static bool HasPendingDuel(this Player player)
        {
            CoroutineHandle pendingDuel = DuelCoroutines.Keys.FirstOrDefault(c => c.Tag.Contains(player.Nickname));
            return pendingDuel.IsRunning;
        }

        public static Dictionary<CoroutineHandle, Tuple<Player, Player>> DuelCoroutines { get; } = new();
        public static Dictionary<Player, Tuple<Player, int>> DuelRequests { get; } = new();
        private static Config Config => Plugin.Singleton.pluginConfig;
        private static Translation Translation => Plugin.Singleton.pluginTranslation;
        private static string PluginName => Plugin.Singleton.pluginHandler.PluginName;
    }
}
