using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using NorthwoodLib.Pools;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Commands.ClientConsole.Duelling
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class DuelParent : ParentCommand
    {
        public DuelParent()
        {
            translation = Translation.AccessTranslation();
            Command = translation.DuelParentCommand ?? _command;
            Description = translation.DuelParentDescription;
            Aliases = translation.DuelParentAliases;
            Log.Debug($"Registered {this.Command} parent command.", translation.Debug);
            this.LoadGeneratedCommands();
        }

        public sealed override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new Accept(translation.AcceptCommand, translation.AcceptDescription, translation.AcceptAliases));
            this.RegisterCommand(new Cancel(translation.CancelCommand, translation.CancelDescription, translation.CancelAliases));
            this.RegisterCommand(new Challenge(translation.ChallengeCommand, translation.ChallengeDescription, translation.ChallengeAliases));
            this.RegisterCommand(new ListDuel(translation.ListduelCommand, translation.ListduelDescription, translation.ListduelAliases));
            this.RegisterCommand(new Reject(translation.RejectCommand, translation.RejectDescription, translation.RejectAliases));
            Log.Debug($"Loaded {this.AllCommands.Count()} command(s) for DuelParent.", translation.Debug);
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
            stringBuilder.AppendLine($"{Description} \n{translation.Subcommands}:");
            foreach (ICommand command in this.AllCommands)
            {
                stringBuilder.AppendLine($"- {command.Command} | {translation.Aliases}: {(command.Aliases == null || command.Aliases.IsEmpty() ? "" : string.Join(", ", command.Aliases))} | {translation.Description}: {command.Description}");
            }
            response = StringBuilderPool.Shared.ToStringReturn(stringBuilder).TrimEnd(Array.Empty<char>());
            return true;
        }

        internal const string _command = "duel";
        internal const string _description = "Parent command for Ghost duelling.";
        internal static readonly string[] _aliases = Array.Empty<string>();
        private static Translation translation;

        public override string Command { get; }
        public override string Description { get; }
        public override string[] Aliases { get; }
    }
}
