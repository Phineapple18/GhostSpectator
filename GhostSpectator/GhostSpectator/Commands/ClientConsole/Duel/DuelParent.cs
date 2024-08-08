using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using NorthwoodLib.Pools;
using PluginAPI.Core;

namespace GhostSpectator.Commands.ClientConsole.Duel
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class DuelParent : ParentCommand
    {
        public DuelParent()
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(translation.DuelParentCommand) ? translation.DuelParentCommand : _command;
            Description = translation.DuelParentDescription;
            Aliases = translation.DuelParentAliases;
            Log.Debug($"Registered {this.Command} parent command.", translation.Debug, Translation.pluginName);
            this.LoadGeneratedCommands();
        }

        public sealed override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new Accept(translation.AcceptCommand, translation.AcceptDescription, translation.AcceptAliases));
            this.RegisterCommand(new Cancel(translation.CancelCommand, translation.CancelDescription, translation.CancelAliases));
            this.RegisterCommand(new ListDuel(translation.ListduelCommand, translation.ListduelDescription, translation.ListduelAliases));
            this.RegisterCommand(new Ply(translation.PlayerCommand, translation.PlayerDescription, translation.PlayerAliases));
            this.RegisterCommand(new Reject(translation.RejectCommand, translation.RejectDescription, translation.RejectAliases));
            Log.Debug($"Registered {this.AllCommands.Count()} command(s) for DuelParent.", translation.Debug, Translation.pluginName);
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Plugin.Singleton == null)
            {
                response = translation.NotEnabled;
                Log.Debug($"Plugin {Translation.pluginName} is not enabled.", translation.Debug, commandName);
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

        internal const string _description = "Parent command. Type empty command for more information regarding subcommands.";

        internal static readonly string[] _aliases = Array.Empty<string>();

        private readonly string commandName;

        private static Translation translation;

        public override string Command { get; }
        public override string Description { get; }
        public override string[] Aliases { get; }
        public bool SanitizeResponse { get; }
    }
}
