using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Extensions;
using NWAPIPermissionSystem;
using PluginAPI.Core;
using UnityEngine;

namespace GhostSpectator.Commands.ClientConsole.Duel
{
    public class Challenge : ICommand, IUsageProvider
    {
        public Challenge(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(command) ? command : _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "PlayerNickname (whole or part, case-insensitive)" };
            Log.Debug($"Registered {this.Command} subcommand.", translation.Debug, Translation.pluginName);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Plugin.Singleton == null)
            {
                response = translation.NotEnabled;
                Log.Debug($"Plugin {Translation.pluginName} is not enabled.", translation.Debug, commandName);
                return false;
            }
            if (sender == null)
            {
                response = translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug, commandName);
                return false;
            }
            if (!sender.CheckPermission("gs.duel"))
            {
                response = translation.NoPerms;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug, commandName);
                return false;
            }
            if (Warhead.IsDetonated)
            {
                response = translation.WarheadDetonated;
                Log.Debug($"Player {sender.LogName} tried to use this command after warhead detonation.", Config.Debug, commandName);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug, commandName);
                return false;
            }
            GhostComponent component = commandsender.GetComponent<GhostComponent>();
            if (component.DuelPartner != null)
            {
                response = translation.ActiveDuelSelf.Replace("%playernick%", component.DuelPartner.Nickname);
                Log.Debug($"Player {commandsender.Nickname} has already an active duel with {component.DuelPartner.Nickname}.", Config.Debug, commandName);
                return false;
            }
            if (commandsender.HasPendingDuel())
            {
                response = translation.ActivePendingDuel;
                Log.Debug($"Player {commandsender.Nickname} has already a pending duel with {component.DuelPartner.Nickname}.", Config.Debug, commandName);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide arguments.", Config.Debug, commandName);
                return false;
            }
            string opponentName = string.Join(" ", arguments);
            List<Player> players = GhostExtensions.GhostPlayerList.Where(p => p != commandsender && string.Equals(p.Nickname, opponentName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (players.IsEmpty())
            {
                players = GhostExtensions.GhostPlayerList.Where(p => p.Nickname.IndexOf(opponentName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (players.IsEmpty())
            {
                response = translation.NoGhosts;
                Log.Debug($"There is no Ghost, that is not {commandsender.Nickname}, with or containing provided nickname.", Config.Debug, commandName);
                return false;
            }
            Player opponent = players.ElementAt(0);
            if (players.Count > 1)
            {
                for (int i = 1; i < players.Count; i++)
                {
                    if (Vector3.Distance(commandsender.Position, players[i].Position) < Vector3.Distance(commandsender.Position, opponent.Position))
                    {
                        opponent = players[i];
                    }
                }
            }
            if (opponent.GetComponent<GhostComponent>().DuelPartner != null)
            {
                response = translation.ActiveDuelOther.Replace("%playernick%", opponent.Nickname);
                Log.Debug($"Player {commandsender.Nickname} can't challenge {opponent.Nickname} to a duel as they already have an active duel.", Config.Debug, commandName);
                return false;
            }
            if (DuelExtensions.DuelRequests.TryGetValue(commandsender, out Tuple<Player, int> previousOpponent) && previousOpponent.Item1 == opponent)
            {
                response = translation.RequestAlreadySent;
                Log.Debug($"Player {commandsender.Nickname} already sent a duel request to {opponent.Nickname}.", Config.Debug, commandName);
                return false;
            }
            commandsender.RequestDuel(opponent, previousOpponent?.Item1);
            response = translation.ChallengeSuccess.Replace("%playernick%", opponent.Nickname);
            Log.Debug($"Player {commandsender.Nickname} has challenged {opponent.Nickname} to a duel.", Config.Debug, commandName);
            return true;
        }

        internal const string _command = "challenge";

        internal const string _description = "Challenge another Ghost to a duel.";

        internal static readonly string[] _aliases = new[] { "cha", "ch", "ply", "p" };

        private readonly string commandName;

        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        public bool SanitizeResponse { get; }
        private static Config Config => Plugin.Singleton.pluginConfig;
    }
}
