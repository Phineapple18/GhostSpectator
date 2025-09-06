using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Commands.RemoteAdmin.GhostSpectator
{
    public class Despawn : ICommand, IUsageProvider
    {
        public Despawn(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "PlayerID/all" };
            Log.Debug($"Registered {this.Command} subcommand.", translation.Debug);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance == null)
            {
                response = translation.PluginNotEnabled;
                Log.Debug("Plugin GhostSpectator is not enabled.", translation.Debug);
                return false;
            }
            if (sender == null)
            {
                response = translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            if (!sender.HasPermissions("gs.spawn.other"))
            {
                response = translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide any argument.", Config.Debug );
                return false;
            }
            List<Player> validPlayers = arguments.At(0).ToLower() == "all" ? Player.ReadyList.ToList() : Player.ReadyList.Where(p => arguments.Contains(p.PlayerId.ToString())).ToList();
            if (validPlayers.IsEmpty())
            {
                response = translation.NoPlayers;
                Log.Debug($"Player {sender.LogName} provided non-existent player(s).", Config.Debug);
                return false;
            }
            StringBuilder success = StringBuilderPool.Shared.Rent();
            StringBuilder failure = StringBuilderPool.Shared.Rent();
            success.AppendLine(translation.DespawnSuccess);
            failure.AppendLine($"{translation.DespawnFail}:");
            int[] num = new int[2] { 0, 0 };
            validPlayers.ForEach<Player>(player =>
            {
                if (player.IsGhost())
                {
                    Ghost.Despawn(player);
                    num[1]++;
                    return;
                }
                failure.AppendLine($"- {player.Nickname}");
                num[0]++;
                Log.Debug($"Player {player.Nickname} is not a Ghost.", Config.Debug);
            });
            success.Replace("%count%", num[1].ToString());
            failure.Replace("%count%", num[0].ToString());
            StringBuilder result = num[1] == 0 ? failure : num[0] == 0 ? success : success.Append(failure);
            response = StringBuilderPool.Shared.ToStringReturn(result).TrimEnd(Array.Empty<char>());
            Log.Debug($"Player {sender.LogName} despawned successfully ({num[1]}) and unsuccessfully ({num[0]}) Ghosts.", Config.Debug);
            return num[1] > 0;
        }

        internal const string _command = "despawn";
        internal const string _description = "Despawn chosen Ghost(s) to Spectator. Separate entries with space.";
        internal static readonly string[] _aliases = new[] { "d" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
