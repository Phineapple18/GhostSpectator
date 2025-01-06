using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.IO;

using GhostSpectator.Commands.ClientConsole;
using GhostSpectator.Commands.ClientConsole.Duel;
using GhostSpectator.Commands.ClientConsole.ShootingTarget;
using GhostSpectator.Commands.ClientConsole.Voicechat;
using GhostSpectator.Commands.RemoteAdmin;
using PluginAPI.Helpers;
using Serialization;

namespace GhostSpectator
{
    public class Translation
    {
        [Description("MISCELLANOUS TRANSLATION. Don't translate words put between two '%'." +
                     "\n# Nickname of Ghost.")]
        public string GhostNickname { get; set; } = "GHOST";

        [Description("Broadcast sent to a Ghost upon spawn.")]
        public string SpawnMessage { get; set; } = "<size=50><color=%colour%>You are a Ghost!</color>\n<size=30>Drop the Lantern to teleport to a random alive player (lit) or a random room (unlit).</size>";

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

        public string SpawnSuccess { get; set; } = "Succesfully spawned %count% Ghosts.";

        public string SpawnFail { get; set; } = "Command failed for %count% existing player(s) (already a Ghost)";

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

        [Description("Challenge command.")]
        public string ChallengeCommand { get; set; } = Challenge._command;

        public string ChallengeDescription { get; set; } = Challenge._description;

        public string[] ChallengeAliases { get; set; } = Challenge._aliases;

        public string ChallengeSuccess { get; set; } = "You have challenged %playernick% to duel.";

        [Description("List command.")]
        public string ListduelCommand { get; set; } = ListDuel._command;

        public string ListduelDescription { get; set; } = ListDuel._description;

        public string[] ListduelAliases { get; set; } = ListDuel._aliases;

        public string ListduelSuccess { get; set; } = "You have been challenged to a duel by following player(s)";

        [Description("Reject command.")]
        public string RejectCommand { get; set; } = Reject._command;

        public string RejectDescription { get; set; } = Reject._description;

        public string[] RejectAliases { get; set; } = Reject._aliases;

        public string RejectSuccessPlayer { get; set; } = "You have rejected duel request from %playernick%.";

        [Description("Translation of other client commands. Make sure not to duplicate commands or aliases." +
                     "\n# CreateTarget command.")]

        public string CreatetargetCommand { get; set; } = CreateTarget._command;

        public string CreatetargetDescription { get; set; } = CreateTarget._description;

        public string[] CreatetargetAliases { get; set; } = CreateTarget._aliases;

        public string CreatetargetSuccess { get; set; } = "You have created a shooting target (%targetname%) with ID %targetid%.";

        [Description("DestroyTarget command.")]
        public string DestroyTargetCommand { get; set; } = DestroyTarget._command;

        public string DestroyTargetDescription { get; set; } = DestroyTarget._description;

        public string[] DestroyTargetAliases { get; set; } = DestroyTarget._aliases;

        public string DestroyTargetList { get; set; } = "List of spawned shooting targets (%count%)";

        public string DestroyTargetSuccess { get; set; } = "You have destroyed your shooting target (%targetname%) with ID %targetid%.";

        public string DestroyTargetFail { get; set; } = "You don't have any shooting target created with ID %targetid%.";

        [Description("DisableVoicechat command.")]
        public string DisablevoicechatCommand { get; set; } = DisableVoicechat._command;

        public string DisablevoicechatDescription { get; set; } = DisableVoicechat._description;

        public string[] DisablevoicechatAliases { get; set; } = DisableVoicechat._aliases;

        public string DisablevoicechatSuccess { get; set; } = "Successfully disabled listening to the following group(s)";

        public string DisablevoicechatFail { get; set; } = "You either don't have permission or have already disabled listening to the following group(s)";

        [Description("EnableVoicechat command.")]
        public string EnablevoicechatCommand { get; set; } = EnableVoicechat._command;

        public string EnablevoicechatDescription { get; set; } = EnableVoicechat._description;

        public string[] EnablevoicechatAliases { get; set; } = EnableVoicechat._aliases;

        public string EnableVoicechatSuccess { get; set; } = "Successfully enabled listening to the following groups";

        public string EnablevoicechatFail { get; set; } = "You either don't have permission or have already enabled listening to the following group(s)";

        [Description("GhostMe command.")]
        public string GhostmeCommand { get; set; } = GhostMe._command;

        public string GhostmeDescription { get; set; } = GhostMe._description;

        public string[] GhostmeAliases { get; set; } = GhostMe._aliases;

        public string GhostmeGhostSuccess { get; set; } = "You have changed yourself to a Ghost.";

        public string GhostmeSpecSuccess { get; set; } = "You have changed yourself to Spectator.";

        public string GhostmeFail { get; set; } = "You must be a Ghost or Spectator to use this command.";

        [Description("GiveFirearm command.")]
        public string GivefirearmCommand { get; set; } = GiveFirearm._command;

        public string GivefirearmDescription { get; set; } = GiveFirearm._description;

        public string[] GivefirearmAliases { get; set; } = GiveFirearm._aliases;

        public string GivefirearmList { get; set; } = "List of firearms";

        public string GivefirearmSuccess { get; set; } = "You have given yourself a %itemtype%. Drop it to get rid of it.";

        [Description("Translation of the command interface.")]
        public string Aliases { get; set; } = "Aliases";

        public string Description { get; set; } = "Description";

        public string Subcommands { get; set; } = "Available subcommands";

        public string Usage { get; set; } = "Usage";

        [Description("Translation of other command responses.")]

        public string ActiveDuelOther { get; set; } = "Player %playernick% already has an active duel.";

        public string ActiveDuelSelf { get; set; } = "You already have an active duel with %playernick%.";

        public string ActivePendingDuel { get; set; } = "You already have a pending duel with %playernick%.";

        public string FirearmOnly { get; set; } = "You can only give yourself a firearm.";

        public string ItemtypeOnly { get; set; } = "Provided argument is not an ItemType.";

        public string MustBeId { get; set; } = "Argument must be an ID of your shooting target.";

        public string NoDuelRequests { get; set; } = "You don't have any pending duel requests.";

        public string NoGhosts { get; set; } = "No Ghost (that is not you) was found with the provided nickname or part of it.";

        public string NoPerms { get; set; } = "You don't have permission to use this command.";

        public string NoPlayers { get; set; } = "Provided player(s) doesn't exist.";

        public string NoTargets { get; set; } = "You don't have any created shooting targets.";

        public string NoTargetsAllowed { get; set; } = "You have reached the limit of created shooting targets.";

        public string NotEnabled { get; set; } = "GhostSpectator is not enabled.";

        public string NotGhost { get; set; } = "You can only use this command, if you are a Ghost.";

        public string NotGrounded { get; set; } = "You must stand on the ground to create a shooting target.";

        public string RequestAlreadySent { get; set; } = "You have already sent a duel request to this player.";

        public string RoundNotStarted { get; set; } = "You can't use this command before round start.";

        public string SenderNull { get; set; } = "Command sender is null.";

        public string WarheadDetonated { get; set; } = "You can't use this command after warhead detonation.";

        public string WrongArea { get; set; } = "You can create shooting targets only in designated shooting range(s).";

        public string WrongArgument { get; set; } = "Provided argument(s) doesn't exist.";

        public static Translation AccessTranslation()
        {
            string filePath = Path.Combine(File.Exists(globalPlugin) ? globalTranslation : localTranslation, translationFileName);
            return File.Exists(filePath) ? YamlParser.Deserializer.Deserialize<Translation>(File.ReadAllText(filePath)) : new();
        }

        private static readonly string globalPlugin = Path.Combine(Paths.GlobalPlugins.Plugins, $"{pluginName}.dll");

        private static readonly string globalTranslation = Path.Combine(Paths.GlobalPlugins.Plugins, pluginName);

        private static readonly string localTranslation = Path.Combine(Paths.LocalPlugins.Plugins, pluginName);

        internal const string pluginName = "GhostSpectator";

        internal const string translationFileName = "translation.yml";
    }
}
