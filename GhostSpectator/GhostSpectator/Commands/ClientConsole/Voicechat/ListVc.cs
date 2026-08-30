using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Wrappers;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.ClientConsole.Voicechat
{
    public class ListVc : ICommand
    {
        public ListVc(string command, string description, string[] aliases)
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
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = Translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            response = $"{Translation.ListVcSuccess}:\n- {string.Join("\n- ", VoicechatParent.voiceChats)}";
            return true;
        }

        internal const string _command = "list";
        internal const string _description = "Print a list of all the toys you created.";
        internal static readonly string[] _aliases = new[] { "l" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
