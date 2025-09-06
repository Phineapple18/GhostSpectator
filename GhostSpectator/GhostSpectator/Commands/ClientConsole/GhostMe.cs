using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace GhostSpectator.Commands.ClientConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class GhostMe : ICommand, IUsageProvider
    {
        public GhostMe()
        {
            translation = Translation.AccessTranslation();
            Command = translation.GhostmeCommand ?? _command;
            Description = translation.GhostmeDescription;
            Aliases = translation.GhostmeAliases;
            Usage = new[] { "dpl (optional)" };
            Log.Debug($"Registered {this.Command} command.", translation.Debug);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance == null)
            {
                response = translation.PluginNotEnabled;
                Log.Debug("Plugin GhostSpectator is not enabled.", translation.Debug);
                return false;
            }
            if (sender == null)
            {
                response = translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            if (!sender.HasPermissions("gs.spawn.self"))
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
            bool deathPosition = !arguments.IsEmpty() && arguments.Contains("dpl");
            Player commandsender = Player.Get(sender);
            if (commandsender.IsGhost())
            {
                Ghost.Despawn(commandsender);
                response = translation.GhostmeSpecSuccess;
                Log.Debug($"Player {commandsender.Nickname} turned themselves into Spectator.", Config.Debug);
                return true;
            }
            if (commandsender.Role == RoleTypeId.Spectator)
            {
                if (Warhead.IsDetonated && Config.DespawnOnDetonation && !commandsender.HasPermissions("gs.warhead"))
                {
                    response = translation.WarheadDetonated;
                    Log.Debug($"Player {commandsender.Nickname} doesn't have permission to spawn as Ghost after warhead detonation.", Config.Debug);
                    return false;
                }
                if (deathPosition && !Config.SpawnAtDeathPos)
                {
                    response = translation.DeathPositionDisabled;
                    return false;
                }
                Ghost.Spawn(commandsender, deathPosition);
                response = translation.GhostmeGhostSuccess;
                Log.Debug($"Player {commandsender.Nickname} turned themselves into Ghost.", Config.Debug);
                return true;
            }
            response = translation.GhostmeFail;
            Log.Debug($"Player {commandsender.Nickname} is neither a Ghost nor Spectator.", Config.Debug);
            return false;
        }

        internal const string _command = "ghostme";
        internal const string _description = "Spawn yourself as a Ghost or change back to Spectator. Use \"dpl\" to spawn at your death's position.";
        internal static readonly string[] _aliases = new[] { "gme", "me" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
