# GhostSpectator (2.0.1)
Plugin for the "SCP: Secret Laboratory" game, that allows players to turn into Ghosts: Tutorials undetectable to alive players and not affecting the round. Depending on the config, Ghosts can perform various activities, such as teleporting to alive players or random rooms, practising target shooting or challenging another Ghost to a duel.

## Features
- Ghosts can pass through most doors.
- Ghosts can teleport to a random alive player or room by dropping a ghost item (Lantern). Dropping lit lantern teleports to a player, while unlit to a room. There is an option to exclude teleportation to certain roles.
- Ghosts are always visible to each other, Spectators spactating a Ghost and Overwatchers. Depending on the config, Ghosts can be visible to Spectators spectating a non-Ghost player and Filmmakers.
- Ghosts can't pick up or use items.
- Ghosts can't interact with objects (except resetting their shooting targets).
- Depending on the assigned permissions, Ghosts can:
  * noclip
  * drop or throw items or throwables (except their ghost item)
  * create a shooting target(s), that are visible only to them
  * give themselves a firearm with infinite ammunition 
  * listen to SCP and Spectators chats (via command or automatically)
  * listen to other Ghosts via RoundSummary chat, if they are not within Proximity Chat or further than certain distance (via command or automatically)
  * challenge other Ghosts to a duel

## Required plugins and dependencies: 
- [NWAPIPermissionSystem](https://github.com/CedModV2/NWAPIPermissionSystem/releases/tag/0.0.6) by ced777ric - plugin
- [Harmony 2.2.2.0 (net48)](https://github.com/pardeike/Harmony/releases/tag/v2.2.2.0) by pardeike - dependency (attached to one of the previous releases)

## Installation
Place *GhostSpectator* and *NWAPIPermissionSystem* dlls in "...\AppData\Roaming\SCP Secret Laboratory\PluginAPI\plugins\global OR port_number".

Place the *Harmony* dll (net48) in "...\AppData\Roaming\SCP Secret Laboratory\PluginAPI\plugins\global OR port_number\dependencies".

## Config
|Name|Type|Default value|Description|
|---|---|---|---|
|is_enabled|bool|true|Should plugin be enabled?|
|debug|bool|false|Should debug be enabled?|
|ghost_color|string|'#A0A0A0'|Ghost nickname color.|
|ghost_health|float|150f|Ghost health.|
|spawnmessage_duration|ushort|5|Spawn message duration.|
|spawn_positions|List\<string>|- 9, 1002, 1|Ghost spawn positions.|
|role_teleport_blacklist|List\<RoleTypeId\>|- Tutorial|Roles, that Ghosts cannot teleport to. SCP-079 is already included.|
|despawn_on_detonation|bool|true|Should Ghosts, that don't have permission, be despawned and not allowed to spawn after warhead detonation?|
|always_see_ghosts|bool|false|Should Spectators be able to see Ghosts, if the spectated player is not a Ghost?|
|filmmaker_see_ghosts|bool|false|Should Filmmakers be able to see Ghosts?|
|target_limit|int|1|How many shooting targets at once can one Ghost have created?|
|shooting_ranges|Dictionary\<string, string>|10, 995, -12: -10, 996, -4<br/> 68, 983, -36: 142, 985, -12|Areas where Ghosts can create shooting targets. The area exists between a pair of coordinates on each axis.|
|hear_distance|float|10f|Minimum distance between the Ghosts, that will make them hear eachother via RoundSummary channel instead of Proximity channel (if they have enabled listening to Ghosts).|
|duel_request_time|float|10f|Time after which the duel request will expire.|

## Translation
The translation file is in the same folder as the config file and allows you to customize e.g:
- Ghost nickname
- hints displayed to Ghosts
- command name, aliases, descripton and responses

*IMPORTANT:* Make sure not to duplicate command names and/or aliases, if you translate them.

## Remote Admin Commands
### ghostspectator
Parent command for managing Ghosts. Subcommands:
- despawn - Despawn chosen Ghost(s) to Spectator. Separate entries with space. Usage: PlayerID/all
- list - Print a list of all Ghosts.
- spawn - Spawn chosen player(s) as Ghost. Separate entries with space. Usage: PlayerID/all

## Client Console Commands
### duel
Parent command for Ghost duelling. Subcommands:
- accept - Accept a duel offer from other Ghost. Usage: PlayerNickname (whole or part, case-insensitive)
- cancel - Cancel your duel, pending duel or duel request.
- challenge - Challenge another Ghost to a duel. Usage: PlayerNickname (whole or part, case-insensitive)
- list (duel) - Print a list of all players who challenged you to a duel.
- reject - Reject a duel offer from other Ghost. Usage: PlayerNickname (whole or part, case-insensitive)

### Miscellanous commands
- createtarget - Create a shooting target. Usage: dboy/sport/binary
- destroytarget - Destroy your shooting target or print a list of your shooting targets. Usage: NetId/list
- disablevoicechat - Disable listening to chosen voicechat(s). Usage: scp/dead/ghost/all
- enablevoicechat - Enable listening to chosen voicechat(s). Usage: scp/dead/ghost/all
- ghostme - Spawn yourself as a Ghost or change back to Spectator.
- givefirearm - Give yourself a firearm or print a list of available firearms. Usage: Itemtype/list

## Permissions
- gs.duel - allows a player to use *player* and *accept* commands
- gs.firearm - allows a player to use *givefirearm* command
- gs.item - allows a player to drop and throw items (expect ghost item)
- gs.list - allows a player to use *list* (RA) command
- gs.listen.dead - allows a player to listen Spectators (via command)
- gs.autolisten.dead - allows the above automatically, when spawned as a Ghost
- gs.listen.ghost - allows a player to lsiten to other Ghosts via RoundSummary chat, if they are not within Proximity chat or further than configurable distance (via command)
- gs.autolisten.ghost - allows the above automatically, when spawned as a Ghost
- gs.listen.scp - allows a player to listen to SCPs (via command)
- gs.autolisten.scp - allows the above automatically, when spawned as a Ghost
- gs.noclip - allows a player to have noclip permitted
- gs.spawn.other - allows a player to use *spawn* and *despawn* commands
- gs.spawn.self - allows a player to use *ghostme* command
- gs.target - allows a player to use *createtarget* command
- gs.warhead - allows a player to remain Ghost and use *ghostme* and *spawn* commands after warhead detonation

## Credits
- Original plugin creator: [Thundermaker300](https://github.com/Thundermaker300)
