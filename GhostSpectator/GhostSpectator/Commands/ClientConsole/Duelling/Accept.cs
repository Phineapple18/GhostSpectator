using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Commands.ClientConsole.Duelling
{
    public class Accept : ICommand, IUsageProvider
    {
        public Accept(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "PlayerNickname (whole or part, case-insensitive)" };
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
            if (!sender.HasPermissions("gs.duel"))
            {
                response = translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            if (Warhead.IsDetonated)
            {
                response = translation.WarheadDetonated;
                Log.Debug($"Player {sender.LogName} tried to use this command after warhead detonation.", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty() || arguments.At(0) == string.Empty)
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide arguments.", Config.Debug);
                return false;
            }
            List<Player> allRequesters = (from p in Duel.Requests where p.Value.Item1 == commandsender select p.Key).ToList();
            if (allRequesters.IsEmpty())
            {
                response = translation.NoDuelRequests;
                Log.Debug($"Player {commandsender.Nickname} has no duel requests.", Config.Debug);
                return false;
            }
            Player requester = requester = allRequesters.FirstOrDefault(p => string.Equals(p.Nickname, string.Join(" ", arguments), StringComparison.OrdinalIgnoreCase), null);
            requester ??= allRequesters.FirstOrDefault(p => p.Nickname.IndexOf(string.Join(" ", arguments), StringComparison.OrdinalIgnoreCase) >= 0, null);
            if (requester == null)
            {
                response = translation.NoPlayers;
                Log.Debug("Provided player(s) doesn't exist.", Config.Debug);
                return false;
            }
            commandsender.Accept(requester, allRequesters);
            response = translation.AcceptSuccess.Replace("%playernick%", requester.Nickname);
            Log.Debug($"Player {commandsender.Nickname} has accepted a duel request from {requester.Nickname}.", Config.Debug);
            return true;
        }

        internal const string _command = "accept";
        internal const string _description = "Accept a duel offer from other Ghost.";
        internal static readonly string[] _aliases = new[] { "a" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
