using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features;
using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Permissions;

namespace GhostSpectator.Commands.RemoteAdmin.GhostSettings
{
    public class Reload : ICommand, IUsageProvider
    {
        public Reload(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "PlayerID/all" };
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
            if (!sender.HasPermissions("gs.settings.reload"))
            {
                response = translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            SSGhostSpectator.Singleton.Disable();
            SSGhostSpectator.Singleton = new();
            SSGhostSpectator.Singleton.Enable();
            response = translation.ReloadSuccess;
            return true;
        }

        internal const string _command = "reload";
        internal const string _description = "Reload server-specific settings for GhostSpectator.";
        internal static readonly string[] _aliases = new[] { "r" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
