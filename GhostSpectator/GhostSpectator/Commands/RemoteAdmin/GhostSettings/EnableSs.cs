using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features;
using LabApi.Features.Permissions;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.RemoteAdmin.GhostSettings
{
    public class EnableSs : ICommand
    {
        public EnableSs(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Log.Info($"Registered {this.Command} subcommand.");
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
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
                response = translation.EnableSsFail;
                return false;
            }
            SSGhostSpectator.Singleton = new();
            SSGhostSpectator.Singleton.Enable();
            response = translation.EnableSsSuccess;
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
