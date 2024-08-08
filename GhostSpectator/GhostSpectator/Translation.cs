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
using GhostSpectator.Commands.RemoteAdminConsole;
using PluginAPI.Helpers;
using Serialization;

namespace GhostSpectator
{
    public class Translation
    {
        [Description("MISCELLANOUS TRANSLATION. Don't translate words between two '%'." +
                     "\n# Nickname of Ghost.")]
        public string GhostNickname { get; set; } = "GHOST";

        [Description("Broadcast sent to Ghost upon spawn.")]
        public string SpawnMessage { get; set; } = "<size=50><color=%colour%>You are a Ghost!</color>\n<size=30>Drop the %teleportitem% to teleport to a random alive player.</size>";

        [Description("Hints shown to Ghost, when dropping teleport item.")]
        public string TeleportSuccess { get; set; } = "You have been teleported to <color=green>%playernick%</color>.";

        public string TeleportFail { get; set; } = "There is nobody you can teleport to.";

        [Description("Duel related hints shown to Ghost.")]
        public string DuelAbandoned { get; set; } = "Your duel has been abandoned by your opponent (<color=red>%playernick%</color>).";

        public string DuelAborted { get; set; } = "Your duel with <color=red>%playernick%</color> has been aborted.";

        public string DuelPrepare { get; set; } = "Prepare for duel!";

        public string DuelStarted { get; set; } = "Duel has started!";

        public string DuelWon { get; set; } = "You have won duel against <color=green>%playernick%</color>.";

        public string DuelLost { get; set; } = "You have lost duel against <color=red>%playernick%</color>.";

        public string DuelRequestCancelled { get; set; } = "Player <color=red>%playernick%</color> has cancelled their duel request.";

        public string DuelRequestExpired { get; set; } = "Your duel request for <color=red>%playernick%</color> has expired.";

        public string DuelRequestReceived { get; set; } = "Player <color=red>%playernick%</color> has challenged you to duel!\nUse the client command console to accept or reject it within <color=yellow>%time%s</color> or let the offer expire.";

        public string DuelRequestRejected { get; set; } = "Player <color=red>%playernick%</color> has rejected your duel request.";

        [Description("COMMANDS\' TRANSLATION. Don't translate words between two '%'." +
                     "\n# Should debug be enabled for command registering?")]
        public bool Debug { get; set; } = false;

        [Description("Translation for GhostSpectator parent command and its subcommands. Make sure not to duplicate commands or aliases." +
                     "\n# GhostSpectator parent command.")]
        public string GhostspectatorParentCommand { get; set; } = GhostSpectatorParent._command;

        public string GhostspectatorParentDescription { get; set; } = GhostSpectatorParent._description;

        public string[] GhostspectatorParentAliases { get; set; } = GhostSpectatorParent._aliases;

        [Description("Despawn command.")]
        public string DespawnCommand { get; set; } = Despawn._command;

        public string DespawnDescription { get; set; } = Despawn._description;

        public string[] DespawnAliases { get; set; } = Despawn._aliases;

        public string DespawnSuccess { get; set; } = "Succesfully despawned %count% existing player(s) from Ghosts";

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

        public string SpawnSuccess { get; set; } = "Succesfully turned %count% existing player(s) into Ghosts";

        public string SpawnFail { get; set; } = "Command failed for %count% existing player(s) (already a Ghost)";

        [Description("Translation for Duel parent command and its subcommands. Make sure not to duplicate commands or aliases." +
                     "\n# Duel parent command.")]
        public string DuelParentCommand { get; set; } = DuelParent._command;

        public string DuelParentDescription { get; set; } = DuelParent._description;

        public string[] DuelParentAliases { get; set; } = DuelParent._aliases;

        [Description("Accept command.")]
        public string AcceptCommand { get; set; } = Accept._command;

        public string AcceptDescription { get; set; } = Accept._description;

        public string[] AcceptAliases { get; set; } = Accept._aliases;

        public string AcceptSuccess { get; set; } = "You have accepted duel request from %playernick%.";

        [Description("Cancel command.")]
        public string CancelCommand { get; set; } = Cancel._command;

        public string CancelDescription { get; set; } = Cancel._description;

        public string[] CancelAliases { get; set; } = Cancel._aliases;

        public string CancelDuelSuccess { get; set; } = "You have cancelled your duel with %playernick%.";

        public string CancelRequestSuccess { get; set; } = "You have cancelled your duel request for %playernick%.";

        public string CancelFail { get; set; } = "You don't have any active duel nor duel requests.";

        [Description("List command.")]
        public string ListduelCommand { get; set; } = ListDuel._command;

        public string ListduelDescription { get; set; } = ListDuel._description;

        public string[] ListduelAliases { get; set; } = ListDuel._aliases;

        public string ListduelSuccess { get; set; } = "You have been challenged to duel by following players";

        [Description("Player command.")]
        public string PlayerCommand { get; set; } = Ply._command;

        public string PlayerDescription { get; set; } = Ply._description;

        public string[] PlayerAliases { get; set; } = Ply._aliases;

        public string PlayerSuccess { get; set; } = "You have challenged %playernick% to duel.";

        [Description("Reject command.")]
        public string RejectCommand { get; set; } = Reject._command;

        public string RejectDescription { get; set; } = Reject._description;

        public string[] RejectAliases { get; set; } = Reject._aliases;

        public string RejectSuccessAll { get; set; } = "You have rejected all duel requests.";

        public string RejectSuccessPlayer { get; set; } = "You have rejected duel request from %playernick%.";

        [Description("Translation for client commands. Make sure not to duplicate commands or aliases." +
                     "\n# CreateTarget command.")]

        public string CreatetargetCommand { get; set; } = CreateTarget._command;

        public string CreatetargetDescription { get; set; } = CreateTarget._description;

        public string[] CreatetargetAliases { get; set; } = CreateTarget._aliases;

        public string CreatetargetSuccess { get; set; } = "You have spawned a shooting target (%targetname%) with ID %targetid%.";

        [Description("DestroyTarget command.")]
        public string DestroyTargetCommand { get; set; } = DestroyTarget._command;

        public string DestroyTargetDescription { get; set; } = DestroyTarget._description;

        public string[] DestroyTargetAliases { get; set; } = DestroyTarget._aliases;

        public string DestroyTargetList { get; set; } = "List of spawned targets (%count%)";

        public string DestroyTargetSuccess { get; set; } = "You have despawned your shooting target (%targetname%) with ID %targetid%.";

        public string DestroyTargetFail { get; set; } = "You don't have any spawned shooting target with ID %targetid%.";

        [Description("DisableVoicechat command.")]
        public string DisablevoicechatCommand { get; set; } = DisableVoicechat._command;

        public string DisablevoicechatDescription { get; set; } = DisableVoicechat._description;

        public string[] DisablevoicechatAliases { get; set; } = DisableVoicechat._aliases;

        public string DisablevoicechatSuccess { get; set; } = "Successfully disabled listening to following groups";

        public string DisablevoicechatFail { get; set; } = "You already have disabled listening to following group(s)";

        public string DisablevoicechatFailNoperm { get; set; } = "You don't have permission to disable listening to following group(s)";

        [Description("EnableVoicechat command.")]
        public string EnablevoicechatCommand { get; set; } = EnableVoicechat._command;

        public string EnablevoicechatDescription { get; set; } = EnableVoicechat._description;

        public string[] EnablevoicechatAliases { get; set; } = EnableVoicechat._aliases;

        public string EnableVoicechatSuccess { get; set; } = "Successfully enabled listening to following groups";

        public string EnablevoicechatFail { get; set; } = "You already have enabled listening to following group(s)";

        public string EnablevoicechatFailNoperm { get; set; } = "You don't have permission to listen to following group(s)";

        [Description("GhostMe command.")]
        public string GhostmeCommand { get; set; } = GhostMe._command;

        public string GhostmeDescription { get; set; } = GhostMe._description;

        public string[] GhostmeAliases { get; set; } = GhostMe._aliases;

        public string GhostmeGhostSuccess { get; set; } = "You have changed yourself to Ghost.";

        public string GhostmeSpecSuccess { get; set; } = "You have changed yourself to Spectator.";

        public string GhostmeFail { get; set; } = "You must be a Ghost or Spectator to use this command.";

        [Description("GiveFirearm command.")]
        public string GivefirearmCommand { get; set; } = GiveFirearm._command;

        public string GivefirearmDescription { get; set; } = GiveFirearm._description;

        public string[] GivefirearmAliases { get; set; } = GiveFirearm._aliases;

        public string GivefirearmList { get; set; } = "List of firearms";

        public string GivefirearmSuccess { get; set; } = "You have given yourself a %itemtype%. Drop it to get rid of it.";

        [Description("Translation for command interface.")]
        public string Aliases { get; set; } = "Aliases";

        public string Description { get; set; } = "Description";

        public string Subcommands { get; set; } = "Available subcommands";

        public string Usage { get; set; } = "Usage";

        [Description("Translation for other command responses.")]

        public string ActiveDuelOther { get; set; } = "Player %playernick% already has active duel.";

        public string ActiveDuelSelf { get; set; } = "You already have active duel with %playernick%.";

        public string ActivePendingDuel { get; set; } = "You already have pending duel with %playernick%.";

        public string DedicatedServer { get; set; } = "You can't use that command on Dedicated Server.";

        public string FirearmOnly { get; set; } = "You can only give yourself a firearm.";

        public string ItemtypeOnly { get; set; } = "Provided argument is not an ItemType.";

        public string MustBeId { get; set; } = "Argument must be an ID of your spawned shooting target.";

        public string NoDuelRequests { get; set; } = "You don't have any pending duel requests.";

        public string NoGhosts { get; set; } = "No Ghost (that is not you) was found with the provided nickname.";

        public string NoPerms { get; set; } = "You don't have permission to use this command.";

        public string NoPlayers { get; set; } = "Provided player(s) doesn't exist.";

        public string NoTargets { get; set; } = "You don't have any spawned shooting targets.";

        public string NoTargetsAllowed { get; set; } = "You have reached the maximum allowed number of spawned shooting targets.";

        public string NotEnabled { get; set; } = "GhostSpectator is not enabled.";

        public string NotGhost { get; set; } = "You can only use this command, if you are a Ghost.";

        public string NotGrounded { get; set; } = "You must stand on the ground to spawn shooting target.";

        public string RequestAlreadySent { get; set; } = "You have already sent duel request to this player.";

        public string RoundNotStarted { get; set; } = "You can't use that command before round start.";

        public string SenderNull { get; set; } = "Command sender is null.";

        public string WarheadDetonated { get; set; } = "You can't use this command after warhead detonation.";

        public string WrongArea { get; set; } = "You can spawn shooting targets only in designated shooting range(s).";

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
