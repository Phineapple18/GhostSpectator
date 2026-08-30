using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using UnityEngine;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.ClientConsole.Duelling
{
    public class Challenge : ICommand, IUsageProvider
    {
        public Challenge(string command, string description, string[] aliases)
        {
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "PlayerNickname (whole or part, case-insensitive)" };
            Log.Info($"Registered {this.Command} subcommand.");
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender == null)
            {
                response = Translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            if (!sender.HasPermissions("gs.duel"))
            {
                response = Translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            if (Warhead.IsDetonated)
            {
                response = Translation.WarheadDetonated;
                Log.Debug($"Player {sender.LogName} tried to use this command after warhead detonation.", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = Translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            if (commandsender.IsInDeathmatch())
            {
                response = Translation.NoDuelInDeathmatch;
                Log.Debug($"Player {commandsender.Nickname} tried to use this command during deathmatch.", Config.Debug);
                return false;
            }
            GhostComponent component = commandsender.GetGhostComponent();
            if (commandsender.HasActiveDuel())
            {
                response = Translation.ActiveDuelSelf.Replace("%playernick%", component.DuelPartner.Nickname);
                Log.Debug($"Player {commandsender.Nickname} has already an active duel with {component.DuelPartner.Nickname}.", Config.Debug);
                return false;
            }
            if (commandsender.HasPendingDuel())
            {
                response = Translation.ActivePendingDuelSelf;
                Log.Debug($"Player {commandsender.Nickname} has already a pending duel with {component.DuelPartner.Nickname}.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty() || arguments.At(0) == string.Empty)
            {
                response = $"{Description} {Translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide arguments.", Config.Debug);
                return false;
            }
            string opponentName = string.Join(" ", arguments);
            List<Player> players = GhostExtensions.GhostList.Where(p => p != commandsender && string.Equals(p.Nickname, opponentName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (players.IsEmpty())
            {
                players = GhostExtensions.GhostList.Where(p => p != commandsender && p.Nickname.IndexOf(opponentName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                if (players.IsEmpty())
                {
                    response = Translation.NoPlayers;
                    Log.Debug($"There is no Ghost, that is not {commandsender.Nickname}, with or containing provided nickname.", Config.Debug);
                    return false;
                }
            }
            Player opponent = players.Aggregate((result, current) 
                            => Vector3.Distance(current.Position, commandsender.Position) < Vector3.Distance(result.Position, commandsender.Position) ? current : result);
            if (opponent.HasActiveDuel())
            {
                response = Translation.ActiveDuelOther.Replace("%playernick%", opponent.Nickname);
                Log.Debug($"Player {commandsender.Nickname} can't challenge {opponent.Nickname} to a duel as they already have an active duel.", Config.Debug);
                return false;
            }
            if (opponent.HasPendingDuel())
            {
                response = Translation.ActivePendingDuelOther.Replace("%playernick%", opponent.Nickname);
                Log.Debug($"Player {commandsender.Nickname} can't challenge {opponent.Nickname} to a duel as they already have a pending duel.", Config.Debug);
                return false;
            }
            if (opponent.IsInDeathmatch())
            {
                response = Translation.OpponentInDeathmatch.Replace("%playernick%", opponent.Nickname);
                Log.Debug($"Player {commandsender.Nickname} can't challenge {opponent.Nickname} to a duel as they are in deatchmatch.", Config.Debug);
                return false;
            }
            if (DuelExtensions.DuelRequests.TryGetValue(commandsender, out Tuple<Player, int> previousOpponent) && previousOpponent.Item1 == opponent)
            {
                response = Translation.RequestAlreadySent;
                Log.Debug($"Player {commandsender.Nickname} already sent a duel request to {opponent.Nickname}.", Config.Debug);
                return false;
            }
            if (Vector3.Distance(commandsender.Position, opponent.Position) > 5)
            {
                response = Translation.PlayerTooFar;
                Log.Debug($"Player {commandsender.Nickname} is to far from player {opponent.Nickname} to send him a duel request.", Config.Debug);
                return false;
            }
            commandsender.SendRequest(opponent, previousOpponent?.Item1);
            if (opponent.IsDummy)
            {
                opponent.AcceptRequest(commandsender, new() { commandsender } );
                response = $"Challenged dummy {opponent.Nickname} to a duel";
                Log.Debug($"Command sent to a dummy {opponent.Nickname} ({opponent.PlayerId}).", Config.Debug);
                return true;
            }
            response = Translation.ChallengeSuccess.Replace("%playernick%", opponent.Nickname);
            Log.Debug($"Player {commandsender.Nickname} has challenged {opponent.Nickname} to a duel.", Config.Debug);
            return true;
        }

        internal const string _command = "challenge";
        internal const string _description = "Challenge another Ghost to a duel.";
        internal static readonly string[] _aliases = new[] { "cha", "ch", "ply", "p" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
