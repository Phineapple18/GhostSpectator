using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using Log = LabApi.Features.Console.Logger;
using NorthwoodLib.Pools;
using Utils.NonAllocLINQ;

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
            Log.Debug($"Registered {this.Command} parent command.", translation.Debug);
            this.LoadGeneratedCommands();
        }

        public sealed override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new Despawn(translation.DespawnCommand, translation.DespawnDescription, translation.DespawnAliases));
            this.RegisterCommand(new List(translation.ListghostCommand, translation.ListghostDescription, translation.ListghostAliases));
            this.RegisterCommand(new Spawn(translation.SpawnCommand, translation.SpawnDescription, translation.SpawnAliases));
            Log.Debug($"Loaded {this.AllCommands.Count()} command(s) for {this.Command} parent command.", translation.Debug);
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance == null)
            {
                response = translation.PluginNotEnabled;
                Log.Debug("Plugin GhostSpectator is not enabled.", translation.Debug);
                return false;
            }
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
            stringBuilder.AppendLine($"{Description} {translation.Subcommands}:");
            foreach (ICommand command in this.AllCommands)
            {
                stringBuilder.AppendLine($"- {command.Command} | {translation.Aliases}: {(command.Aliases == null || command.Aliases.IsEmpty() ? "" : string.Join(", ", command.Aliases))} | {translation.Description}: {command.Description}");
            }
            response = StringBuilderPool.Shared.ToStringReturn(stringBuilder).TrimEnd(Array.Empty<char>());
            return true;
        }

        internal const string _command = "ghostspectator";
        internal const string _description = "Parent command for managing Ghosts.";
        internal static readonly string[] _aliases = new[] { "ghost", "gsp", "gs" };
        private readonly Translation translation;

        public override string Command { get; }
        public override string Description { get; }
        public override string[] Aliases { get; }
    }
}
