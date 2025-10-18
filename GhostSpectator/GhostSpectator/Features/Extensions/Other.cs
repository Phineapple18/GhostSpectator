using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InventorySystem;
using InventorySystem.Items.Firearms;
using LabApi.Features.Wrappers;

namespace GhostSpectator.Features.Extensions
{
    internal static class Other
    {
        internal static bool IsGhostItem(this Item item)
        {
            return GhostItemList.Contains(item);
        }

        internal static readonly List<string> voiceChats = new(){ "ghost", "scp", "spectator" };

        internal static readonly IEnumerable<ItemType> firearmList = from g in InventoryItemLoader.AvailableItems where g.Value is Firearm select g.Key;

        internal static HashSet<Item> GhostItemList { get; } = new();
    }
}
