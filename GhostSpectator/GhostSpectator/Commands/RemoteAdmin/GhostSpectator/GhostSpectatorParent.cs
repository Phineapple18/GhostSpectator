using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using NorthwoodLib.Pools;
using Utils.NonAllocLINQ;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.RemoteAdmin.GhostSpectator
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GhostSpectatorParent : ParentCommand
    {
        public GhostSpectatorParent()
        {
            translation = Translation.AccessTranslation();
            Command = translation.GhostspectatorParentCommand ?? _command;
            Description = translation.GhostspectatorParentDescription;
            Aliases = translation.GhostspectatorParentAliases;
            Log.Info($"Registered {this.Command} parent command.");
            this.LoadGeneratedCommands();
        }

        public sealed override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new Despawn(translation.DespawnCommand, translation.DespawnDescription, translation.DespawnAliases));
            this.RegisterCommand(new ListGs(translation.ListGsCommand, translation.ListGsDescription, translation.ListGsAliases));
            this.RegisterCommand(new Spawn(translation.SpawnCommand, translation.SpawnDescription, translation.SpawnAliases));
            Log.Info($"Loaded {this.AllCommands.Count()} subcommand(s) for {this.Command} parent command.");
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
            stringBuilder.AppendLine($"{Description} {translation.Subcommands}:");
            foreach (ICommand command in this.AllCommands)
            {
                stringBuilder.AppendLine($"- {command.Command} | {translation.Aliases}: {(command.Aliases?.Length > 0 ? string.Join(", ", command.Aliases) : "")} | {translation.Description}: {command.Description}");
            }
            response = StringBuilderPool.Shared.ToStringReturn(stringBuilder).TrimEnd(Array.Empty<char>());
            return true;
        }

        internal const string _command = "ghostspectator";
        internal const string _description = "Parent command for Ghosts managing.";
        internal static readonly string[] _aliases = new[] { "ghost", "gsp", "gs" };
        private readonly Translation translation;

        public override string Command { get; }
        public override string Description { get; }
        public override string[] Aliases { get; }
    }
}
