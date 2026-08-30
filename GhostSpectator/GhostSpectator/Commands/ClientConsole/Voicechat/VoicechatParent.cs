using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using NorthwoodLib.Pools;
using Utils.NonAllocLINQ;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.ClientConsole.Voicechat
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class VoicechatParent : ParentCommand
    {
        public VoicechatParent()
        {
            translation = Translation.AccessTranslation();
            Command = translation.VoicechatParentCommand ?? _command;
            Description = translation.VoicechatParentDescription;
            Aliases = translation.VoicechatParentAliases;
            Log.Info($"Registered {this.Command} parent command.");
            this.LoadGeneratedCommands();
        }

        public sealed override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new EnableVc(translation.EnableVcCommand, translation.EnableVcDescription, translation.EnableVcAliases));
            this.RegisterCommand(new DisableVc(translation.DisableVcCommand, translation.DisableVcDescription, translation.DisableVcAliases));
            this.RegisterCommand(new ListVc(translation.ListVcCommand, translation.ListVcDescription, translation.ListVcAliases));
            Log.Info($"Loaded {this.AllCommands.Count()} subcommand(s) for VoicechatParent.");
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
            stringBuilder.AppendLine($"{Description} \n{translation.Subcommands}:");
            foreach (ICommand command in this.AllCommands)
            {
                stringBuilder.AppendLine($"- {command.Command} | {translation.Aliases}: {(command.Aliases?.Length > 0 ? string.Join(", ", command.Aliases) : "")} | {translation.Description}: {command.Description}");
            }
            response = StringBuilderPool.Shared.ToStringReturn(stringBuilder).TrimEnd(Array.Empty<char>());
            return true;
        }

        internal const string _command = "voicechat";
        internal const string _description = "Parent command for voice chats.";
        internal static readonly string[] _aliases = new[] { "vc" };
        private static Translation translation;
        internal static readonly List<string> voiceChats = new() { "ghost", "scp", "spectator" };

        public override string Command { get; }
        public override string Description { get; }
        public override string[] Aliases { get; }
    }
}
