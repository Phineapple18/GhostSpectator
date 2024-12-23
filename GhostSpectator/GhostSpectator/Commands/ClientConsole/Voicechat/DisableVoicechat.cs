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
            success.Append($"{translation.DisablevoicechatSuccess}:");
            failure.Append($"{translation.DisablevoicechatFail}:");
            int numS = 0;
            int numF = 0;
            foreach (string chat in chats)
            {
                if (!commandsender.CheckPermission($"gs.listen.{chat}"))
                {
                    failure.Append($" {chat},");
                    numF++;
                    Log.Debug($"Player {commandsender.Nickname} doesn't have permission to disable listening to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
                    continue;
                }
                if (commandsender.TemporaryData.Remove(OtherExtensions.voiceChats[chat].Key))
                {
                    success.Append($" {chat},");
                    numS++;
                    continue;
                }
                failure.Append($" {chat},");
                Log.Debug($"Player {commandsender.Nickname} has already disabled listening to {OtherExtensions.voiceChats[chat].Value}.", Config.Debug, commandName);
            }
            StringBuilder result = numS == 0 ? failure : success;
            result.Replace(',', '.', result.Length - 1, 1);
            response = StringBuilderPool.Shared.ToStringReturn(result).TrimEnd(Array.Empty<char>());
            Log.Debug($"Player {sender.LogName} disabled successfully ({numS}) and unsuccessfully ({numF}) voicechats.", Config.Debug, commandName);
            return numS > 0;
        }

        internal const string _command = "disablevoicechat";

        internal const string _description = "Disable listening to chosen voicechat(s).";

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
