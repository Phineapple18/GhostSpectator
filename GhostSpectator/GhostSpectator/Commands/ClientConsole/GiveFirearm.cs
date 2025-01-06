using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Extensions;
using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using NWAPIPermissionSystem;
using PluginAPI.Core;

namespace GhostSpectator.Commands.ClientConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class GiveFirearm : ICommand, IUsageProvider
    {
        public GiveFirearm()
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(translation.GivefirearmCommand) ? translation.GivefirearmCommand : _command;
            Description = translation.GivefirearmDescription;
            Aliases = translation.GivefirearmAliases;
            Usage = new[] { "ItemType/list" };
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
                Log.Debug("Command sender doesn't exist.", Config.Debug, commandName);
                return false;
            }
            if (!sender.CheckPermission("gs.firearm"))
            {
                response = translation.NoPerms;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug, commandName);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug, commandName);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide any argument.", Config.Debug, commandName);
                return false;
            }
            if (arguments.At(0).ToLower() == "list")
            {
                response = $"{translation.GivefirearmList}:\n- " + string.Join("\n- ", from g in InventoryItemLoader.AvailableItems where g.Value is Firearm select g.Key);
                return true;
            }
            if (!Enum.TryParse(arguments.At(0), out ItemType itemType))
            {
                response = translation.ItemtypeOnly;
                Log.Debug($"Player {commandsender.Nickname} didn't provide any item type.", Config.Debug, commandName);
                return false;
            }
            if (!InventoryItemLoader.TryGetItem(itemType, out Firearm firearm))
            {
                response = translation.FirearmOnly;
                Log.Debug($"Player {commandsender.Nickname} didn't provide any firearm.", Config.Debug, commandName);
                return false;
            }
            commandsender.ReferenceHub.inventory.ServerAddItem(firearm.ItemTypeId, InventorySystem.Items.ItemAddReason.AdminCommand, firearm.ItemSerial, firearm.PickupDropModel);
            if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(commandsender.ReferenceHub, out Dictionary<ItemType, uint> dictionary) && dictionary.TryGetValue(itemType, out uint code))
            {
                firearm.ApplyAttachmentsCode(code, true);
                Log.Debug($"Player {commandsender.Nickname} attachment preferences have been applied to the firearm.", Config.Debug, commandName);
            }
            response = translation.GivefirearmSuccess.Replace("%itemtype%", itemType.ToString());
            Log.Debug($"Player {commandsender.Nickname} has given themselves a {itemType}.", Config.Debug, commandName);
            return true;
        }

        internal const string _command = "givefirearm";

        internal const string _description = "Give yourself a firearm or print a list of available firearms.";

        internal static readonly string[] _aliases = new[] { "firearm", "givegun", "gun" };

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
