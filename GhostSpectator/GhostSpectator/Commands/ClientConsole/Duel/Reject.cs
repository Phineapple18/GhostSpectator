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
    public class Reject : ICommand
    {
        public Reject(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(command) ? command : _command;
            Description = description;
            Aliases = aliases;
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
            List<Player> allRequesters = (from p in DuelExtensions.DuelRequests where p.Value.Item1 == commandsender select p.Key).ToList();
            Player requester = null;
            if (arguments.IsEmpty())
            {
                foreach (Player player in allRequesters)
                {
                    commandsender.RejectDuel(player);
                }
                response = translation.RejectSuccessAll;
                Log.Debug($"Player {commandsender.Nickname} has rejected all duel requests.", Config.Debug, commandName);
                return true;
            }
            else
            {
                requester = allRequesters.FirstOrDefault(p => string.Equals(p.Nickname, string.Join(" ", arguments), StringComparison.OrdinalIgnoreCase));
                requester ??= allRequesters.FirstOrDefault(p => p.Nickname.IndexOf(string.Join(" ", arguments), StringComparison.OrdinalIgnoreCase) >= 0);
            }
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

        internal const string _description = "Reject duel offer from player(s). Provide player nickname, whole or part of it, otherwise all offers will be rejected. The case is ignored.";

        internal static readonly string[] _aliases = new[] { "r" };

        private readonly string commandName;

        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public bool SanitizeResponse { get; }
        private static Config Config => Plugin.Singleton.pluginConfig;
    }
}
