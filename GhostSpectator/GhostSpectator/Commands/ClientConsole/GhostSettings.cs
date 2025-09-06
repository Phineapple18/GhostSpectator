using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using GhostSpectator.Features;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;

namespace GhostSpectator.Commands.ClientConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class GhostSettings : ICommand
    {
        public GhostSettings()
        {
            translation = Translation.AccessTranslation();
            Command = translation.GhostsettingsCommand ?? _command;
            Description = translation.GhostsettingsDescription;
            Aliases = translation.GhostsettingsAliases;
            Log.Debug($"Registered {this.Command} command.", translation.Debug);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance== null)
            {
                response = translation.PluginNotEnabled;
                Log.Debug("Plugin GhostSpectator is not enabled.", translation.Debug);
                return false;
            }
            if (SSGhostSpectator.Singleton == null)
            {
                response = translation.SssNotEnabled;
                Log.Debug("Server-specific settings for GhostSpectator plugin are not enabled.", translation.Debug);
                return false;
            }
            if (sender == null)
            {
                response = translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            if (!sender.HasPermissions("gs.settings.activate"))
            {
                response = translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            if (!Round.IsRoundStarted)
            {
                response = translation.RoundNotStarted;
                Log.Debug($"Player {sender.LogName} tried to use this command before round start.", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhostNorSpectator;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            if (SSGhostSpectator.Singleton.lastSentPages.ContainsKey(commandsender.ReferenceHub))
            {
                SSGhostSpectator.Singleton.DeactivateForHub(commandsender.ReferenceHub);
                response = translation.GhostsettingsDeactivated;
                Log.Debug($"Player {commandsender.Nickname} deactivated their ghost settings.", Config.Debug);
            }
            else
            {
                SSGhostSpectator.Singleton.ActivateForHub(commandsender.ReferenceHub);
                response = translation.GhostsettingsActivated;
                Log.Debug($"Player {commandsender.Nickname} activated their ghost settings.", Config.Debug);
            }
            return true;
        }

        internal const string _command = "ghostsettings";
        internal const string _description = "Activate or deactivate GhostSpectator Server-specific Settings.";
        internal static readonly string[] _aliases = new[] { "gset" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
