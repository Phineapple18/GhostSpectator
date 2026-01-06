using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;

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
            Log.Debug($"Registered {this.Command} command.", translation.Debug);
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
            if (arguments.IsEmpty())
            {
                response = $"{Description} {Translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {commandsender.Nickname} didn't provide any argument.", Config.Debug);
                return false;
            }
            if (arguments.At(0).ToLower() == "list")
            {
                response = $"{Translation.GivefirearmList}:\n- " + string.Join("\n- ", Other.firearmList.Select(f => $"{f} ({(int)f})"));
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

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
