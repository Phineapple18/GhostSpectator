using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Permissions;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.RemoteAdmin.GhostSpectator
{
    public class List : ICommand
    {
        public List(string command, string description, string[] aliases)
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
            if (!sender.HasPermissions("gs.list"))
            {
                response = Translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            response = $"{Translation.ListghostSuccess.Replace("%count%", GhostExtensions.GhostList.Count().ToString())}:\n- {string.Join("\n- ", GhostExtensions.GhostList.Select(p => p.Nickname))}";
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
