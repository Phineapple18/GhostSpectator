using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminToys;
using CommandSystem;
using GhostSpectator.Features;
using GhostSpectator.Features.Extensions;
using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles.FirstPersonControl;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Commands.ClientConsole.Toys
{
    public class Create : ICommand, IUsageProvider
    {
        public Create(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { string.Join("/", Toy.names.ToArray()) };
            Log.Debug($"Registered {this.Command} subcommand.", translation.Debug);
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
            if (!sender.HasPermissions("gs.toy"))
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
            if (Config.ToyLimit <= 0)
            {
                response = translation.NoToysAllowed;
                Log.Debug("Spawning toys is not allowed.", Config.Debug);
                return false;
            }
            if (!Toy.SpawnAreas.Any(a => a.Contains(commandsender.Position)))
            {
                response = translation.WrongArea;
                Log.Debug($"Player {commandsender.Nickname} tried to create a toy outside the spawn range(s).", Config.Debug);
                return false;
            }
            if (!(commandsender.RoleBase as IFpcRole).FpcModule.IsGrounded)
            {
                response = translation.NotGrounded;
                Log.Debug($"Player {commandsender.Nickname} must stand on the ground.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide any argument.", Config.Debug);
                return false;
            }
            AdminToyBase toyBase = null;
            try
            {
                NetworkClient.prefabs.Values.First(a => a.TryGetComponent(out toyBase) && toyBase.CommandName.ToLower() == arguments.At(0).ToLower());
            }
            catch (Exception)
            {
                response = translation.WrongArgument;
                Log.Debug($"Player {commandsender.Nickname} provided non-existent argument.", Config.Debug);
                return false;
            }
            GhostComponent component = commandsender.GetGhostComponent();
            if (component.Toys.Count >= Config.ToyLimit)
            {
                Toy.Destroy(commandsender, component.Toys.ElementAt(0));
                Log.Debug($"Destroyed first toy due to toy limit ({Config.ToyLimit}).", Config.Debug);
            }
            AdminToyBase toy = Toy.Create(commandsender, toyBase, arguments);
            response = translation.CreateSuccess.Replace("%toyname%", toy.CommandName).Replace("%toyid%", toy.netId.ToString());
            Log.Debug($"Player {commandsender.Nickname} created a toy ({toy.CommandName}) with ID {toy.netId}.", Config.Debug);
            return true;
        }

        internal const string _command = "create";
        internal const string _description = "Create a toy.";
        internal static readonly string[] _aliases = new[] { "c" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
