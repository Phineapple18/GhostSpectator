using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Wrappers;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.ClientConsole.Duelling
{
    public class ListDuel : ICommand
    {
        public ListDuel(string command, string description, string[] aliases)
        {
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
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
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = Translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            string opponentName = DuelExtensions.DuelRequests.TryGetValue(commandsender, out Tuple<Player, int> opponent) ? opponent.Item1.Nickname : string.Empty;
            response = Translation.ListDuelSuccess.Replace("%playernick%", opponentName).Replace("%players%", $"{string.Join("\n- ", from entry in DuelExtensions.DuelRequests where entry.Value.Item1 == commandsender select entry.Key.Nickname)}");
            return true;
        }

        internal const string _command = "list";
        internal const string _description = "Print a list of all players who you challenged and who challenged you to a duel.";
        internal static readonly string[] _aliases = new[] { "l" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
