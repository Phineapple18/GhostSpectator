using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using PlayerRoles;
using UnityEngine;

namespace GhostSpectator
{
    public class Config
    {
        [Description("Should debug be enabled?")]
        public bool Debug { get; set; } = false;

        [Description("Color of Ghost custom info. Must be in hex value.")]
        public string GhostColor { get; set; } = "#A0A0A0";

        [Description("Ghost health.")]
        public float GhostHealth { get; set; } = 150f;

        [Description("Duration of Ghost spawn message.")]
        public ushort SpawnmessageDuration { get; set; } = 5;

        [Description("List of Ghost spawn positions.")]
        public List<Vector3> SpawnPositions { get; set; } = new() { new(9f, 302f, 1f) };

        [Description("Should Ghosts be spawned at their death location?")]
        public bool SpawnAtDeathPosition { get; set; } = false;

        [Description("Should players be automatically spawned as Ghosts upon death?")]
        public bool AutoGhostSpawn { get; set; } = false;

        [Description("Should Ghosts, that don't have permission, be despawned and not allowed to spawn after warhead detonation?")]
        public bool DespawnOnDetonation { get; set; } = true;

        [Description("Minimum distance between the Ghosts, that will make them hear eachother via RoundSummary channel.")]
        public float HearDistance { get; set; } = 10f;

        [Description("Roles, that Ghosts cannot teleport to. SCP-079 is already included.")]
        public List<RoleTypeId> RoleTeleportBlacklist { get; set; } = new() { RoleTypeId.Tutorial };

        [Description("Time after which the duel request will expire.")]
        public float DuelRequestTime { get; set; } = 10f;

        [Description("Items that deathmatch Ghosts will receive.")]
        public List<ItemType> DeathmatchItems { get; set; } = new()
        {
            ItemType.GunAK
        };

        [Description("Fog effect applied to Ghosts in deathmatch. Set to 0 to leave default (decontamination).")]
        public byte FogType { get; set; } = 0;

        [Description("Should Spectators be able to see Ghosts, if the spectated player is not a Ghost?")]
        public bool AlwaysSeeGhosts { get; set; } = false;

        [Description("Should Filmmakers be able to see Ghosts?")]
        public bool FilmmakerSeeGhosts { get; set; } = false;

        [Description("Limit of toys one Ghost can have at once.")]
        public int ToyLimit { get; set; } = 1;

        [Description("Areas where Ghosts can create toys. The area exists between two opposite coordinates.")]
        public List<ToyArea> ToySpawnAreas { get; set; } = new()
        {
            new ToyArea
            {
                Name = "Area1",
                Corner1 = new(10f, 294f, -12f),
                Corner2 = new(-10f, 296f, -3f),
                TeleportPosition = new(0f, 295f, -8f)
            }
        };

        [Description("Should server-specific settings for this plugin be enabled?")]
        public bool SsSettingsEnabled { get; set; } = false;

        [Description("Should server-specific settings be automatically activated for Ghosts upon spawn?")]
        public bool SendSettingsOnSpawn { get; set; } = false;
    }

    public class ToyArea
    {
        public Bounds Bounds;

        public string Name { get; set; }
        public Vector3 Corner1 { get; set; }
        public Vector3 Corner2 { get; set; }
        public Vector3 TeleportPosition { get; set; }
    }
}
