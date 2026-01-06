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
    public class Spawn : ICommand, IUsageProvider
    {
        public Spawn(string command, string description, string[] aliases)
        {
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "dpl (optional)", "PlayerID/all" };
            Log.Debug($"Registered {this.Command} subcommand.", Translation.AccessTranslation().Debug);
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
            if (!sender.HasPermissions("gs.spawn.all"))
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
            if (Warhead.IsDetonated && Config.DespawnOnDetonation && !sender.HasPermissions("gs.warhead"))
            {
                response = Translation.WarheadDetonated;
                Log.Debug($"Player {sender.LogName} doesn't have permission use this command after warhead detonation.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {Translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide any argument.", Config.Debug);
                return false;
            }
            List<Player> validPlayers = arguments.At(0).ToLower() == "all" ? Player.ReadyList.ToList() : Player.ReadyList.Where(p => arguments.Contains(p.PlayerId.ToString())).ToList();
            if (validPlayers.IsEmpty())
            {
                response = Translation.NoPlayers;
                Log.Debug($"Player {sender.LogName} provided non-existent player(s).", Config.Debug);
                return false;
            }
            StringBuilder success = StringBuilderPool.Shared.Rent();
            StringBuilder failure = StringBuilderPool.Shared.Rent();
            success.AppendLine(Translation.SpawnSuccess);
            failure.AppendLine($"{Translation.SpawnFail}:");
            int[] num = new int[2] { 0, 0 };
            bool deathPosition = arguments.ToList().Contains("dpl", StringComparison.OrdinalIgnoreCase);
            if (deathPosition && !Config.SpawnAtDeathPos)
            {
                response = Translation.DeathPositionDisabled;
                return false;
            }
            validPlayers.ForEach<Player>(player =>
            {
                if (!player.IsGhost())
                {
                    Ghost.Spawn(player, deathPosition);
                    num[1]++;
                    return;
                }
                failure.AppendLine($"- {player.Nickname}");
                num[0]++;
                Log.Debug($"Player {player.Nickname} is already a Ghost.", Config.Debug);
            });
            success.Replace("%count%", num[1].ToString());
            failure.Replace("%count%", num[0].ToString());
            StringBuilder result = num[1] == 0 ? failure : num[0] == 0 ? success : success.Append(failure);
            response = StringBuilderPool.Shared.ToStringReturn(result).TrimEnd(Array.Empty<char>());
            Log.Debug($"Player {sender.LogName} spawned successfully ({num[1]}) and unsuccessfully ({num[0]}) players as Ghosts.", Config.Debug);
            return num[1] > 0;
        }

        internal const string _command = "spawn";
        internal const string _description = "Spawn chosen player(s) as Ghost. Use \"dpl\" to spawn Ghost(s) at their death's position. Separate entries with space.";
        internal static readonly string[] _aliases = new[] { "s" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
