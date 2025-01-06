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
            if (player.CurrentItem is Firearm firearm)
            {
                int maxAmmo = firearm.GetTotalMaxAmmo();
                int storedAmmo = firearm.GetTotalStoredAmmo();
                if (storedAmmo < maxAmmo)
                {
                    try
                    {
                        MagazineModule module = firearm.Modules.First(m => m is MagazineModule) as MagazineModule;
                        module.ServerModifyAmmo(maxAmmo);
                    }
                    catch (InvalidOperationException)
                    {
                        CylinderAmmoModule module = firearm.Modules.First(m => m is CylinderAmmoModule) as CylinderAmmoModule;
                        player.AddAmmo(module.AmmoType, (ushort)(maxAmmo - storedAmmo));
                    }
                }
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
