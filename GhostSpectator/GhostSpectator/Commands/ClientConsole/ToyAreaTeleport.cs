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
using Utils.NonAllocLINQ;

namespace GhostSpectator.Commands.ClientConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ToyAreaTeleport : ICommand, IUsageProvider
    {
        public ToyAreaTeleport()
        {
            translation = Translation.AccessTranslation();
            Command = translation.ToyareateleportCommand ?? _command;
            Description = translation.ToyareateleportDescription;
            Aliases = translation.ToyareateleportAliases;
            Usage = new[] { "AreaName/list" };
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
            if (!sender.HasPermissions("gs.toy.area"))
            {
                response = translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide any argument.", Config.Debug);
                return false;
            }
            if (arguments.At(0).ToLower() == "list")
            {
                response = $"{translation.ToyareateleportList}:\n- " + string.Join("\n- ", Toy.SpawnAreas.Select(a => a.Name));
                return true;
            }
            if (!Toy.SpawnAreas.TryGetFirst(a => a.Name == arguments.At(0), out ToyArea area))
            {
                response = translation.NoArea.Replace("%areaname%", area.Name);
                Log.Debug($"Player {commandsender.Nickname} didn't provide valid area's name.", Config.Debug);
                return false;
            }
            commandsender.Position = area.TeleportPosition;
            Log.Debug($"Player {commandsender.Nickname} was teleported to toy area: {area.Name}.", Config.Debug);
            response = translation.ToyareateleportSuccess.Replace("%areaname%", area.Name);
            return true;
        }

        internal const string _command = "toyareateleport";
        internal const string _description = "Teleport to an area, where you can spawn toys.";
        internal static readonly string[] _aliases = new[] { "tat", "area" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
