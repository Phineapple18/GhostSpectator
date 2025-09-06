using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThrowableItem = InventorySystem.Items.ThrowableProjectiles.ThrowableItem;

using InventorySystem;
using InventorySystem.Items.Firearms;
using LabApi.Features.Wrappers;
using PlayerRoles;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Features.Extensions
{
    internal static class Other
    {
        internal static bool IsGhostItem(this Item item)
        {
            return item != null && GhostItemList.Contains(item);
        }

        internal static bool IsGhostItem(this Pickup pickup)
        {
            return pickup != null && GhostItemList.Any(i => i.Serial == pickup.Serial);
        }

        internal static bool IsGhostItem(this ThrowableItem item)
        {
            return item != null && GhostItemList.Any(i => i.Serial == item.ItemSerial);
        }

        internal static bool IsDead(ReferenceHub hub)
        {
            return !hub.IsAlive() || hub.IsGhost() || hub.IsGhostDespawning() || hub.IsGhostSpawning();
        }

        internal static readonly List<string> voiceChats = new(){ "ghost", "scp", "spectator" };

        internal static readonly IEnumerable<ItemType> firearmList = from g in InventoryItemLoader.AvailableItems where g.Value is Firearm select g.Key;

        internal static HashSet<Item> GhostItemList { get; } = new();
    }
}
