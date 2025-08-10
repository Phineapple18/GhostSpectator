using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Wrappers;
using MEC;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Features.Extensions
{
    internal static class Duel
    {
        internal static void Abandon(this Player player, Player opponent)
        {
            player.GetGhostComponent().DuelPartner = null;
			opponent.GetGhostComponent().DuelPartner = null;
            player.Health = Config.GhostHealth;
			opponent.Health = Config.GhostHealth;
            opponent.SendHint(Translation.DuelAbandoned.Replace("%playernick%", player.Nickname), 5);
            Log.Debug($"Player {player.Nickname} has abandoned duel with player {opponent.Nickname}.", Config.Debug);
        }

        internal static void Abort(Player player, Player opponent)
        {
            if (opponent != null)
            {
                player.GetGhostComponent().DuelPartner = null;
				opponent.GetGhostComponent().DuelPartner = null;
                player.SendHint(Translation.DuelAborted.Replace("%playernick%", opponent.Nickname), 5);
                opponent.SendHint(Translation.DuelAborted.Replace("%playernick%", player.Nickname), 5);
                Log.Debug($"Duel between players {player.Nickname} and {opponent.Nickname} has been aborted.", Config.Debug);
            }
        }

        internal static void Accept(this Player receiver, Player sender, List<Player> allSenders)
        {
            allSenders.Remove(sender);
            Requests.Remove(sender);
            allSenders.ForEach<Player>(player => Reject(receiver, player));
            string coroutineName = $"duel_{sender.PlayerId}_{receiver.PlayerId}";
            Log.Debug($"Player {receiver.Nickname} has accepted a duel request from player {sender.Nickname}.", Config.Debug);
            AllPending.Add(Timing.RunCoroutine(Prepare(sender, receiver, coroutineName), coroutineName), new(sender, receiver));
        }

        internal static void Finish(Player winner, Player loser)
        {
            winner.GetGhostComponent().DuelPartner = null;
            loser.GetGhostComponent().DuelPartner = null;
            winner.Health = Config.GhostHealth;
            loser.Health = Config.GhostHealth;
            winner.SendHint(Translation.DuelWon.Replace("%playernick%", loser.Nickname), 5);
            loser.SendHint(Translation.DuelLost.Replace("%playernick%", winner.Nickname), 5);
            Log.Debug($"Player {winner.Nickname} won a duel against player {loser.Nickname}.", Config.Debug);
        }

        private static IEnumerator<float> Prepare(Player player1, Player player2, string coroutineName)
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
            AllPending.Remove(AllPending.Keys.First(c => c.Tag == coroutineName));
            Log.Debug($"The duel between players {player1.Nickname} and {player2.Nickname} has begun.", Config.Debug);
            yield break;
        }

        internal static void Reject(this Player receiver, Player sender)
        {
            Requests.Remove(sender);
            sender.SendHint(Translation.DuelRequestRejected.Replace("%playernick%", receiver.Nickname), 5);
            Log.Debug($"Player {receiver.Nickname} rejected a duel request from player {sender.Nickname}.", Config.Debug);
        }

        internal static void Request(this Player sender, Player receiver, Player existingReceiver = null)
        {
            int randInt = new Random().Next();
            receiver.SendHint(Translation.DuelRequestReceived.Replace("%playernick%", sender.Nickname).Replace("%time%", Config.DuelRequestTime.ToString()), 7);
            if (existingReceiver == null)
            {
                Requests.Add(sender, new(receiver, randInt));
            }
            else
            {
                Requests[sender] = new(receiver, randInt);
                existingReceiver.SendHint(Translation.DuelRequestCancelled.Replace("%playernick%", sender.Nickname), 5);
                Log.Debug($"Player {sender.Nickname} has cancelled his duel request with {existingReceiver.Nickname}.", Config.Debug);
            }
            Timing.CallDelayed(Config.DuelRequestTime, delegate()
            {
                if (Requests.Any(entry => entry.Key == sender && entry.Value.Item1 == receiver && entry.Value.Item2 == randInt) && !sender.HasPendingDuel())
                {
                    Requests.Remove(sender);
                    sender.SendHint(Translation.DuelRequestExpired.Replace("%playernick%", receiver.Nickname), 5);
                    Log.Debug($"Duel request from player {sender.Nickname} to player {receiver.Nickname} has expired.", Config.Debug);
                }
            });
            Log.Debug($"Player {sender.Nickname} has sent a duel request to player {receiver.Nickname}.", Config.Debug);
        }

        internal static bool TryAbortPrepare(Player player, out string nickname)
        {
            if (!player.HasPendingDuel())
            {
                nickname = string.Empty;
                return false;
            }
            CoroutineHandle pendingDuel = AllPending.Keys.First(c => c.Tag.Contains(player.PlayerId.ToString()));
            Tuple<Player, Player> duelPair = AllPending[pendingDuel];
            Player opponent = player == duelPair.Item1 ? duelPair.Item2 : duelPair.Item1;
            opponent.SendHint(Translation.DuelAbandoned.Replace("%playernick%", player.Nickname), 5);
            Timing.KillCoroutines(pendingDuel.Tag);
            AllPending.Remove(pendingDuel);
            nickname = opponent.Nickname;
            Log.Debug($"Duel preparation between players {player.Nickname} and {opponent.Nickname} has been aborted.", Config.Debug);
            return true;
        }

        internal static bool TryRemoveRequest(Player player, out string nickname, bool onlySentRequest = true)
        {
            nickname = string.Empty;
            bool result = false;
            if (Requests.TryGetValue(player, out Tuple<Player, int> opponent))
            {
                Requests.Remove(player);
                opponent.Item1.SendHint(Translation.DuelRequestCancelled.Replace("%playernick%", player.Nickname), 5);
                nickname = opponent.Item1.Nickname;
                result = true;
                Log.Debug($"Duel request of player {player.Nickname} has been removed.", Config.Debug);
            }
            if (!onlySentRequest && Requests.Values.Any(p => p.Item1 == player))
            {
                List<Player> allSenders = (from entry in Requests where entry.Value.Item1 == player select entry.Key).ToList();
                allSenders.ForEach<Player>(sender =>
                {
                    Requests.Remove(sender);
                    sender.SendHint(Translation.DuelRequestCancelled.Replace("%playernick%", player.Nickname), 5);
                    Log.Debug($"Duel request from player {sender.Nickname} to player {player.Nickname} has been removed.", Config.Debug);
                });
                result = true;
            }
            return result;
        }

        public static bool HasPendingDuel(this Player player)
        {
            CoroutineHandle pendingDuel = AllPending.Keys.FirstOrDefault(c => c.Tag.Contains(player.PlayerId.ToString()));
            return pendingDuel.IsRunning;
        }

        public static Dictionary<CoroutineHandle, Tuple<Player, Player>> AllPending { get; } = new();
        public static Dictionary<Player, Tuple<Player, int>> Requests { get; } = new();

        private static Config Config => MainClass.Instance.pluginConfig;
        private static Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
