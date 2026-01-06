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

namespace GhostSpectator.Commands.ClientConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class CheckWaveInfo : ICommand
    {
        public CheckWaveInfo()
        {
            Translation translation = Translation.AccessTranslation();
            Command = translation.CheckwaveinfoCommand ?? _command;
            Description = translation.CheckwaveinfoDescription;
            Aliases = translation.CheckwaveinfoAliases;
            Log.Debug($"Registered {this.Command} command.", translation.Debug);
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
            response = Translation.CheckwaveinfoSuccess.Replace("%timermtf%", mtfTimer).Replace("%tokensmtf%", mtfTokens).Replace("%timerci%", ciTimer).Replace("%tokensci%", ciTokens);
            return true;
        }

        internal const string _command = "checkwaveinfo";
        internal const string _description = "Check timers and tokens.";
        internal static readonly string[] _aliases = new[] { "timer", "time" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
