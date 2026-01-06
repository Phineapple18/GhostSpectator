# GhostSpectator (3.3.1)
Plugin for the "SCP: Secret Laboratory" game, that allows players to turn into Ghosts: Tutorials undetectable to alive players and not affecting the round. Depending on the config, Ghosts can perform various activities f.e teleport to alive players or random rooms, spawn a toy or challenge another Ghost to a duel.

## Features
- Ghosts can pass through most doors.
- Ghosts can see in dark.
- Ghosts can teleport to a random alive player or room by dropping a ghost item (Lantern). Dropping lit lantern teleports to a player, while unlit to a room. There is an option to exclude teleportation to certain roles.
- Ghosts are always visible to each other, Spectators spactating a Ghost and Overwatchers. Depending on the config, Ghosts can be visible to Spectators spectating a non-Ghost player and Filmmakers.
- Ghosts can't pick up or use items.
- Ghosts can't interact with objects (except resetting their shooting targets).
- Depending on the assigned permissions, Ghosts can:
  * noclip
  * drop or throw items or throwables (except their ghost item)
  * teleport to a toy spawn area
  * create toys (capybara and shooting targets), that are visible only to Ghosts
  * give themselves a firearm (when emptied, player will receive ammo)
  * listen to SCP and Spectators chats (via command or automatically)
  * listen to other Ghosts via RoundSummary chat, if they are not within certain distance (via command or automatically)
  * challenge other Ghosts to a duel
  * be spawned at the place of their death
  * be autospawned after player's death

## Required plugins and dependencies:
- [Harmony 2.2.2.0 (net48)](https://github.com/pardeike/Harmony/releases/tag/v2.2.2.0) by pardeike - dependency

## Installation
Place *GhostSpectator* dll in "...\AppData\Roaming\SCP Secret Laboratory\LabAPI-Beta\plugins\global OR port_number".

Place *Harmony* dll (net48) in "...\AppData\Roaming\SCP Secret Laboratory\LabAPI-Beta\plugins\global OR port_number\dependencies".

## Config
|Name|Type|Default value|Description|
|---|---|---|---|
|debug|bool|false|Should debug be enabled?|
|ghost_color|string|'#A0A0A0'|Ghost nickname color.|
|ghost_health|float|150f|Ghost health.|
|spawnmessage_duration|ushort|5|Spawn message duration.|
|spawn_positions|List\<Vector3>|- x: 9, y: 1002, z: 1|Ghost spawn positions.|
|spawn_at_death_pos|bool|false|Should be Ghosts spawned at the death location?|
|auto_ghost_spawn|bool|false|Should players be automatically spawned as Ghosts upon death?|
|role_teleport_blacklist|List\<RoleTypeId\>|- Tutorial|Roles, that Ghosts cannot teleport to. SCP-079 is already included.|
|despawn_on_detonation|bool|true|Should Ghosts, that don't have permission, be despawned and not allowed to spawn after warhead detonation?|
|always_see_ghosts|bool|false|Should Spectators be able to see Ghosts, if the spectated player is not a Ghost?|
|filmmaker_see_ghosts|bool|false|Should Filmmakers be able to see Ghosts?|
|target_limit|int|1|How many toys can one Ghost have at once?|
|shooting_ranges|Dictionary\<Vector3, Vector3>|- name: Area1<br/>&nbsp;&nbsp;&nbsp;corner1:</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;x: 10</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;y: 294</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;z: -12<br/>&nbsp;&nbsp;&nbsp;corner2:</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;x: -10</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;y: 296</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;z: -3<br/>&nbsp;&nbsp;&nbsp;teleport_position:</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;x: 0</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;y: 295</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;z: -8|Areas where Ghosts can create toys. The area exists between a pair of coordinates on each axis.|
|hear_distance|float|10f|Minimum distance between the Ghosts, that will make them hear eachother via RoundSummary channel.|
|duel_request_time|float|10f|Time after which the duel request will expire.|
|ss_settings_enabled|bool|false|Should server-specific settings for this plugin be enabled?|
|send_settings_on_spawn|bool|false|Should server-specific settings be automatically activated for Ghosts upon spawn?|

## Translation
The translation file is in the same folder as the config file and allows you to customize e.g:
- Ghost nickname
- hints displayed to Ghosts
- command name, aliases, descripton and responses

*IMPORTANT:* If you translate command names and/or aliases (except subcommands), make sure not to duplicate them.

## Remote Admin Commands
### ghostspectator
Parent command for Ghosts managing. Subcommands:
- despawn - Despawn chosen Ghost(s) to Spectator. Separate entries with space. Usage: PlayerID/all
- list - Print a list of all Ghosts.
- spawn - Spawn chosen player(s) as Ghost. Separate entries with space. Usage: PlayerID/all

### ghostsettings
Parent command for managing server-specific settings for GhostSpectator. Subcommands:
- disable - Disable server-specific settings for GhostSpectator.
- enable - Enable server-specific settings for GhostSpectator.
- reload - Reload server-specific settings for GhostSpectator.

## Client Console Commands
### duel
Parent command for Ghost duelling. Subcommands:
- accept - Accept a duel offer from other Ghost. Usage: PlayerNickname (whole or part, case-insensitive)
- cancel - Cancel your duel, pending duel or duel request.
- challenge - Challenge another Ghost to a duel. Usage: PlayerNickname (whole or part, case-insensitive)
- list - Print a list of all players who you challenged and who challenged you to a duel.
- reject - Reject a duel offer from other Ghost. Usage: PlayerNickname (whole or part, case-insensitive)

### toy
Parent command for toy managing. Subcommands:
- create - Create a toy. Usage: Capybara/TargetDBoy/TargetSport/TargetBinary
- destroy - Destroy your toy. Usage: NetId/list
- list - Print a list of all the toys you created.

### Miscellanous commands
- disablevoicechat - Disable listening to chosen voicechat(s). Usage: ghost/scp/spectator/all
- enablevoicechat - Enable listening to chosen voicechat(s). Usage: ghost/scp/spectator/all
- ghostme - Spawn yourself as a Ghost or change back to Spectator.
- givefirearm - Give yourself a firearm or print a list of available firearms. Usage: Item ID/Itemtype/list
- toyareateleport - Teleport to an area, where you can spawn toys. Usage: AreaName/list

## Permissions
- gs.duel - allows a player to use *player* and *accept* commands
- gs.firearm - allows a player to use *givefirearm* command
- gs.item - allows a player to drop and throw items (expect ghost item)
- gs.list - allows a player to use *list* (RA) command
- gs.listen.ghost - allows a player to lsiten to other Ghosts via RoundSummary chat, if they are not within certain distance (via command)
- gs.autolisten.ghost - allows the above automatically, when spawned as a Ghost
- gs.listen.scp - allows a player to listen to SCPs (via command)
- gs.autolisten.scp - allows the above automatically, when spawned as a Ghost
- gs.listen.spectator - allows a player to listen Spectators (via command)
- gs.autolisten.spectator - allows the above automatically, when spawned as a Ghost
- gs.noclip - allows a player to have noclip permitted
- gs.settings.activate - allows a player to activate and deactivate server-specific settings for this plugin
- gs.settings.reload - allows a player to enable, disable and reload server-specific settings for this plugin
- gs.spawn.all - allows a player to use *spawn* and *despawn* commands
- gs.spawn.self - allows a player to use *ghostme* command
- gs.toy.area - allows a player to teleport to a toy spawn area
- gs.toy.create - allows a player to use *createtoy* command
- gs.teleport.player - allows a player to teleport to an alive player
- gs.teleport.room - allows a player to teleport to a random room
- gs.warhead - allows a player to remain Ghost and use *ghostme* and *spawn* commands after warhead detonation
- gs.waveinfo - allows a player to check waves timer and tokens

## Credits
- Original plugin creator: [Thundermaker300](https://github.com/Thundermaker300)
