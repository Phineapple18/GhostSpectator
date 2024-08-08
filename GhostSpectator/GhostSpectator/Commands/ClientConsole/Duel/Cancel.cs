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
    public class Cancel : ICommand
    {
        public Cancel(string command, string description, string[] aliases)
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
            Player opponent = commandsender.GetGhostComponent().DuelPartner;
            if (opponent != null)
            {
                commandsender.AbandonDuel(opponent);
                response = translation.CancelDuelSuccess.Replace("%playernick%", opponent.Nickname);
                Log.Debug($"Player {commandsender.Nickname} has cancelled a duel with {opponent.Nickname}.", Config.Debug, commandName);
                return true;
            }
            if (DuelExtensions.TryAbortDuelPreparation(commandsender, out string opponent2))
            {
                response = translation.CancelDuelSuccess.Replace("%playernick%", opponent2);
                Log.Debug($"Player {commandsender.Nickname} has cancelled pending duel with {opponent2}.", Config.Debug, commandName);
                return true;
            }
            if (DuelExtensions.TryRemoveDuelRequest(commandsender, out string opponent3))
            {
                response = translation.CancelRequestSuccess.Replace("%playernick%", opponent3);
                Log.Debug($"Player {commandsender.Nickname} has cancelled a duel request with {opponent3}.", Config.Debug, commandName);
                return true;
            }
            response = translation.CancelFail;
            Log.Debug($"Player {commandsender.Nickname} has no active/pending duels or duel requests.", Config.Debug, commandName);
            return false;
        }

        internal const string _command = "cancel";

        internal const string _description = "Cancel your duel request or current duel.";

        internal static readonly string[] _aliases = new[] { "cnx", "c" };

        private readonly string commandName;

        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public bool SanitizeResponse { get; }
        private static Config Config => Plugin.Singleton.pluginConfig;
    }
}
