using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using NorthwoodLib.Pools;
using Utils.NonAllocLINQ;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.ClientConsole.Toys
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ToyParent : ParentCommand
    {
        public ToyParent()
        {
            translation = Translation.AccessTranslation();
            Command = translation.ToyParentCommand ?? _command;
            Description = translation.ToyParentDescription;
            Aliases = translation.ToyParentAliases;
            Log.Info($"Registered {this.Command} parent command.");
            this.LoadGeneratedCommands();
        }

        public sealed override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new Create(translation.CreateCommand, translation.CreateDescription, translation.CreateAliases));
            this.RegisterCommand(new Destroy(translation.DestroyCommand, translation.DestroyDescription, translation.DestroyAliases));
            this.RegisterCommand(new ListToy(translation.ListToyCommand, translation.ListToyDescription, translation.ListToyAliases));
            Log.Info($"Loaded {this.AllCommands.Count()} subcommand(s) for ToyParent.");
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

        internal const string _command = "toy";
        internal const string _description = "Parent command for toy managing.";
        internal static readonly string[] _aliases = Array.Empty<string>();
        private static Translation translation;

        public override string Command { get; }
        public override string Description { get; }
        public override string[] Aliases { get; }
    }
}
