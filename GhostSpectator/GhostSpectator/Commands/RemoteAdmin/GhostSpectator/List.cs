using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Permissions;

namespace GhostSpectator.Commands.RemoteAdmin.GhostSpectator
{
    public class List : ICommand
    {
        public List(string command, string description, string[] aliases)
        {
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Log.Debug($"Registered {this.Command} subcommand.", Translation.AccessTranslation().Debug);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance == null)
            {
                response = Translation.PluginNotEnabled;
                Log.Debug($"Plugin {MainClass.Instance.Name} is not enabled.", Translation.Debug);
                return false;
            }
            if (sender == null)
            {
                response = Translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            if (!sender.HasPermissions("gs.list"))
            {
                response = Translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            response = $"{Translation.ListghostSuccess.Replace("%count%", Ghost.List.Count().ToString())}:\n- {string.Join("\n- ", Ghost.List.Select(p => p.Nickname))}";
            return true;
        }

        internal const string _command = "list";
        internal const string _description = "Print a list of all Ghosts.";
        internal static readonly string[] _aliases = new[] { "l" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
