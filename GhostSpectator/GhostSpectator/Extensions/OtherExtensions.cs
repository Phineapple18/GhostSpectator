using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using PluginAPI.Core;
using UnityEngine;

namespace GhostSpectator.Extensions
{
    public static class OtherExtensions
    {
        internal static void RefreshAmmo(this Player player)
        {
            if (player.CurrentItem is Firearm firearm && firearm.GetTotalStoredAmmo() < firearm.GetTotalMaxAmmo())
            {
                MagazineModule module = firearm.Modules.FirstOrDefault(m => m is MagazineModule) as MagazineModule;
                module.ServerModifyAmmo(firearm.GetTotalMaxAmmo());
            }
        }

        public static bool IsGhostItem(this ItemBase item)
        {
            return item != null && GhostItemList.Contains(item);
        }

        internal static readonly Dictionary<string, KeyValuePair<string, string>> voiceChats = new()
        {
            { "scp", new("ListenScpChat", "SCPs") },
            { "dead", new("ListenSpectator", "Spectators") },
            { "ghost", new("ListenGhosts", "Ghosts") }
        };

        internal static HashSet<ItemBase> GhostItemList { get; } = new();
        internal static List<Vector3> SpawnPositions { get; private set; } = new();
    }
}
