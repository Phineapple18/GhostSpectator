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

namespace GhostSpectator.Commands.ClientConsole.Voicechat
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class EnableVoicechat : ICommand, IUsageProvider
    {
        public EnableVoicechat()
        {
            Translation translation = Translation.AccessTranslation();
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
            if (arguments.IsEmpty())
            {
                response = $"{Description} {Translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide any argument.", Config.Debug);
                return false;
            }
            IEnumerable<string> chats = arguments.Contains("all") ? Other.voiceChats : Other.voiceChats.Intersect(arguments);
            if (chats.IsEmpty())
            {
                response = Translation.WrongArgument;
                Log.Debug($"Player {sender.LogName} provided non-existent argument(s).", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            StringBuilder success = StringBuilderPool.Shared.Rent();
            StringBuilder failure = StringBuilderPool.Shared.Rent();
            StringBuilder failurePerm = StringBuilderPool.Shared.Rent();
            success.Append($"{Translation.EnableVoicechatSuccess}:");
            failure.Append($"{Translation.EnablevoicechatFail}:");
            failurePerm.Append($"{Translation.EnablevoicechatPermFail}:");
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
            Log.Debug($"Player {commandsender.Nickname} enabled successfully ({num[0]}) and unsuccessfully ({num[1] + num[2]}) voicechats.", Config.Debug);
            return num[1] > 0;
        }

        internal const string _command = "enablevoicechat";
        internal const string _description = "Enable listening to chosen voicechat(s).";
        internal static readonly string[] _aliases = new[] { "evc" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
