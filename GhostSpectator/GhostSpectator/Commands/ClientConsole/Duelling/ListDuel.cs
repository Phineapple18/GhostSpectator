using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Wrappers;

namespace GhostSpectator.Commands.ClientConsole.Duelling
{
    public class ListDuel : ICommand
    {
        public ListDuel(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Log.Debug($"Registered {this.Command} subcommand.", translation.Debug);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance == null)
            {
                response = translation.PluginNotEnabled;
                Log.Debug("Plugin GhostSpectator is not enabled.", translation.Debug);
                return false;
            }
            if (sender == null)
            {
                response = translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            string opponentName = Duel.Requests.TryGetValue(commandsender, out Tuple<Player, int> opponent) ? opponent.Item1.Nickname : string.Empty;
            response = translation.ListduelSuccess.Replace("%playernick%", opponentName).Replace("%players%", $"{string.Join("\n- ", from kvp in Duel.Requests where kvp.Value.Item1 == commandsender select kvp.Key.Nickname)}");
            return true;
        }

        internal const string _command = "list";
        internal const string _description = "Print a list of all players who you challenged and who challenged you to a duel.";
        internal static readonly string[] _aliases = new[] { "l" };
        private static Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
