using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AdminToys;
using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;

namespace GhostSpectator.Features.Extensions
{
    internal static class Toy
    {
        internal static AdminToyBase Create(Player player, AdminToyBase toyBase, ArraySegment<string> arguments)
        {
            AdminToyBase toy = UnityEngine.Object.Instantiate(toyBase);
            toy.OnSpawned(player.ReferenceHub, arguments);
            foreach (Player ply in Player.List.Except(Ghost.List))
            {
                toy.netIdentity.observers.Remove(ply.ReferenceHub.networkIdentity.connectionToClient.connectionId);
                ply.ReferenceHub.networkIdentity.connectionToClient.RemoveFromObserving(toy.netIdentity, false);
            }
            player.GetGhostComponent().Toys.Add(toy);
            return toy;
        }

        internal static void Destroy(Player player, AdminToyBase toy)
        {
            NetworkServer.Destroy(toy.gameObject);
            player.GetGhostComponent().Toys.Remove(toy);
            Log.Debug($"Destroyed toy {toy.name} with ID {toy.netId}.", Config.Debug);
        }

        internal static readonly List<string> names = new()
        {
            "Capybara",
            "TargetDBoy",
            "TargetSport",
            "TargetBinary"
        };

        internal static List<Bounds> SpawnAreas { get; set; } = new();
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
