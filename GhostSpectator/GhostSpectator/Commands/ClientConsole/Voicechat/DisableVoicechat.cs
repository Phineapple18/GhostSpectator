using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Extensions;
using NWAPIPermissionSystem;
using PluginAPI.Core;

namespace GhostSpectator.Commands.ClientConsole.Voicechat
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class DisableVoicechat : ICommand, IUsageProvider
    {
        public DisableVoicechat()
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(translation.DisablevoicechatCommand) ? translation.DisablevoicechatCommand : _command;
            Description = translation.DisablevoicechatDescription;
            Aliases = translation.DisablevoicechatAliases;
            Usage = new[] { $"{string.Join("/", OtherExtensions.voiceChats.Keys.ToArray())}/all" };
            Log.Debug($"Registered {this.Command} command.", translation.Debug, Translation.pluginName);
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
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide arguments for command.", Config.Debug, commandName);
                return false;
            }
            if (!arguments.Any(a => OtherExtensions.voiceChats.Keys.Contains(a.ToLower()) || a.ToLower() == "all"))
            {
                response = translation.WrongArgument;
                Log.Debug($"Player {sender.LogName} provided nonexistent argument: {arguments.ElementAt(0)}.", Config.Debug, commandName);
                return false;
            }
            Player commandsender = Player.Get(sender);
            IEnumerable<string> chats = arguments.Any(a => a.ToLower() == "all") ? OtherExtensions.voiceChats.Keys : arguments.Intersect(OtherExtensions.voiceChats.Keys);
            Dictionary<string, List<string>> results = new() { { "success", new() }, { "noperm", new() }, { "alreadydisabled", new() } };
            bool result = false;
            foreach (string chat in chats)
            {
                if (!commandsender.CheckPermission($"gs.listen.{chat}"))
                {
                    results["noperm"].Add(chat);
                    Log.Debug($"Player {commandsender.Nickname} doesn't have permission to disable listening to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
                    continue;
                }
                if (commandsender.TemporaryData.Remove(OtherExtensions.voiceChats[chat].Key))
                {
                    results["success"].Add(chat);
                    result = true;
                    Log.Debug($"Player {commandsender.Nickname} has disabled listening to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
                    continue;
                }
                results["alreadydisabled"].Add(chat);
                Log.Debug($"Player {commandsender.Nickname} has already disabled listening to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
            }
            response = result ? $"{translation.DisablevoicechatSuccess}: {string.Join(", ", results["success"])}." 
                     : results["alreadydisabled"].Count > 0 ? $"{translation.DisablevoicechatFail}: {string.Join(", ", results["alreadydisabled"])}." 
                     : $"{translation.DisablevoicechatFailNoperm}: {string.Join(", ", results["noperm"])}.";
            Log.Debug($"Player {commandsender.Nickname} {(result ? "" : "un")}successfully disabled themselves {(result ? results["success"].Count : results["noperm"].Count + results["alreadydisabled"].Count)} voicechat(s).", Config.Debug, commandName);
            return result;
        }

        internal const string _command = "disablevoicechat";

        internal const string _description = "Disable listening to selected voicechat(s).";

        internal static readonly string[] _aliases = new[] { "dvc" };

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
