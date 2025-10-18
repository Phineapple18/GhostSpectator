using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using GhostSpectator.Commands.ClientConsole;
using GhostSpectator.Commands.ClientConsole.Duelling;
using GhostSpectator.Commands.ClientConsole.Toys;
using GhostSpectator.Commands.ClientConsole.Voicechat;
using GhostSpectator.Features.Extensions;
using Hints;
using RemoteAdmin;
using static TMPro.TMP_InputField;
using UserSettings.ServerSpecific;
using UserSettings.ServerSpecific.Examples;
using static UserSettings.ServerSpecific.SSDropdownSetting;
using Utils.NonAllocLINQ;

namespace GhostSpectator.Features
{
    public class SSGhostSpectator
    {
        public SSGhostSpectator()
        {
            currentPage = translation.CurrentPage ?? "Current Page";
            Headers = (translation.PageHeaders == null || translation.PageHeaders.Count() != 4 || translation.PageHeaders.Any(h => h == null)) ? _headers : translation.PageHeaders;
            DuelActionOptions = (translation.DuelActions == null || translation.DuelActions.Count() != 4 || translation.DuelActions.Any(h => h == null)) ? _duelActionOptions : translation.DuelActions;
            actionSelect = translation.ActionSelect ?? "Select an action";
            firearmSelect = translation.FirearmSelect ?? "Select firearm";
            toySelect = translation.ToySelect ?? "Select toy type";
            toyAreaSelect = translation.AreaSelect ?? "Select toy area";
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
                    waveSettings[0] = new SSButton(null, translation.WaveInfo, translation.Press),
                    waveSettings[1] = new SSTextArea(null, string.Empty)
                }),
                new(Headers[1], new[]
                {
                    voicechatSettings[0] = new SSTwoButtonsSetting(null, translation.Ghosts, translation.Enabled, translation.Disabled, true, translation.PermissionNeeded),
                    voicechatSettings[1] = new SSTwoButtonsSetting(null, translation.Scps, translation.Enabled, translation.Disabled, true, translation.PermissionNeeded),
                    voicechatSettings[2] = new SSTwoButtonsSetting(null, translation.Spectators, translation.Enabled, translation.Disabled, true, translation.PermissionNeeded),
                }),
                new(Headers[2], new[]
                {
                    toySettings[0] = new SSDropdownSetting(null, toySelect, Toy.names.ToArray()),
                    toySettings[1] = new SSButton(null, translation.ToyCreate, translation.Press),
                    toySettings[2] = new SSGroupHeader (translation.ToyManage),
                    toySettings[3] = new SSButton(null, translation.ToyList, translation.Press),
                    toySettings[4] = new SSTextArea(null, string.Empty),
                    toySettings[5] = new SSPlaintextSetting(null, translation.ToyNetid, "...", 3, ContentType.Standard, null, 255, true),
                    toySettings[6] = new SSButton(null, translation.ToyDestroy, translation.Hold, 1f),
                    toySettings[7] = new SSGroupHeader (translation.ToyAreas),
                    toySettings[8] = new SSDropdownSetting (null, toyAreaSelect, Toy.SpawnAreas.Select(a => a.Name).ToArray()),
                    toySettings[9] = new SSButton(null, translation.AreaTeleport, translation.Hold, 0.5f),
                }),
                new(Headers[3], new[]
                {
                    duelSettings[0] = new SSPlaintextSetting(null, translation.GhostName, "...", 15, ContentType.Standard, translation.FullPartNickname, 255, true),
                    duelSettings[1] = new SSDropdownSetting(null, actionSelect, DuelActionOptions),
                    duelSettings[2] = new SSButton(null, translation.ActionExecute, translation.Hold, 0.5f),
                    duelSettings[3] = new SSTextArea(null, string.Empty),
                    firearmSettings[0] = new SSGroupHeader(translation.Firearms),
                    firearmSettings[1] = new SSDropdownSetting(null, firearmSelect, Other.firearmList.Select(f => f.ToString()).ToArray()),
                    firearmSettings[2] = new SSButton(null, translation.FirearmGive, translation.Press)
                }),
            };
            pages.ForEach(page => page.GenerateCombinedEntries(pinnedSection));
            List<ServerSpecificSettingBase> allSettings = new(pinnedSection);
            pages.ForEach(page => allSettings.AddRange(page.OwnEntries));
            ghostspectatorSettings = allSettings.ToArray();
            ServerSpecificSettingsSync.DefinedSettings = ghostspectatorSettings;
            ServerSpecificSettingsSync.SendOnJoinFilter = _ => false;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += this.ProcessUserInput;
            Log.Debug("Enabled server-specific settings for GhostSpectator plugin.", config.Debug);
        }

        public void Disable()
        {
            Singleton = null;
            ServerSpecificSettingsSync.SendOnJoinFilter = null;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= this.ProcessUserInput;
            Log.Debug("Disabled server-specific settings for GhostSpectator plugin.", config.Debug);
        }

        internal void ActivateForHub(ReferenceHub referenceHub)
        {
            lastSentPages.Add(referenceHub, 0);
            ServerSpecificSettingsSync.SendToPlayer(referenceHub, pages[0].CombinedEntries, null);
            Log.Debug($"Activated server-specific settings for player {referenceHub.nicknameSync.MyNick}.", config.Debug);
        }

        internal void DeactivateForHub(ReferenceHub referenceHub)
        {
            if (lastSentPages.Remove(referenceHub))
            {
                lastVoicechatSettings.Remove(referenceHub);
                ServerSpecificSettingsSync.SendToPlayer(referenceHub, null, null);
                Log.Debug($"Deactivated server-specific settings for player {referenceHub.nicknameSync.MyNick}.", config.Debug);
            }
        }

        private void ProcessUserInput(ReferenceHub referenceHub, ServerSpecificSettingBase setting)
        {
            if (ServerSpecificSettingsSync.DefinedSettings != ghostspectatorSettings)
            {
                Log.Debug("Current DefinedSettings doesn't belong to GhostSpectator, skipped.", config.Debug);
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
                    argument = Other.voiceChats.ElementAt(Array.FindIndex(voicechatSettings, v => v.SettingId == ssTwoButton.SettingId));
                    if (lastVoicechatSettings.Any(s => s.Key == referenceHub && s.Value.ContainsKey(argument) && s.Value[argument] == ssTwoButton.DebugValue))
                    {
                        Log.Debug($"Player {referenceHub.nicknameSync.MyNick} has already set this option, skipped.", config.Debug);
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
                        case int i when i == waveSettings[0].SettingId:
                            command = QueryProcessor.DotCommandHandler.AllCommands.First(c => c is CheckWaveInfo);
                            this.HandleCommand(command, referenceHub, new string[0], (SSTextArea)waveSettings[1]);
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
                        default:
                            Log.Debug("Button not found.", config.Debug);
                            return;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Debug($"Skipped processing input for GhostSpectator due to an error: {exception}.", config.Debug);
            }
        }

        private void HandleCommand(ICommand command, ReferenceHub referenceHub, string[] arguments, SSTextArea textToUpdate = null, bool receiveHint = false)
        {
            bool success = command.Execute(new(arguments), new PlayerCommandSender(referenceHub), out string response);
            textToUpdate?.SendTextUpdate(response, true, h => h == referenceHub);
            referenceHub.gameConsoleTransmission.SendToClient(response, success ? "green" : "magenta");
            if (receiveHint && (command is not Accept and not ListDuel || !success))
            {
                referenceHub.hints.Show(new TextHint(response, new HintParameter[] { new StringHintParameter(response) }));
            }
            Log.Debug($"Player {referenceHub.nicknameSync.MyNick} used setting {command.Command}.", config.Debug);
        }

        private void ChangePage(ReferenceHub referenceHub, int settingIndex)
        {
            if (lastSentPages.TryGetValue(referenceHub, out int num) && num == settingIndex)
            {
                return;
            }
            lastSentPages[referenceHub] = settingIndex;
            ServerSpecificSettingsSync.SendToPlayer(referenceHub, pages[settingIndex].CombinedEntries, null);
            Log.Debug($"Player {referenceHub.nicknameSync.MyNick} changed server-specific settings page to {settingIndex}.", config.Debug);
        }

        internal static readonly string[] _duelActionOptions = new string[5] { "Accept a duel request", "Cancel a duel (request)", "Challenge to a duel", "Print a list", "Reject a duel" };
        internal static readonly string[] _headers = new string[4] { "Wave timers", "Voice chats", "Toys", "Duelling" };

        private static string actionSelect;
        private static string currentPage;
        private static string firearmSelect;
        private static string toySelect;
        private static string toyAreaSelect;

        internal Dictionary<ReferenceHub, int> lastSentPages;
        private Dictionary<ReferenceHub, Dictionary<string, string>> lastVoicechatSettings;

        private ServerSpecificSettingBase[] ghostspectatorSettings;
        private SSPagesExample.SettingsPage[] pages;
        private SSDropdownSetting pageSelector;
        private ServerSpecificSettingBase[] pinnedSection;

        private readonly ServerSpecificSettingBase[] duelSettings = new ServerSpecificSettingBase[4];
        private readonly ServerSpecificSettingBase[] firearmSettings = new ServerSpecificSettingBase[3];
        private readonly ServerSpecificSettingBase[] toySettings = new ServerSpecificSettingBase[10];
        private readonly SSTwoButtonsSetting[] voicechatSettings = new SSTwoButtonsSetting[3];
        private readonly ServerSpecificSettingBase[] waveSettings = new ServerSpecificSettingBase[2];

        private readonly Config config = MainClass.Instance.pluginConfig;
        private readonly Translation translation = MainClass.Instance.pluginTranslation;

        private string[] DuelActionOptions { get; }
        private string[] Headers { get; }
        public static SSGhostSpectator Singleton { get; internal set; }
    }
}
