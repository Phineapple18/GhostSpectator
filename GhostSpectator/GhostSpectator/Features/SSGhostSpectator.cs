using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using GhostSpectator.Commands.ClientConsole;
using GhostSpectator.Commands.ClientConsole.Duelling;
using GhostSpectator.Commands.ClientConsole.Toys;
using GhostSpectator.Commands.ClientConsole.Voicechat;
using GhostSpectator.Features.Extensions;
using Hints;
using RemoteAdmin;
using UserSettings.ServerSpecific;
using UserSettings.ServerSpecific.Examples;
using Utils.NonAllocLINQ;
using static TMPro.TMP_InputField;
using static UserSettings.ServerSpecific.SSDropdownSetting;
using Log = LabApi.Features.Console.Logger;

namespace GhostSpectator.Features
{
    public class SSGhostSpectator
    {
        public SSGhostSpectator()
        {
            currentPage = Translation.CurrentPage ?? "Current Page";
            Headers = Translation.PageHeaders?.Count(h => h != null) == 4 ? Translation.PageHeaders : _headers;
            DuelActions = Translation.DuelActions?.Count(h => h != null) == 5 ? Translation.DuelActions : _duelActions;
            Selections = Translation.Selections?.Count(h => h != null) == 4 ? Translation.Selections : _selections;
        }

        public void Enable()
        {
            pinnedSection = new[] { pageSelector = new(null, currentPage, Headers, 0, DropdownEntryType.HybridLoop) };
            lastSentPages = new();
            lastVoicechatSettings = new();
            pages = new SSPagesExample.SettingsPage[]
            {
                new(Headers[0], new[]
                {
                    duelSettings[0] = new SSPlaintextSetting(null, Translation.GhostName, "...", 15, ContentType.Standard, Translation.FullPartNickname, 255, true),
                    duelSettings[1] = new SSDropdownSetting(null, Selections[0], DuelActions),
                    duelSettings[2] = new SSButton(null, Translation.ActionExecute, Translation.Hold, 0.5f),
                    duelSettings[3] = new SSTextArea(null, string.Empty),
                    firearmSettings[0] = new SSGroupHeader(Translation.Firearms),
                    firearmSettings[1] = new SSDropdownSetting(null, Selections[1], GiveFirearm.firearmList.Select(f => f.ToString()).ToArray()),
                    firearmSettings[2] = new SSButton(null, Translation.FirearmGive, Translation.Press),
                    deathmatchSettings[0] = new SSGroupHeader(Translation.DeathMatch),
                    deathmatchSettings[1] = new SSButton(null, Translation.DeathmatchJoin, Translation.Press),
                    deathmatchSettings[2] = new SSButton(null, Translation.DeathmatchLeave, Translation.Press)
                }),
                new(Headers[1], new[]
                {
                    toySettings[0] = new SSDropdownSetting(null, Selections[2], ToyExtensions.toyNames.Values.ToArray()),
                    toySettings[1] = new SSButton(null, Translation.ToyCreate, Translation.Press),
                    toySettings[2] = new SSGroupHeader (Translation.ToyManage),
                    toySettings[3] = new SSButton(null, Translation.ToyList, Translation.Press),
                    toySettings[4] = new SSTextArea(null, string.Empty),
                    toySettings[5] = new SSPlaintextSetting(null, Translation.ToyNetid, "...", 3, ContentType.Standard, null, 255, true),
                    toySettings[6] = new SSButton(null, Translation.ToyDestroy, Translation.Hold, 1f),
                    toySettings[7] = new SSGroupHeader (Translation.ToyAreas),
                    toySettings[8] = new SSDropdownSetting (null, Selections[3], ToyExtensions.ToySpawnAreas.Select(a => a.Name).ToArray()),
                    toySettings[9] = new SSButton(null, Translation.AreaTeleport, Translation.Hold, 0.5f),
                }),
                new(Headers[2], new[]
                {
                    voicechatSettings[0] = new SSTwoButtonsSetting(null, Translation.Ghosts, Translation.Enabled, Translation.Disabled, true, Translation.PermissionNeeded),
                    voicechatSettings[1] = new SSTwoButtonsSetting(null, Translation.Scps, Translation.Enabled, Translation.Disabled, true, Translation.PermissionNeeded),
                    voicechatSettings[2] = new SSTwoButtonsSetting(null, Translation.Spectators, Translation.Enabled, Translation.Disabled, true, Translation.PermissionNeeded),
                }),
                new(Headers[3], new[]
                {
                    waveSettings[0] = new SSButton(null, Translation.WaveInfo, Translation.Press),
                    waveSettings[1] = new SSTextArea(null, string.Empty)
                }),
            };
            pages.ForEach(page => page.GenerateCombinedEntries(pinnedSection));
            List<ServerSpecificSettingBase> allSettings = new(pinnedSection);
            pages.ForEach(page => allSettings.AddRange(page.OwnEntries));
            ghostspectatorSettings = allSettings.ToArray();
            ServerSpecificSettingsSync.DefinedSettings = ghostspectatorSettings;
            ServerSpecificSettingsSync.SendOnJoinFilter = _ => false;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += this.ProcessUserInput;
            Log.Debug($"Enabled server-specific settings for {MainClass.Instance.Name} plugin.", Config.Debug);
        }

        public void Disable()
        {
            Singleton = null;
            ServerSpecificSettingsSync.SendOnJoinFilter = null;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= this.ProcessUserInput;
            Log.Debug($"Disabled server-specific settings for {MainClass.Instance.Name} plugin.", Config.Debug);
        }

        internal void ActivateForHub(ReferenceHub referenceHub)
        {
            lastSentPages.Add(referenceHub, 0);
            ServerSpecificSettingsSync.SendToPlayer(referenceHub, pages[0].CombinedEntries, null);
            Log.Debug($"Activated server-specific settings for player {referenceHub.nicknameSync.MyNick}.", Config.Debug);
        }

        internal void DeactivateForHub(ReferenceHub referenceHub)
        {
            if (lastSentPages.Remove(referenceHub))
            {
                lastVoicechatSettings.Remove(referenceHub);
                ServerSpecificSettingsSync.SendToPlayer(referenceHub, null, null);
                Log.Debug($"Deactivated server-specific settings for player {referenceHub.nicknameSync.MyNick}.", Config.Debug);
            }
        }

        private void ProcessUserInput(ReferenceHub referenceHub, ServerSpecificSettingBase setting)
        {
            if (ServerSpecificSettingsSync.DefinedSettings != ghostspectatorSettings)
            {
                Log.Debug("Current DefinedSettings doesn't belong to GhostSpectator, skipped.", Config.Debug);
                return;
            }
            try
            {
                if (setting is SSDropdownSetting ssdropdownSetting && ssdropdownSetting.SettingId == pageSelector.SettingId)
                {
                    this.ChangePage(referenceHub, ssdropdownSetting.SyncSelectionIndexValidated);
                    return;
                }
                ICommand command;
                string argument;
                if (setting is SSTwoButtonsSetting ssTwoButton)
                {
                    argument = VoicechatParent.voiceChats.ElementAt(Array.FindIndex(voicechatSettings, v => v.SettingId == ssTwoButton.SettingId));
                    if (lastVoicechatSettings.Any(s => s.Key == referenceHub && s.Value.ContainsKey(argument) && s.Value[argument] == ssTwoButton.DebugValue))
                    {
                        Log.Debug($"Player {referenceHub.nicknameSync.MyNick} has already set this option, skipped.", Config.Debug);
                        return;
                    }
                    command = QueryProcessor.DotCommandHandler.AllCommands.First(c => ssTwoButton.SyncIsA ? c is EnableVoicechat : c is DisableVoicechat);
                    if (lastVoicechatSettings.ContainsKey(referenceHub))
                    {
                        lastVoicechatSettings[referenceHub][argument] = ssTwoButton.DebugValue;
                    }
                    else
                    {
                        lastVoicechatSettings.Add(referenceHub, new Dictionary<string, string> { { argument, ssTwoButton.DebugValue } });
                    }
                    this.HandleCommand(command, referenceHub, new string[] { argument }, null, true);
                    return;
                }
                ParentCommand parentCommand;
                if (setting is SSButton ssButton)
                {
                    switch (ssButton.SettingId)
                    {
                        case int i when i == duelSettings[2].SettingId:
                            parentCommand = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is DuelParent) as ParentCommand;
                            int action = ServerSpecificSettingsSync.GetSettingOfUser<SSDropdownSetting>(referenceHub, duelSettings[1].SettingId).SyncSelectionIndexRaw;
                            command = parentCommand.AllCommands.ElementAt(action);
                            argument = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(referenceHub, duelSettings[0].SettingId).SyncInputText;
                            this.HandleCommand(command, referenceHub, new string[] { argument }, command is ListDuel ? (SSTextArea)duelSettings[3] : null, true);
                            return;
                        case int i when i == firearmSettings[2].SettingId:
                            command = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is GiveFirearm);
                            argument = ServerSpecificSettingsSync.GetSettingOfUser<SSDropdownSetting>(referenceHub, firearmSettings[1].SettingId).SyncSelectionText;
                            this.HandleCommand(command, referenceHub, new string[] { argument }, null, true);
                            return;
                        case int i when i == deathmatchSettings[1].SettingId:
                            command = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is Deathmatch);
                            this.HandleCommand(command, referenceHub, new string[] { "join" }, null, true);
                            return;
                        case int i when i == deathmatchSettings[2].SettingId:
                            command = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is Deathmatch);
                            this.HandleCommand(command, referenceHub, new string[] { "leave" }, null, true);
                            return;
                        case int i when i == toySettings[1].SettingId:
                            parentCommand = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is ToyParent) as ParentCommand;
                            command = parentCommand.AllCommands.First(c => c is Create);
                            argument = ServerSpecificSettingsSync.GetSettingOfUser<SSDropdownSetting>(referenceHub, toySettings[0].SettingId).SyncSelectionText;
                            this.HandleCommand(command, referenceHub, new string[] { argument }, null, true);
                            return;
                        case int i when i == toySettings[3].SettingId:
                            parentCommand = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is ToyParent) as ParentCommand;
                            command = parentCommand.AllCommands.First(c => c is ListToy);
                            this.HandleCommand(command, referenceHub, new string[0], (SSTextArea)toySettings[4], false);
                            return;
                        case int i when i == toySettings[6].SettingId:
                            parentCommand = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is ToyParent) as ParentCommand;
                            command = parentCommand.AllCommands.First(c => c is Destroy);
                            argument = ServerSpecificSettingsSync.GetSettingOfUser<SSPlaintextSetting>(referenceHub, toySettings[5].SettingId).SyncInputText;
                            this.HandleCommand(command, referenceHub, new string[] { argument }, null, true);
                            return;
                        case int i when i == toySettings[9].SettingId:
                            command = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is ToyAreaTeleport);
                            argument = ServerSpecificSettingsSync.GetSettingOfUser<SSDropdownSetting>(referenceHub, toySettings[8].SettingId).SyncSelectionText;
                            this.HandleCommand(command, referenceHub, new string[] { argument }, null, true);
                            return;
                        case int i when i == waveSettings[0].SettingId:
                            command = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is CheckWaveInfo);
                            this.HandleCommand(command, referenceHub, new string[0], (SSTextArea)waveSettings[1]);
                            return;
                        default:
                            Log.Debug("Button not found.", Config.Debug);
                            return;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Debug($"Skipped processing input for {MainClass.Instance.Name} due to an error: {exception}.", Config.Debug);
            }
        }

        private void HandleCommand(ICommand command, ReferenceHub referenceHub, string[] arguments, SSTextArea textToUpdate = null, bool receiveHint = false)
        {
            bool success = command.Execute(new(arguments), new PlayerCommandSender(referenceHub), out string response);
            textToUpdate?.SendTextUpdate(response, true, h => h == referenceHub);
            referenceHub.gameConsoleTransmission.SendToClient(response, success ? "green" : "magenta");
            if (receiveHint && (!success || command is not Accept and not ListDuel || command is Deathmatch && arguments.Contains("leave")))
            {
                referenceHub.hints.Show(new TextHint(response, new HintParameter[] { new StringHintParameter(response) }));
            }
            Log.Debug($"Player {referenceHub.nicknameSync.MyNick} used setting {command.Command}.", Config.Debug);
        }

        private void ChangePage(ReferenceHub referenceHub, int settingIndex)
        {
            if (lastSentPages.TryGetValue(referenceHub, out int num) && num == settingIndex)
            {
                return;
            }
            lastSentPages[referenceHub] = settingIndex;
            ServerSpecificSettingsSync.SendToPlayer(referenceHub, pages[settingIndex].CombinedEntries, null);
            Log.Debug($"Player {referenceHub.nicknameSync.MyNick} changed server-specific settings page to {settingIndex}.", Config.Debug);
        }

        internal static readonly string[] _duelActions = new string[5] { "Accept a duel request", "Cancel a duel (request)", "Challenge to a duel", "Print a list", "Reject a duel" };
        internal static readonly string[] _headers = new string[4] { "Duelling & Deathmatch", "Toys", "Voice chats", "Wave timers" };
        internal static readonly string[] _selections = new string[4] { "Select an action", "Select a firearm", "Select a toy type", "Select a toy area" };

        private static string currentPage;

        internal Dictionary<ReferenceHub, int> lastSentPages;
        private Dictionary<ReferenceHub, Dictionary<string, string>> lastVoicechatSettings;

        private ServerSpecificSettingBase[] ghostspectatorSettings;
        private SSPagesExample.SettingsPage[] pages;
        private SSDropdownSetting pageSelector;
        private ServerSpecificSettingBase[] pinnedSection;

        private readonly ServerSpecificSettingBase[] deathmatchSettings = new ServerSpecificSettingBase[3];
        private readonly ServerSpecificSettingBase[] duelSettings = new ServerSpecificSettingBase[4];
        private readonly ServerSpecificSettingBase[] firearmSettings = new ServerSpecificSettingBase[3];
        private readonly ServerSpecificSettingBase[] toySettings = new ServerSpecificSettingBase[10];
        private readonly SSTwoButtonsSetting[] voicechatSettings = new SSTwoButtonsSetting[3];
        private readonly ServerSpecificSettingBase[] waveSettings = new ServerSpecificSettingBase[2];

        private string[] DuelActions { get; }
        private string[] Headers { get; }
        private string[] Selections { get; }
        public static SSGhostSpectator Singleton { get; internal set; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
