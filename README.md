# GhostSpectator
Plugin for SCP-SL game, that allows player to become a Ghost: Tutorial, that is undetectable to alive players and has no influence on a course of the game. Depending on the config, Ghosts can perform various activities, such as teleport to alive players, practice on shooting targets or challenge another Ghost to a duel.

## Features
- Ghosts can pass through most of the doors.
- Ghosts can teleport to a random alive player by dropping a ghost item, which type is declared in the config. If the type is set to None, then player won't receive a ghost item. There is an option to exclude certain roles, that Ghosts won't be able to teleport to.
- Ghosts are always visible to each other, Spectators spactating a Ghost and Overwatchers. Depending on the config, Ghosts can be visible to Spectators spectating a non-Ghost player and Filmmakers.
- Ghosts are 10x smaller than normal players.
- Ghosts can't pick up or use items.
- Ghosts can't interact with objects (except resetting the shooting targets).
- Depending on the assigned permissions, Ghosts can:
  * noclip
  * drop or throw items or throwables, except the ghost item
  * spawn themselves a shooting target
  * give themselves a firearm, which will be automatically refilled when emptied
  * use a Particle Disruptor 
  * listen to SCP and Spectators chats (via command or automatically)
  * listen to other Ghosts via RoundSummary chat, if they are not within Proximity Chat or further than configurable distance (via command or automatically)
  * challenge other Ghosts to a duel
- Ghosts can be resurrected by SCP-049 regardless, if they were human or SCP-0492.

## Required plugins and dependencies (1.3.2): 
- [NWAPIPermissionSystem](https://github.com/CedModV2/NWAPIPermissionSystem/releases/tag/0.0.6) by ced777ric - plugin
- [Harmony 2.2.2.0 (net48)](https://github.com/pardeike/Harmony/releases/tag/v2.2.2.0) by pardeike - dependency (attached to one of the previous releases)

## Config
|Name|Type|Default value|Description|
|---|---|---|---|
|is_enabled|bool|true|Should plugin be enabled?|
|debug|bool|false|Should debug be enabled?|
|ghost_color|string|'#A0A0A0'|Color of Ghost nickname.|
|ghost_health|float|150f|Maximum health of Ghost.|
|spawnmessage_duration|ushort|5|Duration of spawn message.|
|spawn_positions|List\<string>|- 9, 1002, 1|Spawn positions for Ghosts.|
|teleport_item|ItemType|Lantern|Item given to every Ghost, that can teleport them to an alive player when dropped.|
|role_teleport_blacklist|List\<RoleTypeId\>|- Tutorial|List of roles, that Ghosts cannot be teleported to. SCP-079 is already included.|
|despawn_on_detonation|bool|true|Should Ghosts be despawned (and not allowed to spawn) after warhead detonation?|
|always_see_ghosts|bool|false|Should Spectators be able to see Ghosts, if spectated player is not a Ghost?|
|filmmaker_see_ghosts|bool|false|Should Filmmakers be able to see Ghosts?|
|target_limit|int|1|How many shooting targets can one Ghost have spawned?|
|shooting_ranges|Dictionary\<string, string>|10, 995, -12: -10, 996, -4<br/> 68, 983, -36: 142, 985, -12|Areas, where Ghosts can spawn shooting targets. For each area, provide a pair of positions. Their coordinates will be used as a perimeter along every axis.|
|hear_distance|float|10f|Minimum distance between the Ghosts, that will make them hear eachother via RoundSummary channel instead of Proximity channel (if they have enabled listening to Ghosts).|
|duel_request_time|float|10f|Time, which the duel request will expire after.|

## Translation
The translation file is in the same folder as config file and allows you to customize:
- Ghost nickname
- messages shown to Ghosts
- whether or not the debug for command registering should be enabled
- commands, their aliases and descripton
- command responses

## Remote Admin Commands
### ghostspectator
Parent command. Type empty command for more information regarding subcommands. Subcommands:
- despawn - Despawn selected player(s) from Ghost to Spectator. Separate entries with space. Usage: PlayerId/PlayerNickname/all
- list - Print a list of all Ghosts.
- spawn - Spawn selected player(s) as Ghost. Separate entries with space. Usage: PlayerId/PlayerNickname/all

## Client Console Commands
### duel
Parent command. Type empty command for more information regarding subcommands. Subcommands:
- accept - Accept duel offer from a player. Provide player nickname, whole or part of it, otherwise the first offer will be accepted. The case is ignored.
- cancel - Cancel your duel request or current duel.
- list (duel) - Print a list of all players, who challenged you to a duel.
- player - Challenge another Ghost to a duel by typing their nickname, whole or part of it. The case is ignored. 
- reject - Reject duel offer from player(s). Provide player nickname, whole or part of it, otherwise all offers will be rejected. The case is ignored.

### Miscellanous commands
- createtarget - Create a shooting target. Usage: dboy/sport/bin
- destroytarget - Destroy your shooting target or print a list of your targets. Usage: NetId/list
- disablevoicechat - Disable listening to selected voicechat(s). Usage: scp/dead/ghost/all
- enablevoicechat - Enable listening to selected voicechat(s). Usage: scp/dead/ghost/all
- ghostme - Change yourself to Ghost from Spectator or vice versa.
- givefirearm - Give yourself a firearm or print a list of available firearms. Usage: Itemtype/list

## Permissions
- gs.duel - allows player to use *duel* and *accept* commands
- gs.firearm - allows player to use *givefirearm* command
- gs.item - allows player to drop and throw items (expect ghost item) and use Particle Disruptor
- gs.list - allows player to use *list* (RA) command
- gs.listen.dead - allows player to listen Spectators (via command)
- gs.autolisten.dead - allows above automatically, when spawned as Ghost
- gs.listen.ghost - allows player to lsiten to other Ghosts via RoundSummary chat, if they are not within Proximity chat or further than configurable distance (via command)
- gs.autolisten.ghost - allows above automatically, when spawned as Ghost
- gs.listen.scp - allows player to listen to SCPs (via command)
- gs.autolisten.scp - allows above automatically, when spawned as Ghost
- gs.noclip - allows player to have noclip permitted
- gs.spawn.other - allows player to use *spawn* and *despawn* commands
- gs.spawn.self - allows player to use *ghostme* command
- gs.target - allows player to use *createtarget* command
- gs.warhead - allows player to remain Ghost and use *ghostme* and *spawn* commands after warhead detonation

## Credits
- Original plugin creator: [Thundermaker300](https://github.com/Thundermaker300)
