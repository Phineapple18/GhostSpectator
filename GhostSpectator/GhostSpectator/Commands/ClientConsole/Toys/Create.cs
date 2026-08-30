using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminToys;
using CommandSystem;
using GhostSpectator.Features;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles.FirstPersonControl;
using Utils.NonAllocLINQ;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.ClientConsole.Toys
{
    public class Create : ICommand, IUsageProvider
    {
        public Create(string command, string description, string[] aliases)
        {
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { string.Join("/", ToyExtensions.toyNames.ToArray()) };
            Log.Info($"Registered {this.Command} subcommand.");
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender == null)
            {
                response = Translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            if (!sender.HasPermissions("gs.toy.create"))
            {
                response = Translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = Translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            if (commandsender.IsInDeathmatch())
            {
                response = Translation.NoToysInDeatchmatch;
                Log.Debug($"Player {commandsender.Nickname} can't spawn toys during deatchmatch.", Config.Debug);
                return false;
            }
            if (Config.ToyLimit <= 0)
            {
                response = Translation.NoToysAllowed;
                Log.Debug("Spawning toys is not allowed.", Config.Debug);
                return false;
            }
            if (!ToyExtensions.ToySpawnAreas.Any(a => a.Bounds.Contains(commandsender.Position)))
            {
                response = Translation.WrongArea;
                Log.Debug($"Player {commandsender.Nickname} tried to create a toy outside the spawn range(s).", Config.Debug);
                return false;
            }
            if (!(commandsender.RoleBase as IFpcRole).FpcModule.IsGrounded)
            {
                response = Translation.NotGrounded;
                Log.Debug($"Player {commandsender.Nickname} must stand on the ground.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {Translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide any argument.", Config.Debug);
                return false;
            }
            AdminToyBase toyBase = null;
            if (NetworkClient.prefabs.Values.FirstOrDefault(a => a.TryGetComponent(out toyBase) && (toyBase.CommandName.ToLower() == arguments.At(0).ToLower()
            || ToyExtensions.toyNames.TryGetValue(arguments.At(0), out string toyName) && toyBase.CommandName.ToLower() == toyName)) == null)
            {
                response = Translation.WrongArgument;
                Log.Debug($"Player {commandsender.Nickname} provided non-existent argument.", Config.Debug);
                return false;
            }
            GhostComponent component = commandsender.GetGhostComponent();
            if (component.Toys.Count >= Config.ToyLimit)
            {
                ToyExtensions.DestroyToy(commandsender, component.Toys.ElementAt(0));
                Log.Debug($"Destroyed first toy due to toy limit ({Config.ToyLimit}).", Config.Debug);
            }
            AdminToyBase toy = ToyExtensions.CreateToy(commandsender, toyBase, arguments);
            response = Translation.CreateSuccess.Replace("%toyname%", toy.CommandName).Replace("%toyid%", toy.netId.ToString());
            Log.Debug($"Player {commandsender.Nickname} created a toy ({toy.CommandName}) with ID {toy.netId}.", Config.Debug);
            return true;
        }

        internal const string _command = "create";
        internal const string _description = "Create a toy.";
        internal static readonly string[] _aliases = new[] { "c" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
