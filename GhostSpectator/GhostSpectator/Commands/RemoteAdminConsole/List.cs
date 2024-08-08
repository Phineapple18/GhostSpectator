using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Extensions;
using NWAPIPermissionSystem;
using PluginAPI.Core;

namespace GhostSpectator.Commands.RemoteAdminConsole
{
	public class List : ICommand
	{
        public List(string command, string description, string[] aliases)
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
            if (!sender.CheckPermission("gs.list"))
            {
                response = translation.NoPerms;
                Log.Debug($"Player {sender.LogName} doesn't have required permission to use this command.", Config.Debug, commandName);
                return false;
            }
            response = $"{translation.ListghostSuccess.Replace("%count%", GhostExtensions.GhostPlayerList.Count().ToString())}:\n- {string.Join("\n- ", from player in GhostExtensions.GhostPlayerList select player.Nickname)}";
            return true;
		}

        internal const string _command = "list";

        internal const string _description = "Print a list of all Ghosts.";

        internal static readonly string[] _aliases = new[] { "l" };

        private readonly string commandName;

        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public bool SanitizeResponse { get; }
        private static Config Config => Plugin.Singleton.pluginConfig;
    }
}
