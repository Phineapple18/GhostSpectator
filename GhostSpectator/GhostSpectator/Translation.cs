using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.IO;

using GhostSpectator.Commands.ClientConsole;
using GhostSpectator.Commands.ClientConsole.Duelling;
using GhostSpectator.Commands.ClientConsole.Toys;
using GhostSpectator.Commands.ClientConsole.Voicechat;
using GhostSpectator.Commands.RemoteAdmin.GhostSettings;
using GhostSpectator.Commands.RemoteAdmin.GhostSpectator;
using GhostSpectator.Features;
using LabApi.Loader.Features.Paths;
using Serialization;


namespace GhostSpectator
{
    public class Translation
    {
        [Description("MISCELLANOUS TRANSLATION. Don't translate words put between two '%'." +
                     "\n# Nickname of Ghost.")]
        public string GhostNickname { get; set; } = "GHOST";

        [Description("Broadcast sent to a Ghost upon spawn.")]
        public string SpawnMessage { get; set; } = "<size=50><color=%colour%>You are a Ghost!</color>" +
                                                   "\n<size=30>- Drop the Lantern to teleport to a random alive player (lit) or a random room (unlit)." +
                                                   "\n- Open your client console to learn about other features.</size>";

        [Description("Console message sent to a Ghost upon spawn.")]
        public string SpawnConsoleMessage { get; set; } = "GhostSpectator features:" +
                                                          "\n- Use command <color=red>\"%ghostset%\"</color> to enable or disable server-specific settings for this plugin." +
                                                          "\n- Use command <color=red>\"%wavetimer%\"</color> to check respawn times." +
                                                          "\n- Use commands <color=red>\"%createtoy%\"</color> or <color=red>\"%destroytoy%\"</color> to learn create or destroy a toy." +
                                                          "\n- Use commands <color=red>\"%enablevc%\"</color> or <color=red>\"disablevc\"</color> to enable or disable specific voicechat." +
                                                          "\n- Use command <color=red>\"%duel%\"</color> to learn about duelling." +
                                                          "\n- Use command <color=red>\"%givegun%\"</color> to give yourself a firearm.";

        [Description("Hints shown to a Ghost, when dropping the teleport item.")]
        public string TeleportPlayerSuccess { get; set; } = "You have been teleported to <color=green>%playernick%</color>.";
        public string TeleportPlayerFail { get; set; } = "There is nobody you can teleport to.";
        public string TeleportRoomFail { get; set; } = "You can't teleport to any room after warhead detonation.";

        [Description("Duel hints shown to a Ghost.")]
        public string DuelAbandoned { get; set; } = "Player <color=red>%playernick%</color> has abandoned the duel with you.";
        public string DuelAborted { get; set; } = "Your duel with <color=red>%playernick%</color> has been aborted.";
        public string DuelPrepare { get; set; } = "Prepare for a duel!";
        public string DuelStarted { get; set; } = "The duel has started!";
        public string DuelWon { get; set; } = "You have won a duel against <color=green>%playernick%</color>.";
        public string DuelLost { get; set; } = "You have a lost duel to <color=red>%playernick%</color>.";
        public string DuelRequestCancelled { get; set; } = "Player <color=red>%playernick%</color> has cancelled their duel request.";
        public string DuelRequestExpired { get; set; } = "Your duel request for <color=red>%playernick%</color> has expired.";
        public string DuelRequestReceived { get; set; } = "Player <color=red>%playernick%</color> has challenged you to a duel!\nUse the client command console to accept or reject it within <color=yellow>%time%s</color> or let the offer expire.";
        public string DuelRequestRejected { get; set; } = "Player <color=red>%playernick%</color> has rejected your duel request.";

        [Description("COMMANDS\' TRANSLATION. Don't translate words put between two '%'." +
                     "\n# Should debug be enabled for command registering?")]
        public bool Debug { get; set; } = false;

        [Description("Translation of GhostSpectator parent command and its subcommands. Make sure not to duplicate commands or aliases." +
                     "\n# GhostSpectator parent command.")]
        public string GhostspectatorParentCommand { get; set; } = GhostSpectatorParent._command;
        public string GhostspectatorParentDescription { get; set; } = GhostSpectatorParent._description;
        public string[] GhostspectatorParentAliases { get; set; } = GhostSpectatorParent._aliases;

        [Description("Despawn command.")]
        public string DespawnCommand { get; set; } = Despawn._command;
        public string DespawnDescription { get; set; } = Despawn._description;
        public string[] DespawnAliases { get; set; } = Despawn._aliases;
        public string DespawnSuccess { get; set; } = "Succesfully despawned %count% Ghost(s).";
        public string DespawnFail { get; set; } = "Command failed for %count% existing player(s) (not a Ghost)";

        [Description("List command.")]
        public string ListghostCommand { get; set; } = List._command;
        public string ListghostDescription { get; set; } = List._description;
        public string[] ListghostAliases { get; set; } = List._aliases;
        public string ListghostSuccess { get; set; } = "List of Ghosts (%count%)";

        [Description("Spawn command.")]
        public string SpawnCommand { get; set; } = Spawn._command;
        public string SpawnDescription { get; set; } = Spawn._description;
        public string[] SpawnAliases { get; set; } = Spawn._aliases;
        public string SpawnSuccess { get; set; } = "Succesfully spawned %count% Ghost(s).";
        public string SpawnFail { get; set; } = "Command failed for %count% existing player(s) (already a Ghost)";

        [Description("Translation of GhostSettings parent command and its subcommands. Make sure not to duplicate commands or aliases." +
                     "\n# GhostSettings parent command.")]
        public string GhostsettingsParentCommand { get; set; } = GhostSettingsParent._command;
        public string GhostsettingsParentDescription { get; set; } = GhostSettingsParent._description;
        public string[] GhostsettingsParentAliases { get; set; } = GhostSettingsParent._aliases;

        [Description("Disable command.")]
        public string DisableCommand { get; set; } = Disable._command;
        public string DisableDescription { get; set; } = Disable._description;
        public string[] DisableAliases { get; set; } = Disable._aliases;
        public string DisableSuccess { get; set; } = "Succesfully disabled server-specific settings for GhostSpectator.";
        public string DisableFail { get; set; } = "Server-specific settings are already disabled for GhostSpectator.";

        [Description("Enable command.")]
        public string EnableCommand { get; set; } = Enable._command;
        public string EnableDescription { get; set; } = Enable._description;
        public string[] EnableAliases { get; set; } = Enable._aliases;
        public string EnableSuccess { get; set; } = "Succesfully enabled server-specific settings for GhostSpectator.";
        public string EnableFail { get; set; } = "Server-specific settings are already enabled for GhostSpectator.";

        [Description("Reload command.")]
        public string ReloadCommand { get; set; } = Reload._command;
        public string ReloadDescription { get; set; } = Reload._description;
        public string[] ReloadAliases { get; set; } = Reload._aliases;
        public string ReloadSuccess { get; set; } = "Succesfully reloaded server-specific settings for GhostSpectator.";

        [Description("Translation of Duel parent command and its subcommands. Make sure not to duplicate commands or aliases." +
                     "\n# Duel parent command.")]
        public string DuelParentCommand { get; set; } = DuelParent._command;
        public string DuelParentDescription { get; set; } = DuelParent._description;
        public string[] DuelParentAliases { get; set; } = DuelParent._aliases;

        [Description("Accept command.")]
        public string AcceptCommand { get; set; } = Accept._command;
        public string AcceptDescription { get; set; } = Accept._description;
        public string[] AcceptAliases { get; set; } = Accept._aliases;
        public string AcceptSuccess { get; set; } = "You have accepted a duel request from %playernick%.";

        [Description("Cancel command.")]
        public string CancelCommand { get; set; } = Cancel._command;
        public string CancelDescription { get; set; } = Cancel._description;
        public string[] CancelAliases { get; set; } = Cancel._aliases;
        public string CancelDuelSuccess { get; set; } = "You have cancelled your duel with %playernick%.";
        public string CancelRequestSuccess { get; set; } = "You have cancelled your duel request to %playernick%.";
        public string CancelFail { get; set; } = "You don't have any active duel nor duel requests.";

        [Description("List command.")]
        public string ListduelCommand { get; set; } = ListDuel._command;
        public string ListduelDescription { get; set; } = ListDuel._description;
        public string[] ListduelAliases { get; set; } = ListDuel._aliases;
        public string ListduelSuccess { get; set; } = "You have challenged following player to a duel: %playernick%\nYou have been challenged to a duel by following player(s): %players%";

        [Description("Challenge command.")]
        public string ChallengeCommand { get; set; } = Challenge._command;
        public string ChallengeDescription { get; set; } = Challenge._description;
        public string[] ChallengeAliases { get; set; } = Challenge._aliases;
        public string ChallengeSuccess { get; set; } = "You have challenged %playernick% to duel.";

        [Description("Reject command.")]
        public string RejectCommand { get; set; } = Reject._command;
        public string RejectDescription { get; set; } = Reject._description;
        public string[] RejectAliases { get; set; } = Reject._aliases;
        public string RejectSuccessPlayer { get; set; } = "You have rejected duel request from %playernick%.";

        [Description("Translation of Toy parent command and its subcommands. Make sure not to duplicate commands or aliases." +
                     "\n# Toy parent command.")]
        public string ToyParentCommand { get; set; } = ToyParent._command;
        public string ToyParentDescription { get; set; } = ToyParent._description;
        public string[] ToyParentAliases { get; set; } = ToyParent._aliases;

        [Description("Create command.")]
        public string CreateCommand { get; set; } = Create._command;
        public string CreateDescription { get; set; } = Create._description;
        public string[] CreateAliases { get; set; } = Create._aliases;
        public string CreateSuccess { get; set; } = "You have created a <color=green>(%toyname%)</color> with ID <color=green>%toyid%</color>.";

        [Description("Destroy command.")]
        public string DestroyCommand { get; set; } = Destroy._command;
        public string DestroyDescription { get; set; } = Destroy._description;
        public string[] DestroyAliases { get; set; } = Destroy._aliases;
        public string DestroySuccess { get; set; } = "You have destroyed your toy (%toyname%) with ID %toyid%.";
        public string DestroyFail { get; set; } = "You don't have any toy created with ID %toyid%.";

        [Description("List command.")]
        public string ListtoyCommand { get; set; } = ListToy._command;
        public string ListtoyDescription { get; set; } = ListToy._description;
        public string[] ListtoyAliases { get; set; } = ListToy._aliases;
        public string ListtoySuccess { get; set; } = "List of spawned toys (%count%)";

        [Description("Translation of other client commands. Make sure not to duplicate commands or aliases." +
                     "\n# DisableVoicechat command.")]
        public string DisablevoicechatCommand { get; set; } = DisableVoicechat._command;
        public string DisablevoicechatDescription { get; set; } = DisableVoicechat._description;
        public string[] DisablevoicechatAliases { get; set; } = DisableVoicechat._aliases;
        public string DisablevoicechatSuccess { get; set; } = "Successfully disabled listening to the following group(s)";
        public string DisablevoicechatFail { get; set; } = "You have already disabled listening to the following group(s)";
        public string DisablevoicechatPermFail { get; set; } = "You don't have permission to listen to the following group(s)";

        [Description("EnableVoicechat command.")]
        public string EnablevoicechatCommand { get; set; } = EnableVoicechat._command;
        public string EnablevoicechatDescription { get; set; } = EnableVoicechat._description;
        public string[] EnablevoicechatAliases { get; set; } = EnableVoicechat._aliases;
        public string EnableVoicechatSuccess { get; set; } = "Successfully enabled listening to the following group(s)";
        public string EnablevoicechatFail { get; set; } = "You have already enabled listening to the following group(s)";
        public string EnablevoicechatPermFail { get; set; } = "You don't have permission to listen to the following group(s)";

        [Description("Checkwaveinfo command.")]
        public string CheckwaveinfoCommand { get; set; } = CheckWaveInfo._command;
        public string CheckwaveinfoDescription { get; set; } = CheckWaveInfo._description;
        public string[] CheckwaveinfoAliases { get; set; } = CheckWaveInfo._aliases;
        public string CheckwaveinfoSuccess { get; set; } = "<color=blue>Mobile Task Force:</color>\n- Timer: %timermtf%\n- Tokens: %tokensmtf%\n<color=green>Chaos Insurgency:</color>\n- Timer: %timerci%\n- Tokens: %tokensci%";

        [Description("GhostMe command.")]
        public string GhostmeCommand { get; set; } = GhostMe._command;
        public string GhostmeDescription { get; set; } = GhostMe._description;
        public string[] GhostmeAliases { get; set; } = GhostMe._aliases;
        public string GhostmeGhostSuccess { get; set; } = "You have changed yourself to a Ghost.";
        public string GhostmeSpecSuccess { get; set; } = "You have changed yourself to Spectator.";
        public string GhostmeFail { get; set; } = "You must be a Ghost or Spectator to use this command.";

        [Description("GhostSettings command.")]
        public string GhostsettingsCommand { get; set; } = GhostSettings._command;
        public string GhostsettingsDescription { get; set; } = GhostSettings._description;
        public string[] GhostsettingsAliases { get; set; } = GhostSettings._aliases;
        public string GhostsettingsActivated { get; set; } = "You have activated your Ghost Settings.";
        public string GhostsettingsDeactivated { get; set; } = "You have deactivated your Ghost Settings.";

        [Description("GiveFirearm command.")]
        public string GivefirearmCommand { get; set; } = GiveFirearm._command;
        public string GivefirearmDescription { get; set; } = GiveFirearm._description;
        public string[] GivefirearmAliases { get; set; } = GiveFirearm._aliases;
        public string GivefirearmList { get; set; } = "List of firearms";
        public string GivefirearmSuccess { get; set; } = "You have given yourself a <color=green>%itemtype%</color>. Drop it to get rid of it.";
        public string GivefirearmFailure { get; set; } = "You have already reached firearm's limit (%categorylimit%) in your inventory.";

        [Description("ToyAreaTeleport command.")]
        public string ToyareateleportCommand { get; set; } = ToyAreaTeleport._command;
        public string ToyareateleportDescription { get; set; } = ToyAreaTeleport._description;
        public string[] ToyareateleportAliases { get; set; } = ToyAreaTeleport._aliases;
        public string ToyareateleportList { get; set; } = "List of areas, where you can create toys";
        public string ToyareateleportSuccess { get; set; } = "You have been teleported to <color=green>%areaname%</color>.";

        [Description("Translation of the command interface.")]
        public string Aliases { get; set; } = "Aliases";
        public string Description { get; set; } = "Description";
        public string Subcommands { get; set; } = "Available subcommands";
        public string Usage { get; set; } = "Usage";

        [Description("Translation of other command responses.")]
        public string ActiveDuelOther { get; set; } = "Player %playernick% already has an active duel.";
        public string ActiveDuelSelf { get; set; } = "You already have an active duel with %playernick%.";
        public string ActivePendingDuelOther { get; set; } = "Player %playernick% already has a pending duel.";
        public string ActivePendingDuelSelf { get; set; } = "You already have a pending duel with %playernick%.";
        public string DeathPositionDisabled { get; set; } = "Spawning at death position is currently disabled.";
        public string FirearmOnly { get; set; } = "You can only give yourself a firearm.";
        public string MustBeNumber { get; set; } = "Argument must be a number.";
        public string MustBeNumberOrType { get; set; } = "Argument must be a number or item type.";
        public string NoArea { get; set; } = "Provided area %areaname% doesn't exists.";
        public string NoDuelRequests { get; set; } = "You don't have any pending duel requests.";
        public string NoPermission { get; set; } = "You don't have permission to use this feature.";
        public string NoPlayers { get; set; } = "Provided player(s) doesn't exist.";
        public string NoToys { get; set; } = "You don't have any created toys.";
        public string NoToysAllowed { get; set; } = "You have reached the limit of created toys.";
        public string NotGhost { get; set; } = "You can only use this feature, if you are a Ghost.";
        public string NotGhostNorSpectator { get; set; } = "You can only use this feature, if you are a Ghost or Spectator.";
        public string NotGrounded { get; set; } = "You must stand on the ground to create a toy.";
        public string PluginNotEnabled { get; set; } = "GhostSpectator is not enabled.";
        public string RequestAlreadySent { get; set; } = "You have already sent a duel request to this player.";
        public string RoundNotStarted { get; set; } = "You can't use this feature before round start.";
        public string SenderNull { get; set; } = "Command sender is null.";
        public string SssNotEnabled { get; set; } = "Server-specific settings for GhostSpectator are not enabled.";
        public string WarheadDetonated { get; set; } = "You can't use this feature after warhead detonation.";
        public string WrongArea { get; set; } = "You can create toys only in designated area(s).";
        public string WrongArgument { get; set; } = "Provided argument(s) doesn't exist.";

        [Description("SERVER-SPECIFIC SETTINGS\' TRANSLATION. Don't translate words put between two '%'." +
                     "Translation of buttons.")]
        public string Disabled { get; set; } = "DISABLED";
        public string Enabled { get; set; } = "ENABLED";
        public string Hold { get; set; } = "HOLD";
        public string Press { get; set; } = "PRESS";

        [Description("Translation of headers.")]
        public string CurrentPage { get; set; } = "Current Page";
        public string Firearms { get; set; } = "Firearms";
        public string[] PageHeaders { get; set; } = SSGhostSpectator._headers;
        public string ToyManage { get; set; } = "Managing toys";
        public string ToyAreas { get; set; } = "Toy areas";

        [Description("Translation of hints.")]
        public string FullPartNickname { get; set; } = "<size=25><color=yellow>You can provide full or part of the player's nickname.</color></size>";
        public string PermissionNeeded { get; set; } = "<size=25><color=yellow>This option will only work if you have permission.</color></size>";

        [Description("Translation of labels.")]
        public string ActionExecute { get; set; } = "Execute selected action";
        public string ActionSelect { get; set; } = "Select an action";
        public string AreaSelect { get; set; } = "Select toy area";
        public string AreaTeleport { get; set; } = "Teleport to the selected area";
        public string FirearmGive { get; set; } = "Give yourself a firearm of selected type";
        public string FirearmSelect { get; set; } = "Select firearm";
        public string GhostName { get; set; } = "Type name of the Ghost";
        public string Ghosts { get; set; } = "Ghost";
        public string Spectators { get; set; } = "Spectator";
        public string Scps { get; set; } = "SCP";
        public string ToyCreate { get; set; } = "Create a toy of selected type";
        public string ToyDestroy { get; set; } = "Destroy your toy";
        public string ToyList { get; set; } = "Print a list of all your toys";
        public string ToyNetid { get; set; } = "Type a NetID of your toy";
        public string ToySelect { get; set; } = "Select toy type";
        public string WaveInfo { get; set; } = "Check timers and tokens";

        [Description("Translation of options.")]
        public string[] DuelActions { get; set; } = SSGhostSpectator._duelActionOptions;

        internal static Translation AccessTranslation()
        {
            return translation ??= File.Exists(filePath) ? YamlParser.Deserializer.Deserialize<Translation>(File.ReadAllText(filePath)) : new();
        }

        private static Translation translation;

        private static readonly string filePath = Path.Combine(PathManager.Configs.FullName, "GhostSpectator", "translation.yml");
    }
}
