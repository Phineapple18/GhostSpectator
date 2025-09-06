using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using GhostSpectator.Features;
using LabApi.Features.Permissions;

namespace GhostSpectator.Commands.RemoteAdmin.GhostSettings
{
    public class Enable : ICommand
    {
        public Enable(string command, string description, string[] aliases)
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
            if (!sender.HasPermissions("gs.settings.reload"))
            {
                response = translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            if (SSGhostSpectator.Singleton != null)
            {
                response = translation.EnableFail;
                return false;
            }
            SSGhostSpectator.Singleton = new();
            SSGhostSpectator.Singleton.Enable();
            response = translation.EnableSuccess;
            return true;
        }

        internal const string _command = "enable";
        internal const string _description = "Enable server-specific settings for GhostSpectator.";
        internal static readonly string[] _aliases = new[] { "e" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
