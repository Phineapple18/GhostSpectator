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
    public class Cancel : ICommand
    {
        public Cancel(string command, string description, string[] aliases)
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
            Player opponent = commandsender.GetGhostComponent().DuelPartner;
            if (opponent != null)
            {
                commandsender.Abandon(opponent);
                response = translation.CancelDuelSuccess.Replace("%playernick%", opponent.Nickname);
                Log.Debug($"Player {commandsender.Nickname} has cancelled a duel with {opponent.Nickname}.", Config.Debug);
                return true;
            }
            if (Duel.TryAbortPrepare(commandsender, out string opponentName))
            {
                response = translation.CancelDuelSuccess.Replace("%playernick%", opponentName);
                Log.Debug($"Player {commandsender.Nickname} has cancelled a pending duel with {opponentName}.", Config.Debug);
                return true;
            }
            if (Duel.TryRemoveRequest(commandsender, out opponentName))
            {
                response = translation.CancelRequestSuccess.Replace("%playernick%", opponentName);
                Log.Debug($"Player {commandsender.Nickname} has cancelled a duel request with {opponentName}.", Config.Debug);
                return true;
            }
            response = translation.CancelFail;
            Log.Debug($"Player {commandsender.Nickname} has no active or pending duels or duel requests.", Config.Debug);
            return false;
        }

        internal const string _command = "cancel";
        internal const string _description = "Cancel your duel, pending duel or duel request.";
        internal static readonly string[] _aliases = new[] { "cnx", "c" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
