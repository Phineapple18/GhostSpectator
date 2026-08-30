using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Commands.ClientConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Deathmatch : ICommand, IUsageProvider
    {
        public Deathmatch()
        {
            Translation translation = Translation.AccessTranslation();
            Command = translation.DeatchmatchCommand ?? _command;
            Description = translation.DeatchmatchDescription;
            Aliases = translation.DeatchmatchAliases;
            Usage = new[] { "join/leave" };
            Log.Info($"Registered {this.Command} command.");
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender == null)
            {
                response = Translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            if (!sender.HasPermissions("gs.deathmatch"))
            {
                response = Translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            if (arguments.Count == 0)
            {
                response = $"{Description} {Translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide any argument.", Config.Debug);
                return false;
            }
            if (!Decontamination.IsDecontaminating && arguments.At(0).ToLower() != "leave")
            {
                response = Translation.DecontaminationNotStarted;
                Log.Debug($"Player {sender.LogName} tried to use this command before decontamination start.", Config.Debug);
                return false;
            }
            if (int.TryParse(arguments.At(0), out int id))
            {
                Player dummy = Player.Get(id);
                if (dummy != null && dummy.IsDummy && !(dummy.HasPendingDuel() && dummy.HasActiveDuel()))
                {
                    if (dummy.IsInDeathmatch())
                    {
                        dummy.LeaveDeathmatch(arguments.Count == 2 && arguments.At(1) == "respawn");
                        response = $"Dummy {dummy.Nickname} left deathmatch.";
                    }
                    else
                    {
                        dummy.JoinDeathmatch();
                        response = $"Dummy {dummy.Nickname} joined deathmatch.";
                    }
                    return true;
                }
            }
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost() && commandsender.IsAlive && arguments.At(0).ToLower() != "leave")
            {
                response = Translation.MustBeDeadOrGhost;
                return false;
            }
            if (arguments.At(0).ToLower() == "join")
            {
                if (DeathmatchExtensions.DeathmatchPlayers.Contains(commandsender))
                {
                    response = Translation.DeatchmatchJoinFail;
                    Log.Debug($"Player {commandsender.Nickname} already joined a deatchmatch.", Config.Debug);
                    return false;
                }
                commandsender.JoinDeathmatch();
                response = Translation.DeatchmatchJoinSuccess;
                Log.Debug($"Player {commandsender.Nickname} joined a deatchmatch.", Config.Debug);
                return true;
            }
            if (arguments.At(0).ToLower() == "leave")
            {
                if (!DeathmatchExtensions.DeathmatchPlayers.Contains(commandsender))
                {
                    response = Translation.DeatchmatchLeaveFail;
                    Log.Debug($"Player {commandsender.Nickname} is not in a deatchmatch.", Config.Debug);
                    return false;
                }
                commandsender.LeaveDeathmatch(arguments.Count == 2 && arguments.At(1) == "respawn");
                response = Translation.DeatchmatchLeaveSuccess;
                Log.Debug($"Player {commandsender.Nickname} left a deatchmatch.", Config.Debug);
                return true;
            }
            response = Translation.WrongArgument;
            Log.Debug($"Player {sender.LogName} provided non-existent argument(s).", Config.Debug);
            return false;
        }

        internal const string _command = "deathmatch";
        internal const string _description = "Join or leave a deathmatch with other Ghosts in Light Containment Zone after decontamination.";
        internal static readonly string[] _aliases = new[] { "dmt" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
