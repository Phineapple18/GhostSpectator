using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminToys;
using CommandSystem;
using GhostSpectator.Extensions;
using PluginAPI.Core;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Commands.ClientConsole.ShootingTarget
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class DestroyTarget: ICommand, IUsageProvider
    {
        public DestroyTarget()
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(translation.DestroyTargetCommand) ? translation.DestroyTargetCommand : _command;
            Description = translation.DestroyTargetDescription;
            Aliases = translation.DestroyTargetAliases;
            Usage = new[] { "NetID/list" };
            Log.Debug($"Registered {this.Command} command.", translation.Debug, Translation.pluginName);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Plugin.Singleton == null)
            {
                response = translation.NotEnabled;
                Log.Debug($"Plugin {Translation.pluginName} is not enabled.", translation.Debug, commandName);
                return false;
            }
            if (sender == null)
            {
                response = translation.SenderNull;
                Log.Debug("Command sender is null.", Config.Debug, commandName);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug, commandName);
                return false;
            }
            GhostComponent component = commandsender.GetGhostComponent();
            if (component.ShootingTargets.Count == 0)
            {
                response = translation.NoTargets;
                Log.Debug($"Player {commandsender.Nickname} doesn't have any spawned shooting targets.", Config.Debug, commandName);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide arguments for command.", Config.Debug, commandName);
                return false;
            }
            if (arguments.At(0).ToLower() == "list")
            {
                response = $"{translation.DestroyTargetList.Replace("%count%", component.ShootingTargets.Count().ToString())}:\n- {string.Join("\n- ", from toy in component.ShootingTargets select toy.netId)}";
                return true;
            }
            if (!uint.TryParse(arguments.At(0), out uint targetId))
            {
                response = translation.MustBeId;
                Log.Debug($"Player {commandsender.Nickname} provided argument in wrong format.", Config.Debug, commandName);
                return false;
            }
            if (!component.ShootingTargets.TryGetFirst(t => t.netId == targetId, out AdminToyBase target))
            {
                response = translation.DestroyTargetFail.Replace("%targetid%", target.netId.ToString());
                Log.Debug($"Player {commandsender.Nickname} doesn't have any spawned shooting target with ID {arguments.ElementAt(0)}.", Config.Debug, commandName);
                return false;
            }
            OtherExtensions.DestroyShootingTarget(component, target);
            response = translation.DestroyTargetSuccess.Replace("%targetname%", target.CommandName).Replace("%targetid%", target.netId.ToString());
            Log.Debug($"Player {commandsender.Nickname} destroyed their target ({target}) with ID {targetId}.", Config.Debug, commandName);
            return true;
        }

        internal const string _command = "DestroyTarget";

        internal const string _description = "Destroy your shooting target or print a list of your targets.";

        internal static readonly string[] _aliases = new[] { "dstg", "dtg" };

        private readonly string commandName;

        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        public bool SanitizeResponse { get; }
        private static Config Config => Plugin.Singleton.pluginConfig;
    }
}
