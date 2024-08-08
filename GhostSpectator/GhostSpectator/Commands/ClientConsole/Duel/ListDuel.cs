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
    public class ListDuel : ICommand
    {
        public ListDuel(string command, string description, string[] aliases)
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
            if (!DuelExtensions.DuelRequests.ContainsKey(commandsender))
            {
                response = translation.NoDuelRequests;
                Log.Debug($"Player {commandsender.Nickname} has no duel requests.", Config.Debug, commandName);
                return false;
            }
            response = $"{translation.ListduelSuccess}: \n- {string.Join("\n- ", from kvp in DuelExtensions.DuelRequests where kvp.Value.Item1 == commandsender select kvp.Key.Nickname)}";
            return true;
        }

        internal const string _command = "list";

        internal const string _description = "Print a list of all players, who challenged you to a duel.";

        internal static readonly string[] _aliases = new[] { "l" };

        private readonly string commandName;

        private static Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        public bool SanitizeResponse { get; }
        private static Config Config => Plugin.Singleton.pluginConfig;
    }
}
