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
                Log.Debug("Command sender doesn't exist.", Config.Debug, commandName);
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide any argument.", Config.Debug, commandName);
                return false;
            }
            IEnumerable<string> chats = arguments.Contains("all") ? OtherExtensions.voiceChats.Keys : OtherExtensions.voiceChats.Keys.Intersect(arguments);
            if (chats.IsEmpty())
            {
                response = translation.WrongArgument;
                Log.Debug($"Player {sender.LogName} provided non-existent argument(s).", Config.Debug, commandName);
                return false;
            }
            Player commandsender = Player.Get(sender);
            StringBuilder success = StringBuilderPool.Shared.Rent();
            StringBuilder failure = StringBuilderPool.Shared.Rent();
            success.Append($"{translation.EnableVoicechatSuccess}:");
            failure.Append($"{translation.EnablevoicechatFail}:");
            int numS = 0;
            int numF = 0;
            foreach (string chat in chats)
            {
                if (!commandsender.CheckPermission($"gs.listen.{chat}"))
                {
                    failure.Append($" {chat},");
                    numF++;
                    Log.Debug($"Player {commandsender.Nickname} doesn't have permission to listen to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
                    continue;
                }
                if (commandsender.TemporaryData.Add(OtherExtensions.voiceChats[chat].Key, "1"))
                {
                    success.Append($" {chat},");
                    numS++;
                    continue;
                }
                failure.Append($" {chat},");
                Log.Debug($"Player {commandsender.Nickname} has already enabled listening to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
            }
            StringBuilder result = numS == 0 ? failure : success;
            result.Replace(',', '.', result.Length - 1, 1);
            response = StringBuilderPool.Shared.ToStringReturn(result).TrimEnd(Array.Empty<char>());
            Log.Debug($"Player {sender.LogName} enabled successfully ({numS}) and unsuccessfully ({numF}) voicechats.", Config.Debug, commandName);
            return numS > 0;
        }

        internal const string _command = "enablevoicechat";

        internal const string _description = "Enable listening to chosen voicechat(s).";

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
