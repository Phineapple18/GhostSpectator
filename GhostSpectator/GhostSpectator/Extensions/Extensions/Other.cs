using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThrowableItem = InventorySystem.Items.ThrowableProjectiles.ThrowableItem;
using LabApi.Features.Wrappers;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Features.Extensions
{
    internal static class Other
    {
        public static bool IsGhostItem(this Item item)
        {
            return item != null && GhostItemList.Contains(item);
        }

        public static bool IsGhostItem(this Pickup pickup)
        {
            return pickup != null && GhostItemList.Any(i => i.Serial == pickup.Serial);
        }

        public static bool IsGhostItem(this ThrowableItem item)
        {
            return item != null && GhostItemList.Any(i => i.Serial == item.ItemSerial);
        }

        internal static readonly List<string> voiceChats = new(){ "ghost", "scp", "spectator" };

        internal static HashSet<Item> GhostItemList { get; } = new();
    }
}
