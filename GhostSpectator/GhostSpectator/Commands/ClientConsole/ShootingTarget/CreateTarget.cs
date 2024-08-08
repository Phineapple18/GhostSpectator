using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminToys;
using CommandSystem;
using GhostSpectator.Extensions;
using Mirror;
using NWAPIPermissionSystem;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;

namespace GhostSpectator.Commands.ClientConsole.ShootingTarget
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class CreateTarget : ICommand, IUsageProvider
    {
        public CreateTarget()
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(translation.CreatetargetCommand) ? translation.CreatetargetCommand : _command;
            Description = translation.CreatetargetDescription;
            Aliases = translation.CreatetargetAliases;
            Usage = new[] { string.Join("/", targetNames.Keys.ToArray())};
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
            if (!sender.CheckPermission("gs.target"))
            {
                response = translation.NoPerms;
                Log.Debug($"Player {sender.LogName} doesn't have required permission to use this command.", Config.Debug, commandName);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug, commandName);
                return false;
            }
            if (Config.TargetLimit <= 0)
            {
                response = translation.NoTargetsAllowed;
                Log.Debug("Spawning shooting targets is not allowed.", Config.Debug, commandName);
                return false;
            }
            if (!OtherExtensions.ShootingRanges.Any(a => a.Contains(commandsender.Position)))
            {
                response = translation.WrongArea;
                Log.Debug($"Player {commandsender.Nickname} can't spawn a shooting target outside of shooting range(s).", Config.Debug, commandName);
                return false;
            }
            IFpcRole fpcRole = commandsender.RoleBase as IFpcRole;
            if (!fpcRole.FpcModule.IsGrounded)
            {
                response = translation.NotGrounded;
                Log.Debug($"Player {commandsender.Nickname} must stand on the ground to spawn a shooting target.", Config.Debug, commandName);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide arguments for command.", Config.Debug, commandName);
                return false;
            }
            AdminToyBase targetBase = null;
            try
            {
                NetworkClient.prefabs.Values.First(a => a.TryGetComponent<AdminToyBase>(out targetBase) && targetBase.CommandName == targetNames[arguments.At(0)]);
            }
            catch (Exception)
            {
                response = translation.WrongArgument;
                Log.Debug($"Player {commandsender.Nickname} provided nonexistent argument: {arguments.ElementAt(0)}.", Config.Debug, commandName);
                return false;
            }
            GhostComponent component = commandsender.GetGhostComponent();
            if (component.ShootingTargets.Count >= Config.TargetLimit)
            {
                OtherExtensions.DestroyShootingTarget(component, component.ShootingTargets.ElementAt(0));
                Log.Debug($"Destroyed first shooting target due to target limit ({Config.TargetLimit}).", Config.Debug, commandName);
            }
            AdminToyBase target = UnityEngine.Object.Instantiate<AdminToyBase>(targetBase);
            target.OnSpawned(commandsender.ReferenceHub, arguments);
            component.ShootingTargets.Add(target);
            response = translation.CreatetargetSuccess.Replace("%targetname%", target.CommandName).Replace("%targetid%", target.netId.ToString());
            Log.Debug($"Player {commandsender.Nickname} created a shooting target ({target.CommandName}) with ID {target.netId}.", Config.Debug, commandName);
            return true;
        }

        internal const string _command = "createtarget";

        internal const string _description = "Create a shooting target.";

        internal static readonly string[] _aliases = new[] { "cstg", "ctg" };

        private readonly Dictionary<string, string> targetNames = new()
        {
            { "dboy", "TargetDBoy" },
            { "sport", "TargetSport" },
            { "bin", "TargetBinary" }
        };

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
