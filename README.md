# GhostSpectator (3.3.2)
Plugin for the "SCP: Secret Laboratory" game, that allows players to turn into Ghosts: Tutorials undetectable to alive players and not affecting the round. Depending on the config, Ghosts can perform various activities listed below.

## Features
- Ghosts's names have different color, also in RA console. The color is set in config.
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
  * join a deathmatch
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
|ghost_color|string|'#A0A0A0'|Color of Ghost's custom info and names in RA console. Must be in hex value.|
|ghost_health|float|150f|Ghost health.|
|spawnmessage_duration|ushort|5|Duration of Ghost spawn message.|
|spawn_positions|List\<Vector3>|- x: 9, y: 1002, z: 1|List of Ghost spawn positions.|
|spawn_at_death_pos|bool|false|Should Ghosts be spawned at their death location?|
|auto_ghost_spawn|bool|false|Should players be automatically spawned as Ghosts upon death?|
|despawn_on_detonation|bool|true|Should Ghosts, that don't have permission, be despawned and not allowed to spawn after warhead detonation?|
|hear_distance|float|10f|Minimum distance between the Ghosts, that will make them hear eachother via RoundSummary channel.|
|role_teleport_blacklist|List\<RoleTypeId\>|- Tutorial|Roles, that Ghosts cannot teleport to. SCP-079 is already included.|
|duel_request_time|float|10f|Time after which the duel request will expire.|
|deathmatch_items|List<Itemtype>|- GunAK|Items that deathmatch Ghosts will receive.|
|fog_type|byte|0|Fog effect applied to Ghosts in deathmatch. Set to 0 to leave default (decontamination).|
|always_see_ghosts|bool|false|Should Spectators be able to see Ghosts, if the spectated player is not a Ghost?|
|filmmaker_see_ghosts|bool|false|Should Filmmakers be able to see Ghosts?|
|toy_limit|int|1|Limit of toys one Ghost can have at once.|
|toy_spawn_areas|Dictionary\<Vector3, Vector3>|- name: Area1<br/>&nbsp;&nbsp;&nbsp;corner1:</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;x: 10</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;y: 294</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;z: -12<br/>&nbsp;&nbsp;&nbsp;corner2:</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;x: -10</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;y: 296</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;z: -3<br/>&nbsp;&nbsp;&nbsp;teleport_position:</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;x: 0</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;y: 295</br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;z: -8|Areas where Ghosts can create toys. The area exists between two opposite coordinates.|
|ss_settings_enabled|bool|false|Should server-specific settings for this plugin be enabled?|
|send_settings_on_spawn|bool|false|Should server-specific settings be automatically activated for Ghosts upon spawn?|

## Translation
The translation file is in the same folder as the config file and allows you to customize information shown to ghosts, such as hints and command names, aliases, descriptons and their responses.

*IMPORTANT:* If you translate command names and/or aliases (except subcommands), make sure not to duplicate them.

## Remote Admin Commands
1. *ghostspectator* - allows managing the Ghosts. Subcommands: despawn, list, spawn
2. *ghostsettings* - allows managing server-specific settings. Subcommands: disable, enable, reload

## Client Console Commands
1. *duel* - allows managing the duels. Subcommands: accept, cancel, challenge, list, reject
2. *toy* - allows managing the toys. Subcommands: create, destroy, list
3. *voicechat* - allows managing the voicechats. Subcommands: disable, enable, list

### Miscellanous commands
1. *deathmatch* - joining and leaving a deathmatch
2. *ghostme* - self ghost spawning and despawning
3. *ghostsettings* - activating and deactivating servr-specific settings for self
4. *givefirearm* - giving or printing list of firearms
5. *toyareateleport* - teleporting to area, where ghosts can spawn toys
5. *waveinfo* - checking wave timers and tokens

## Permissions
- gs.deathmatch - allows a player to use *deathmatch* command
- gs.duel - allows a player to use *challenge* and *accept* commands
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
