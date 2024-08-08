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
    public class EnableVoicechat : ICommand, IUsageProvider
    {
        public EnableVoicechat()
        {
            translation = Translation.AccessTranslation();
            commandName = $"{Translation.pluginName}.{this.GetType().Name}";
            Command = !string.IsNullOrWhiteSpace(translation.EnablevoicechatCommand) ? translation.EnablevoicechatCommand : _command;
            Description = translation.EnablevoicechatDescription;
            Aliases = translation.EnablevoicechatAliases;
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
            Dictionary<string, List<string>> results = new() { { "success", new() }, { "noperm", new() }, { "alreadyenabled", new() } };
            bool result = false;
            foreach (string chat in chats)
            {
                if (!commandsender.CheckPermission($"gs.listen.{chat}"))
                {
                    results["noperm"].Add(chat);
                    Log.Debug($"Player {commandsender.Nickname} doesn't have permission to listen to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
                    continue;
                }
                if (commandsender.TemporaryData.Add(OtherExtensions.voiceChats[chat].Key, "1"))
                {
                    results["success"].Add(chat);
                    result = true;
                    Log.Debug($"Player {commandsender.Nickname} has enabled listening to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
                    continue;
                }
                results["alreadyenabled"].Add(chat);
                Log.Debug($"Player {commandsender.Nickname} has already enabled listening to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
            }
            response = result ? $"{translation.EnableVoicechatSuccess}: {string.Join(", ", results["success"])}."
                     : results["alreadyenabled"].Count > 0 ? $"{translation.EnablevoicechatFail}: {string.Join(", ", results["alreadyenabled"])}."
                     : $"{translation.EnablevoicechatFailNoperm}: {string.Join(", ", results["noperm"])}.";
            Log.Debug($"Player {commandsender.Nickname} {(result ? "" : "un")}successfully enabled themselves {(result ? results["success"].Count : results["noperm"].Count + results["alreadyenabled"].Count)} voicechat(s).", Config.Debug, commandName);
            return result;
        }

        internal const string _command = "enablevoicechat";

        internal const string _description = "Enable listening to selected voicechat(s).";

        internal static readonly string[] _aliases = new[] { "evc" };

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
