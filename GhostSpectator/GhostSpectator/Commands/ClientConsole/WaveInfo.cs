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
    public class WaveInfo : ICommand
    {
        public WaveInfo()
        {
            Translation translation = Translation.AccessTranslation();
            Command = translation.WaveInfoCommand ?? _command;
            Description = translation.WaveInfoDescription;
            Aliases = translation.WaveInfoAliases;
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
            if (!sender.HasPermissions("gs.waveinfo"))
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
            Player commandsender = Player.Get(sender);
            if (!commandsender.IsGhost())
            {
                response = Translation.NotGhost;
                Log.Debug($"Player {commandsender.Nickname} is not a Ghost.", Config.Debug);
                return false;
            }
            string mtfTimer = ((int)RespawnWaves.PrimaryMtfWave.TimeLeft).ToString();
            string mtfTokens = ((int)RespawnWaves.PrimaryMtfWave.RespawnTokens).ToString();
            string ciTimer = ((int)RespawnWaves.PrimaryChaosWave.TimeLeft).ToString();
            string ciTokens = ((int)RespawnWaves.PrimaryChaosWave.RespawnTokens).ToString();
            response = Translation.WaveInfoSuccess.Replace("%timermtf%", mtfTimer).Replace("%tokensmtf%", mtfTokens).Replace("%timerci%", ciTimer).Replace("%tokensci%", ciTokens);
            return true;
        }

        internal const string _command = "waveinfo";
        internal const string _description = "Check timers and tokens.";
        internal static readonly string[] _aliases = new[] { "wi", "vtimer", "vtime" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
