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
            Usage = new[] { "%item%/list" };
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
            if (!sender.CheckPermission("gs.firearm"))
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
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide arguments for command.", Config.Debug, commandName);
                return false;
            }
            if (arguments.At(0).ToLower() == "list")
            {
                response = $"{translation.GivefirearmList}:" + string.Join("\n- ", from g in InventoryItemLoader.AvailableItems where g.Value.Category == ItemCategory.Firearm select g.Key);
                return true;
            }
            if (!Enum.TryParse(arguments.At(0), out ItemType itemType))
            {
                response = translation.ItemtypeOnly;
                Log.Debug($"Player {commandsender.Nickname} didn't provide an item type.", Config.Debug, commandName);
                return false;
            }
            if (!InventoryItemLoader.AvailableItems.Any(i => i.Key == itemType && i.Value.Category == ItemCategory.Firearm))
            {
                response = translation.FirearmOnly;
                Log.Debug($"Player {commandsender.Nickname} didn't provide an item type, that is a firearm.", Config.Debug, commandName);
                return false;
            }
            Firearm firearm = commandsender.AddItem(itemType) as Firearm;
            if (AttachmentsServerHandler.PlayerPreferences.TryGetValue(commandsender.ReferenceHub, out Dictionary<ItemType, uint> dictionary) && dictionary.TryGetValue(itemType, out uint code))
            {
                firearm.ApplyAttachmentsCode(code, true);
                Log.Debug($"Player {commandsender.Nickname} attachment preferences have been applied to the firearm.", Config.Debug, commandName);
            }
            firearm.Status = new(firearm.AmmoManagerModule.MaxAmmo, FirearmStatusFlags.MagazineInserted, firearm.GetCurrentAttachmentsCode());
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
