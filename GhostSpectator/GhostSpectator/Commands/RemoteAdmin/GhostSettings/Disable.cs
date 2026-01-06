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
    public class Disable : ICommand
    {
        public Disable(string command, string description, string[] aliases)
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
                Log.Debug($"Plugin {MainClass.Instance.Name} is not enabled.", translation.Debug);
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
            if (SSGhostSpectator.Singleton == null)
            {
                response = translation.DisableFail;
                return false;
            }
            SSGhostSpectator.Singleton.Disable();
            response = translation.DisableSuccess;
            return true;
        }

        internal const string _command = "disable";
        internal const string _description = "Disable server-specific settings for GhostSpectator.";
        internal static readonly string[] _aliases = new[] { "d" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
