using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using NorthwoodLib.Pools;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Commands.RemoteAdmin.GhostSettings
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GhostSettingsParent : ParentCommand
    {
        public GhostSettingsParent()
        {
            translation = Translation.AccessTranslation();
            Command = translation.GhostsettingsParentCommand ?? _command;
            Description = translation.GhostsettingsParentDescription;
            Aliases = translation.GhostsettingsParentAliases;
            Log.Debug($"Registered {this.Command} parent command.", translation.Debug);
            this.LoadGeneratedCommands();
        }

        public sealed override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new Disable(translation.DisableCommand, translation.DisableDescription, translation.DisableAliases));
            this.RegisterCommand(new Enable(translation.EnableCommand, translation.EnableDescription, translation.EnableAliases));
            this.RegisterCommand(new Reload(translation.ReloadCommand, translation.ReloadDescription, translation.ReloadAliases));
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

        internal const string _command = "ghostsettings";
        internal const string _description = "Parent command for managing server-specific settings for GhostSpectator.";
        internal static readonly string[] _aliases = new[] { "gst" };
        private readonly Translation translation;

        public override string Command { get; }
        public override string Description { get; }
        public override string[] Aliases { get; }
    }
}
