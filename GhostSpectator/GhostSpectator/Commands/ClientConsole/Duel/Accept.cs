using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Extensions;
using NWAPIPermissionSystem;
using PluginAPI.Core;

namespace GhostSpectator.Commands.ClientConsole.Duel
{
    public class Accept : ICommand
    {
        public Accept(string command, string description, string[] aliases)
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
                requester = allRequesters.First();
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
            commandsender.AcceptDuel(requester, allRequesters);
            response = translation.AcceptSuccess.Replace("%playernick%", requester.Nickname);
            Log.Debug($"Player {commandsender.Nickname} has accepted a duel request from {requester.Nickname}.", Config.Debug, commandName);
            return true;
        }

        internal const string _command = "accept";

        internal const string _description = "Accept duel offer from a player. Provide player nickname, whole or part of it, otherwise the first offer will be accepted. The case is ignored.";

        internal static readonly string[] _aliases = new[] { "a" };

        private readonly string commandName;

        private readonly Translation translation;
       
        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public bool SanitizeResponse { get; }
        private static Config Config => Plugin.Singleton.pluginConfig;
    }
}
