using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabApi.Features.Wrappers;
using MEC;
using Utils.NonAllocLINQ;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Features.Extensions
{
    internal static class DuelExtensions
    {
        internal static void FinishDuel(Player player, Player opponent, bool isWinLose = false)
        {
            player.GetGhostComponent().DuelPartner = opponent.GetGhostComponent().DuelPartner = null;
            player.Health = opponent.Health = Config.GhostHealth;
            if (isWinLose)
            {
                player.SendHint(Translation.DuelWon.Replace("%playernick%", opponent.Nickname), 5);
                opponent.SendHint(Translation.DuelLost.Replace("%playernick%", player.Nickname), 5);
                Log.Debug($"Player {player.Nickname} won a duel against player {opponent.Nickname}.", Config.Debug);
            }
            else
            {
                if (player.IsGhost())
                {
                    player.SendHint(Translation.DuelAborted.Replace("%playernick%", opponent.Nickname), 5);
                }
                if (opponent.IsGhost())
                {
                    opponent.SendHint(Translation.DuelAborted.Replace("%playernick%", player.Nickname), 5);
                }
                Log.Debug($"Player {player.Nickname} has aborted duel with player {opponent.Nickname}.", Config.Debug);
            }
        }

        internal static void AcceptRequest(this Player receiver, Player sender, List<Player> senders)
        {
            senders.Remove(sender);
            DuelRequests.Remove(sender);
            senders.ForEach<Player>(player => RejectRequest(receiver, player));
            string pairName = $"duel_{sender.PlayerId}_{receiver.PlayerId}";
            PendingDuels.Add(Timing.RunCoroutine(PrepareDuel(sender, receiver, pairName), pairName), new(sender, receiver));
            Log.Debug($"Player {receiver.Nickname} has accepted a duel request from player {sender.Nickname}.", Config.Debug);
        }

        private static IEnumerator<float> PrepareDuel(Player player1, Player player2, string pairName)
        {
            player1.SendHint(Translation.DuelPrepare, 5);
            player2.SendHint(Translation.DuelPrepare, 5);
            Log.Debug($"Preparing duel for players {player1.Nickname} and {player2.Nickname}.", Config.Debug);
            yield return Timing.WaitForSeconds(5f);
            int i = 5;
            while (i > 0)
            {
                player1.SendHint(i.ToString());
                player2.SendHint(i.ToString());
                i--;
                yield return Timing.WaitForSeconds(1f);
            }
            player1.GetGhostComponent().DuelPartner = player2;
            player2.GetGhostComponent().DuelPartner = player1;
            player1.SendHint(Translation.DuelStarted, 5);
            player2.SendHint(Translation.DuelStarted, 5);
            PendingDuels.Remove(PendingDuels.Keys.First(c => c.Tag == pairName));
            Log.Debug($"The duel between players {player1.Nickname} and {player2.Nickname} has begun.", Config.Debug);
            yield break;
        }

        internal static void RejectRequest(this Player receiver, Player sender)
        {
            DuelRequests.Remove(sender);
            sender.SendHint(Translation.DuelRequestRejected.Replace("%playernick%", receiver.Nickname), 5);
            Log.Debug($"Player {receiver.Nickname} rejected a duel request from player {sender.Nickname}.", Config.Debug);
        }

        internal static void SendRequest(this Player sender, Player receiver, Player previousReceiver = null)
        {
            int requestId = Round.Duration.Milliseconds;
            receiver.SendHint(Translation.DuelRequestReceived.Replace("%playernick%", sender.Nickname).Replace("%time%", Config.DuelRequestTime.ToString()), 7);
            DuelRequests[sender] = new(receiver, requestId);
            previousReceiver?.SendHint(Translation.DuelRequestCancelled.Replace("%playernick%", sender.Nickname), 5);
            Timing.CallDelayed(Config.DuelRequestTime, delegate()
            {
                if (DuelRequests.Any(entry => entry.Key == sender && entry.Value.Item1 == receiver && entry.Value.Item2 == requestId) && !sender.HasPendingDuel())
                {
                    DuelRequests.Remove(sender);
                    sender.SendHint(Translation.DuelRequestExpired.Replace("%playernick%", receiver.Nickname), 5);
                    Log.Debug($"Duel request from player {sender.Nickname} to player {receiver.Nickname} has expired.", Config.Debug);
                }
            });
            Log.Debug($"Player {sender.Nickname} has sent a duel request to player {receiver.Nickname}.", Config.Debug);
        }

        internal static bool TryAbortPendingDuel(Player player, out string nickname)
        {
            if (!player.HasPendingDuel())
            {
                nickname = string.Empty;
                return false;
            }
            var pendingDuel = PendingDuels.First(d => d.Key.Tag.Contains(player.PlayerId.ToString()));
            Player opponent = player == pendingDuel.Value.Item1 ? pendingDuel.Value.Item2 : pendingDuel.Value.Item1;
            opponent.SendHint(Translation.DuelAborted.Replace("%playernick%", player.Nickname), 5);
            Timing.KillCoroutines(pendingDuel.Key.Tag);
            PendingDuels.Remove(pendingDuel.Key);
            nickname = opponent.Nickname;
            Log.Debug($"Duel preparation between players {player.Nickname} and {nickname} has been aborted.", Config.Debug);
            return true;
        }

        internal static bool TryRemoveRequest(Player player, out string nickname)
        {
            if (!DuelRequests.TryGetValue(player, out Tuple<Player, int> opponent))
            {
                nickname = string.Empty;
                return false;
            }
            DuelRequests.Remove(player);
            if (opponent.Item1.IsGhost())
            {
                opponent.Item1.SendHint(Translation.DuelRequestCancelled.Replace("%playernick%", player.Nickname), 5);
            }
            nickname = opponent.Item1.Nickname;
            Log.Debug($"Duel request of player {player.Nickname} has been removed.", Config.Debug);
            return true;
        }

        public static bool HasActiveDuel(this Player player)
        {
            return player.TryGetGhostComponent(out GhostComponent component) && component.DuelPartner != null;
        }

        public static bool HasPendingDuel(this Player player)
        {
            CoroutineHandle pendingDuel = PendingDuels.Keys.FirstOrDefault(c => c.Tag.Contains(player.PlayerId.ToString()));
            return pendingDuel.IsRunning;
        }

        public static Dictionary<CoroutineHandle, Tuple<Player, Player>> PendingDuels { get; } = new();
        public static Dictionary<Player, Tuple<Player, int>> DuelRequests { get; } = new();
        private static Config Config => MainClass.Instance.pluginConfig;
        private static Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
