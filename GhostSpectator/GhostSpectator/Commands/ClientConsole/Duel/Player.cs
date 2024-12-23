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
    public class Ply : ICommand, IUsageProvider
    {
        public Ply(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(command) ? command : _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "PlayerNickname" };
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
                Log.Debug("Command sender is null.", Config.Debug, commandName);
                return false;
            }
            if (!sender.CheckPermission("gs.duel"))
            {
                response = translation.NoPerms;
                Log.Debug($"Player {sender.LogName} doesn't have required permission to use this command.", Config.Debug, commandName);
                return false;
            }
            if (Warhead.IsDetonated)
            {
                response = translation.WarheadDetonated;
                Log.Debug($"Player {sender.LogName} can't use this command after warhead detonation.", Config.Debug, commandName);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug, commandName);
                return false;
            }
            GhostComponent component = commandsender.GetGhostComponent();
            if (component.DuelPartner != null)
            {
                response = translation.ActiveDuelSelf.Replace("%playernick%", component.DuelPartner.Nickname);
                Log.Debug($"Player {commandsender.Nickname} has already active duel with {component.DuelPartner.Nickname}.", Config.Debug, commandName);
                return false;
            }
            if (commandsender.HasPendingDuel())
            {
                response = translation.ActivePendingDuel;
                Log.Debug($"Player {commandsender.Nickname} has already pending duel with {component.DuelPartner.Nickname}.", Config.Debug, commandName);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide arguments for command.", Config.Debug, commandName);
                return false;
            }
            List<Player> players = new();
            string nickname = string.Join(" ", arguments);
            var ghostList = GhostExtensions.GhostPlayerList.Where(p => p != commandsender).ToList();
            if (ghostList.Any(p => string.Equals(p.Nickname, nickname, StringComparison.OrdinalIgnoreCase)))
            {
                players = ghostList.Where(p => string.Equals(p.Nickname, nickname, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                players = ghostList.Where(p => p.Nickname.IndexOf(nickname, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            if (players.IsEmpty())
            {
                response = translation.NoGhosts;
                Log.Debug($"There is no Ghost, that is not {commandsender.Nickname}, with/containing provided nickname.", Config.Debug, commandName);
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
            if (opponent.GetGhostComponent().DuelPartner != null)
            {
                response = translation.ActiveDuelOther.Replace("%playernick%", opponent.Nickname);
                Log.Debug($"Player {commandsender.Nickname} can't challenge {opponent.Nickname} to duel as they already have active duel.", Config.Debug, commandName);
                return false;
            }
            if (DuelExtensions.DuelRequests.TryGetValue(commandsender, out Tuple<Player, int> previousOpponent) && previousOpponent.Item1 == opponent)
            {
                response = translation.RequestAlreadySent;
                Log.Debug($"Player {commandsender.Nickname} already sent duel request to {opponent.Nickname}.", Config.Debug, commandName);
                return false;
            }
            commandsender.RequestDuel(opponent, previousOpponent?.Item1);
            response = translation.PlayerSuccess.Replace("%playernick%", opponent.Nickname);
            Log.Debug($"Player {commandsender.Nickname} has challenged {opponent.Nickname} to a duel.", Config.Debug, commandName);
            return true;
        }

        internal const string _command = "player";

        internal const string _description = "Challenge another Ghost to a duel by typing their nickname, whole or part of it. The case is ignored.";

        internal static readonly string[] _aliases = new[] { "p", "pl", "ply" };

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
