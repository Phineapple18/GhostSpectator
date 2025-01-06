using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Extensions;
using PluginAPI.Core;

namespace GhostSpectator.Commands.ClientConsole.Duel
{
    public class Reject : ICommand, IUsageProvider
    {
        public Reject(string command, string description, string[] aliases)
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
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug, commandName);
                return false;
            }
            if (!DuelExtensions.DuelRequests.Values.Any(p => p.Item1 == commandsender))
            {
                response = translation.NoDuelRequests;
                Log.Debug($"Player {commandsender.Nickname} has no duel requests.", Config.Debug, commandName);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide arguments.", Config.Debug, commandName);
                return false;
            }
            List<Player> allRequesters = (from p in DuelExtensions.DuelRequests where p.Value.Item1 == commandsender select p.Key).ToList();
            if (allRequesters.IsEmpty())
            {
                response = translation.NoDuelRequests;
                Log.Debug($"Player {commandsender.Nickname} has no duel requests.", Config.Debug, commandName);
                return false;
            }
            Player requester = requester = allRequesters.FirstOrDefault(p => string.Equals(p.Nickname, string.Join(" ", arguments), StringComparison.OrdinalIgnoreCase));
            requester ??= allRequesters.FirstOrDefault(p => p.Nickname.IndexOf(string.Join(" ", arguments), StringComparison.OrdinalIgnoreCase) >= 0);
            if (requester == null)
            {
                response = translation.NoPlayers;
                Log.Debug("Provided player(s) doesn't exist.", Config.Debug, commandName);
                return false;
            }
            commandsender.RejectDuel(requester);
            response = translation.RejectSuccessPlayer.Replace("%playernick%", requester.Nickname);
            Log.Debug($"Player {commandsender.Nickname} has rejected a duel request from {requester.Nickname}.", Config.Debug, commandName);
            return true;
        }

        internal const string _command = "reject";

        internal const string _description = "Reject a duel offer from other Ghost.";

        internal static readonly string[] _aliases = new[] { "r" };

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
