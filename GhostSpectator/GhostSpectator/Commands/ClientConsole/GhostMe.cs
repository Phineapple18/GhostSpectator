using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Extensions;
using NWAPIPermissionSystem;
using PlayerRoles;
using PluginAPI.Core;

namespace GhostSpectator.Commands.ClientConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class GhostMe : ICommand
	{
        public GhostMe() 
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(translation.GhostmeCommand) ? translation.GhostmeCommand : _command;
            Description = translation.GhostmeDescription;
            Aliases = translation.GhostmeAliases;
            Log.Debug($"Registered {this.Command} command.", translation.Debug, Translation.pluginName);
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
            if (!sender.CheckPermission("gs.spawn.self"))
			{
                response = translation.NoPerms;
                Log.Debug($"Player {sender.LogName} doesn't have required permission to use this command.", Config.Debug, commandName);
                return false;
            }
            if (!Round.IsRoundStarted)
            {
                response = translation.RoundNotStarted;
                Log.Debug("This command can't be used before round start.", Config.Debug, commandName);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (commandsender.IsGhost())
			{
				GhostExtensions.Despawn(commandsender);
                response = translation.GhostmeSpecSuccess;
                Log.Debug($"Player {commandsender.Nickname} turned themselves into Spectator.", Config.Debug, commandName);
                return true;
			}
			if (commandsender.Role == RoleTypeId.Spectator)
			{
                if (Warhead.IsDetonated && Config.DespawnOnDetonation && !commandsender.CheckPermission("gs.warhead"))
                {
                    response = translation.WarheadDetonated;
                    Log.Debug($"Player {commandsender.Nickname} doesn't have required permission to spawn as Ghost after warhead detonation.", Config.Debug, commandName);
                    return false;
                }
                GhostExtensions.Spawn(commandsender);
                response = translation.GhostmeGhostSuccess;
                Log.Debug($"Player {commandsender.Nickname} turned themselves into Ghost.", Config.Debug, commandName);
                return true;
			}
            response = translation.GhostmeFail;
            Log.Debug($"Player {commandsender.Nickname} must be a Ghost or Spectator to use this command.", Config.Debug, commandName);
            return false;
		}

        internal const string _command = "ghostme";

        internal const string _description = "Change yourself to Ghost from Spectator or vice versa.";

        internal static readonly string[] _aliases = new[] { "gme", "me" };

        private readonly string commandName;

        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public bool SanitizeResponse { get; }
        private static Config Config => Plugin.Singleton.pluginConfig;
    }
}
