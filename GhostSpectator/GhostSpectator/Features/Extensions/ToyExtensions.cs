using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Features.Extensions
{
    internal static class ToyExtensions
    {
        internal static AdminToyBase CreateToy(Player player, AdminToyBase toyBase, ArraySegment<string> arguments)
        {
            AdminToyBase toy = UnityEngine.Object.Instantiate(toyBase);
            toy.OnSpawned(player.ReferenceHub, arguments);
            foreach (Player ply in Player.List.Except(GhostExtensions.GhostList))
            {
                toy.netIdentity.RemoveObserver(ply.ConnectionToClient);
                ply.ConnectionToClient.RemoveFromObserving(toy.netIdentity, false);
            }
            player.GetGhostComponent().Toys.Add(toy);
            return toy;
        }

        internal static void DestroyToy(Player player, AdminToyBase toy)
        {
            NetworkServer.Destroy(toy.gameObject);
            player.GetGhostComponent().Toys.Remove(toy);
            Log.Debug($"Destroyed toy {toy.name} with ID {toy.netId}.", Config.Debug);
        }

        internal static readonly Dictionary<string, string> toyNames = new()
        {
            { "capy", "Capybara" },
            { "dboy", "TargetDBoy" },
            { "sport", "TargetSport" },
            { "bin", "TargetBinary" }
        };

        internal static List<ToyArea> ToySpawnAreas { get; set; } = new();
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
