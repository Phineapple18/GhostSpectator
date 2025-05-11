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
    [CommandHandler(typeof(ClientCommandHandler))]
    public class DestroyToy : ICommand, IUsageProvider
    {
        public DestroyToy()
        {
            translation = Translation.AccessTranslation();
            Command = translation.DestroyToyCommand ?? _command;
            Description = translation.DestroyToyDescription;
            Aliases = translation.DestroyToyAliases;
            Usage = new[] { "NetID/list" };
            Log.Debug($"Registered {this.Command} command.", translation.Debug);
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
            if (arguments.At(0).ToLower() == "list")
            {
                response = $"{translation.DestroyToyList.Replace("%count%", component.Toys.Count().ToString())}:\n- {string.Join("\n- ", component.Toys.Select(t => $"{t.CommandName} ({t.netId})"))}";
                return true;
            }
            if (!uint.TryParse(arguments.At(0), out uint toyId))
            {
                response = translation.MustBeNumber;
                Log.Debug($"Player {commandsender.Nickname} didn't provide any toy ID.", Config.Debug);
                return false;
            }
            if (!component.Toys.TryGetFirst(t => t.netId == toyId, out AdminToyBase toy))
            {
                response = translation.DestroyToyFail.Replace("%toyid%", toyId.ToString());
                Log.Debug($"Player {commandsender.Nickname} doesn't have any toy with ID {arguments.ElementAt(0)}.", Config.Debug);
                return false;
            }
            Toy.Destroy(commandsender, toy);
            response = translation.DestroyToySuccess.Replace("%toyname%", toy.CommandName).Replace("%toyid%", toy.netId.ToString());
            Log.Debug($"Player {commandsender.Nickname} destroyed their toy ({toy}) with ID {toyId}.", Config.Debug);
            return true;
        }

        internal const string _command = "destroytoy";
        internal const string _description = "Destroy your toy or print a list of your toys.";
        internal static readonly string[] _aliases = new[] { "dst", "dt" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
