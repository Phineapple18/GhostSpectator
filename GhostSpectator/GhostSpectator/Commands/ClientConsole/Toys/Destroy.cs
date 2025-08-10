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
using LabApi.Features.Wrappers;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Commands.ClientConsole.Toys
{
    public class Destroy : ICommand, IUsageProvider
    {
        public Destroy(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "NetID" };
            Log.Debug($"Registered {this.Command} subcommand.", translation.Debug);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance == null)
            {
                response = translation.PluginNotEnabled;
                Log.Debug($"Plugin GhostSpectator is not enabled.", translation.Debug);
                return false;
            }
            if (sender == null)
            {
                response = translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            GhostComponent component = commandsender.GetGhostComponent();
            if (component.Toys.Count == 0)
            {
                response = translation.NoToys;
                Log.Debug($"Player {commandsender.Nickname} doesn't have any toys.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty() || arguments.At(0) == string.Empty)
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide any argument.", Config.Debug);
                return false;
            }
            if (!uint.TryParse(arguments.At(0), out uint toyId))
            {
                response = translation.MustBeNumber;
                Log.Debug($"Player {commandsender.Nickname} didn't provide any toy ID.", Config.Debug);
                return false;
            }
            if (!component.Toys.TryGetFirst(t => t.netId == toyId, out AdminToyBase toy))
            {
                response = translation.DestroyFail.Replace("%toyid%", toyId.ToString());
                Log.Debug($"Player {commandsender.Nickname} doesn't have any toy with ID {arguments.ElementAt(0)}.", Config.Debug);
                return false;
            }
            Toy.Destroy(commandsender, toy);
            response = translation.DestroySuccess.Replace("%toyname%", toy.CommandName).Replace("%toyid%", toy.netId.ToString());
            Log.Debug($"Player {commandsender.Nickname} destroyed their toy ({toy}) with ID {toyId}.", Config.Debug);
            return true;
        }

        internal const string _command = "destroy";
        internal const string _description = "Destroy your toy.";
        internal static readonly string[] _aliases = new[] { "d" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
