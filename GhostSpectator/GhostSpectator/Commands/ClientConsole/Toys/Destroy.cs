using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using AdminToys;
using CommandSystem;
using GhostSpectator.Features;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Wrappers;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Commands.ClientConsole.Toys
{
    public class Destroy : ICommand, IUsageProvider
    {
        public Destroy(string command, string description, string[] aliases)
        {
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "NetID" };
            Log.Debug($"Registered {this.Command} subcommand.", Translation.AccessTranslation().Debug);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance == null)
            {
                response = Translation.PluginNotEnabled;
                Log.Debug($"Plugin {MainClass.Instance.Name} is not enabled.", Translation.Debug);
                return false;
            }
            if (sender == null)
            {
                response = Translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = Translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            GhostComponent component = commandsender.GetGhostComponent();
            if (component.Toys.Count == 0)
            {
                response = Translation.NoToys;
                Log.Debug($"Player {commandsender.Nickname} doesn't have any toys.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty() || arguments.At(0) == string.Empty)
            {
                response = $"{Description} {Translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide any argument.", Config.Debug);
                return false;
            }
            if (!uint.TryParse(arguments.At(0), out uint toyId))
            {
                response = Translation.MustBeNumber;
                Log.Debug($"Player {commandsender.Nickname} didn't provide any toy ID.", Config.Debug);
                return false;
            }
            if (!component.Toys.TryGetFirst(t => t.netId == toyId, out AdminToyBase toy))
            {
                response = Translation.DestroyFail.Replace("%toyid%", toyId.ToString());
                Log.Debug($"Player {commandsender.Nickname} doesn't have any toy with ID {arguments.ElementAt(0)}.", Config.Debug);
                return false;
            }
            Toy.Destroy(commandsender, toy);
            response = Translation.DestroySuccess.Replace("%toyname%", toy.CommandName).Replace("%toyid%", toy.netId.ToString());
            Log.Debug($"Player {commandsender.Nickname} destroyed their toy ({toy}) with ID {toyId}.", Config.Debug);
            return true;
        }

        internal const string _command = "destroy";
        internal const string _description = "Destroy your toy.";
        internal static readonly string[] _aliases = new[] { "d" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
