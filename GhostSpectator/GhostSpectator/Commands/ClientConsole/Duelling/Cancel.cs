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
    public class Cancel : ICommand
    {
        public Cancel(string command, string description, string[] aliases)
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
            if (!(commandsender.IsGhost() || commandsender.IsGhostDespawning()))
            {
                response = Translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            Player opponent = commandsender.GetGhostComponent().DuelPartner;
            if (opponent != null)
            {
                DuelExtensions.FinishDuel(commandsender, opponent);
                response = Translation.CancelDuelSuccess.Replace("%playernick%", opponent.Nickname);
                Log.Debug($"Player {commandsender.Nickname} has cancelled a duel with player {opponent.Nickname}.", Config.Debug);
                return true;
            }
            if (DuelExtensions.TryAbortPendingDuel(commandsender, out string opponentName))
            {
                response = Translation.CancelDuelSuccess.Replace("%playernick%", opponentName);
                Log.Debug($"Player {commandsender.Nickname} has cancelled a pending duel with player {opponentName}.", Config.Debug);
                return true;
            }
            if (DuelExtensions.TryRemoveRequest(commandsender, out opponentName))
            {
                response = Translation.CancelRequestSuccess.Replace("%playernick%", opponentName);
                Log.Debug($"Player {commandsender.Nickname} has cancelled a duel request with player(s) {opponentName}.", Config.Debug);
                return true;
            }
            response = Translation.CancelFail;
            Log.Debug($"Player {commandsender.Nickname} has no active or pending duels or duel requests.", Config.Debug);
            return false;
        }

        internal const string _command = "cancel";
        internal const string _description = "Cancel your duel, pending duel or duel request.";
        internal static readonly string[] _aliases = new[] { "cnx", "c" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
