using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Extensions;
using NorthwoodLib.Pools;
using NWAPIPermissionSystem;
using PluginAPI.Core;
using Utils;

namespace GhostSpectator.Commands.RemoteAdminConsole
{
    public class Spawn : ICommand, IUsageProvider
	{
        public Spawn(string command, string description, string[] aliases)
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(command) ? command : _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "%player%/all" };
            Log.Debug($"Registered {this.Command} subcommand.", translation.Debug, Translation.pluginName);
        }

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
            if (Plugin.Singleton == null)
			{
                response = translation.NotEnabled;
                Log.Debug($"Plugin {Translation.pluginName} is not enabled.", translation.Debug, commandName);
				return false;
			}
            if (sender == null)
            {
                response = translation.SenderNull;
                Log.Debug("Command sender is null.", Config.Debug, commandName);
                return false;
            }
            if (!sender.CheckPermission("gs.spawn.other"))
            {
                response = translation.NoPerms;
                Log.Debug($"Player {sender.LogName} doesn't have required permission to use this command.", Config.Debug, commandName);
                return false;
            }
            if (!Round.IsRoundStarted)
			{
				response = translation.RoundNotStarted;
                Log.Debug("This command can't be used before round start.", Config.Debug, commandName);
                return false;
			}
            if (Warhead.IsDetonated && Config.DespawnOnDetonation && !sender.CheckPermission("gs.warhead"))
			{
				response = translation.WarheadDetonated;
                Log.Debug($"Player {sender.LogName} can't use this command after warhead detonation.", Config.Debug, commandName);
                return false;
			}
            if (arguments.IsEmpty())
			{
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide arguments for command.", Config.Debug, commandName);
                return false;
			}
            List<ReferenceHub> validHubs = arguments.At(0).ToLower() == "all" ? ReferenceHub.AllHubs.ToList() : RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out string[] _);
            if (validHubs.IsEmpty())
			{
                response = translation.NoPlayers;
                Log.Debug("Provided player(s) doesn't exist.", Config.Debug, commandName);
                return false;
            }
            if (validHubs.Count == 1 && validHubs[0].isLocalPlayer)
            {
                response = translation.DedicatedServer;
                Log.Debug($"Player {sender.LogName} attempted to use this command on Dedicated Server.", Config.Debug, commandName);
                return false;
            }
            validHubs.Remove(Server.Instance.ReferenceHub);
            StringBuilder success = StringBuilderPool.Shared.Rent(); 
            StringBuilder failure = StringBuilderPool.Shared.Rent();
            success.AppendLine($"{translation.SpawnSuccess}:");
            failure.AppendLine($"{translation.SpawnFail}:");
            int numS = 0;
            int numF = 0;
            foreach (ReferenceHub hub in validHubs)
			{
				Player player = Player.Get(hub);
                if (!player.IsGhost())
                {
                    GhostExtensions.Spawn(player);
                    success.AppendLine($"- {player.Nickname}");
                    numS++;
                    continue;
                }
                failure.AppendLine($"- {player.Nickname}");
                numF++;
                Log.Debug($"Player {player.Nickname} is already a Ghost.", Config.Debug, commandName);
            }
            success.Replace("%count%", numS.ToString());
            failure.Replace("%count%", numF.ToString());
            StringBuilder result = numS == 0 ? failure : numF == 0 ? success : success.Append(failure);
            response = StringBuilderPool.Shared.ToStringReturn(result).TrimEnd(Array.Empty<char>());
            Log.Debug($"Player {sender.LogName} spawned successfully ({numS}) and unsuccessfully ({numF}) players as Ghosts.", Config.Debug, commandName);
            return true;
		}

        internal const string _command = "spawn";

        internal const string _description = "Spawn selected player(s) as Ghost. Separate entries with space.";

        internal static readonly string[] _aliases = new[] { "s" };

        private readonly string commandName;

        private readonly Translation translation;
        
        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        public bool SanitizeResponse { get; }
        private static Config Config => Plugin.Singleton.pluginConfig;
    }
}
