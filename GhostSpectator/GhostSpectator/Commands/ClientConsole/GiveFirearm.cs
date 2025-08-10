using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;

namespace GhostSpectator.Commands.ClientConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class GiveFirearm : ICommand, IUsageProvider
    {
        public GiveFirearm()
        {
            translation = Translation.AccessTranslation();
            Command = translation.GivefirearmCommand ?? _command;
            Description = translation.GivefirearmDescription;
            Aliases = translation.GivefirearmAliases;
            Usage = new[] { "%item%/ItemType/list" };
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
            if (!sender.HasPermissions("gs.firearm"))
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
                response = $"{translation.GivefirearmList}:\n- " + string.Join("\n- ", Other.firearmList.Select(f => $"{f} ({(int)f})"));
                return true;
            }
            ItemType itemType = ItemType.None;
            if (!(int.TryParse(arguments.At(0), out int id) || Enum.TryParse(arguments.At(0), out itemType)))
            {
                response = translation.MustBeNumberOrType;
                Log.Debug($"Player {commandsender.Nickname} didn't provide any item ID or type.", Config.Debug);
                return false;
            }
            if (!(InventoryItemLoader.TryGetItem((ItemType)id, out Firearm firearm) || InventoryItemLoader.TryGetItem(itemType, out firearm)))
            {
                response = translation.FirearmOnly;
                Log.Debug($"Player {commandsender.Nickname} didn't provide any firearm.", Config.Debug);
                return false;
            }
            commandsender.AddItem(firearm.ItemTypeId, ItemAddReason.AdminCommand);
            response = translation.GivefirearmSuccess.Replace("%itemtype%", firearm.ItemTypeId.ToString());
            Log.Debug($"Player {commandsender.Nickname} has given themselves a {firearm.ItemTypeId}.", Config.Debug);
            return true;
        }

        internal const string _command = "givefirearm";
        internal const string _description = "Give yourself a firearm or print a list of available firearms.";
        internal static readonly string[] _aliases = new[] { "firearm", "givegun", "gun" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
