using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Features.Extensions;
using Log = LabApi.Features.Console.Logger;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;

namespace GhostSpectator.Commands.ClientConsole.Voicechat
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class EnableVoicechat : ICommand, IUsageProvider
    {
        public EnableVoicechat()
        {
            translation = Translation.AccessTranslation();
            Command = translation.EnablevoicechatCommand ?? _command;
            Description = translation.EnablevoicechatDescription;
            Aliases = translation.EnablevoicechatAliases;
            Usage = new[] { $"{string.Join("/", Other.voiceChats.ToArray())}/all" };
            Log.Debug($"Registered {this.Command} command.", translation.Debug);
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
            if (arguments.IsEmpty())
            {
                response = $"{Description} {translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide any argument.", Config.Debug);
                return false;
            }
            IEnumerable<string> chats = arguments.Contains("all") ? Other.voiceChats : Other.voiceChats.Intersect(arguments);
            if (chats.IsEmpty())
            {
                response = translation.WrongArgument;
                Log.Debug($"Player {sender.LogName} provided non-existent argument(s).", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            StringBuilder success = StringBuilderPool.Shared.Rent();
            StringBuilder failure = StringBuilderPool.Shared.Rent();
            StringBuilder failurePerm = StringBuilderPool.Shared.Rent();
            success.Append($"{translation.EnableVoicechatSuccess}:");
            failure.Append($"{translation.EnablevoicechatFail}:");
            failurePerm.Append($"{translation.EnablevoicechatPermFail}:");
            int[] num = new int[3] { 0, 0, 0 };
            foreach (string chat in chats)
            {
                if (!commandsender.HasPermissions($"gs.listen.{chat}"))
                {
                    failurePerm.Append($" {chat},");
                    num[2]++;
                    Log.Debug($"Player {commandsender.Nickname} doesn't have permission to listen to {chat}s.", Config.Debug);
                    continue;
                }
                if (commandsender.GetGhostComponent().VoiceChats.Add(chat))
                {
                    success.Append($" {chat},");
                    num[0]++;
                    continue;
                }
                failure.Append($" {chat},");
                num[1]++;
                Log.Debug($"Player {commandsender.Nickname} has already enabled listening to {chat}s.", Config.Debug);
            }
            StringBuilder result = num[0] > 0 ? success : num[1] == 0 ? failurePerm : num[2] == 0 ? failure : failure.Append(failurePerm);
            result.Replace(',', '.', result.Length - 1, 1);
            response = StringBuilderPool.Shared.ToStringReturn(result).TrimEnd(Array.Empty<char>());
            Log.Debug($"Player {sender.LogName} enabled successfully ({num[0]}) and unsuccessfully ({num[1] + num[2]}) voicechats.", Config.Debug);
            return num[1] > 0;
        }

        internal const string _command = "enablevoicechat";
        internal const string _description = "Enable listening to chosen voicechat(s).";
        internal static readonly string[] _aliases = new[] { "evc" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
    }
}
