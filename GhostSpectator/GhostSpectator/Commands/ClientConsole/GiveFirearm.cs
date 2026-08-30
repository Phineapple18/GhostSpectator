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
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.ClientConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class GiveFirearm : ICommand, IUsageProvider
    {
        public GiveFirearm()
        {
            Translation translation = Translation.AccessTranslation();
            Command = translation.GivefirearmCommand ?? _command;
            Description = translation.GivefirearmDescription;
            Aliases = translation.GivefirearmAliases;
            Usage = new[] { "%item%/ItemType/list" };
            Log.Info($"Registered {this.Command} command.");
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender == null)
            {
                response = Translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            if (!sender.HasPermissions("gs.firearm"))
            {
                response = Translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            if (!Round.IsRoundStarted)
            {
                response = Translation.RoundNotStarted;
                Log.Debug($"Player {sender.LogName} tried to use this command before round start.", Config.Debug);
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
                response = Translation.NoWeaponInDeatchmatch;
                Log.Debug($"Player {commandsender.Nickname} can't use this command in deathmatch.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {Translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide any argument.", Config.Debug);
                return false;
            }
            if (arguments.At(0).ToLower() == "list")
            {
                response = $"{Translation.GivefirearmList}:\n- " + string.Join("\n- ", firearmList.Select(f => $"{f} ({(int)f})"));
                return true;
            }
            int gunLimit = Server.CategoryLimits[ItemCategory.Firearm];
            if (commandsender.Items.Count(i => i is FirearmItem) >= gunLimit)
            {
                response = $"{Translation.GivefirearmFailure.Replace("%categorylimit%", gunLimit.ToString())}";
                return false;
            }
            ItemType itemType = ItemType.None;
            if (!(int.TryParse(arguments.At(0), out int id) || Enum.TryParse(arguments.At(0), out itemType)))
            {
                response = Translation.MustBeNumberOrType;
                Log.Debug($"Player {commandsender.Nickname} didn't provide any item ID or type.", Config.Debug);
                return false;
            }
            if (!(InventoryItemLoader.TryGetItem((ItemType)id, out Firearm firearm) || InventoryItemLoader.TryGetItem(itemType, out firearm)))
            {
                response = Translation.FirearmOnly;
                Log.Debug($"Player {commandsender.Nickname} didn't provide any firearm.", Config.Debug);
                return false;
            }
            commandsender.AddItem(firearm.ItemTypeId, ItemAddReason.AdminCommand);
            response = Translation.GivefirearmSuccess.Replace("%itemtype%", firearm.ItemTypeId.ToString());
            Log.Debug($"Player {commandsender.Nickname} has given themselves a {firearm.ItemTypeId}.", Config.Debug);
            return true;
        }

        internal const string _command = "givefirearm";
        internal const string _description = "Give yourself a firearm or print a list of available firearms.";
        internal static readonly string[] _aliases = new[] { "firearm", "givegun", "gun" };
        internal static readonly IEnumerable<ItemType> firearmList = from g in InventoryItemLoader.AvailableItems where g.Value is Firearm select g.Key;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
