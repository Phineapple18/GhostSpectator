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

        [Description("Ghost nickname color.")]
        public string GhostColor { get; set; } = "#A0A0A0";

        [Description("Ghost health.")]
        public float GhostHealth { get; set; } = 150f;

        [Description("Spawn message duration.")]
        public ushort SpawnmessageDuration { get; set; } = 5;

        [Description("Ghost spawn positions.")]
        public List<Vector3> SpawnPositions { get; set; } = new List<Vector3>{ new(9f, 302f, 1f) };

        [Description("Roles, that Ghosts cannot teleport to. SCP-079 is already included.")]
        public List<RoleTypeId> RoleTeleportBlacklist { get; set; } = new List<RoleTypeId> { RoleTypeId.Tutorial };

        [Description("Should Ghosts, that don't have permission, be despawned and not allowed to spawn after warhead detonation?")]
        public bool DespawnOnDetonation { get; set; } = true;

        [Description("Should Spectators be able to see Ghosts, if the spectated player is not a Ghost?")]
        public bool AlwaysSeeGhosts { get; set; } = false;

        [Description("Should Filmmakers be able to see Ghosts?")]
        public bool FilmmakerSeeGhosts { get; set; } = false;

        [Description("How many toys can one Ghost have at once?")]
        public int ToyLimit { get; set; } = 1;

        [Description("Areas where Ghosts can create toys. The area exists between a pair of coordinates on each axis.")]
        public Dictionary<Vector3, Vector3> ToySpawnAreas { get; set; } = new Dictionary<Vector3, Vector3>()
        {
            { new(10f, 294f, -12f), new(-10f, 296f, -3f) },
            { new(68f, 282f, -36f), new(142f, 285f, -12f) }
        };

        [Description("Minimum distance between the Ghosts, that will make them hear eachother via RoundSummary channel.")]
        public float HearDistance { get; set; } = 10f;

        [Description("Time after which the duel request will expire.")]
        public float DuelRequestTime { get; set; } = 10f;

        [Description("Should server-specific settings for this plugin be enabled?")]
        public bool SsSettingsEnabled { get; set; } = false;

        [Description("Should server-specific settings be automatically activated for Ghosts upon spawn?")]
        public bool SendSettingsOnSpawn { get; set; } = false;
    }
}
