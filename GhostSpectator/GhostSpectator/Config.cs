using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

using PlayerRoles;

namespace GhostSpectator
{
    public class Config
    {
        [Description("Should plugin be enabled?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Should debug be enabled?")]
        public bool Debug { get; set; } = false;

        [Description("Color of Ghost nickname.")]
        public string GhostColor { get; set; } = "#A0A0A0";

        [Description("Maximum health of Ghost.")]
        public float GhostHealth { get; set; } = 150f;

        [Description("Duration of spawn message.")]
        public ushort SpawnmessageDuration { get; set; } = 5;

        [Description("Spawn positions for Ghosts.")]
        public List<string> SpawnPositions { get; set; } = new List<string>{ "9, 1002, 1" };

        [Description("Item given to every Ghost, that can teleport them to an alive player when dropped.")]
        public ItemType TeleportItem { get; set; } = ItemType.Lantern;

        [Description("List of roles, that Ghosts cannot be teleported to. SCP-079 is already included.")]
        public List<RoleTypeId> RoleTeleportBlacklist { get; set; } = new List<RoleTypeId>
        {
            RoleTypeId.Tutorial
        };

        [Description("Should Ghosts be despawned (and not allowed to spawn) after warhead detonation?")]
        public bool DespawnOnDetonation { get; set; } = true;

        [Description("Should Spectators be able to see Ghosts, if spectated player is not a Ghost?")]
        public bool AlwaysSeeGhosts { get; set; } = false;

        [Description("Should Filmmakers be able to see Ghosts?")]
        public bool FilmmakerSeeGhosts { get; set; } = false;

        [Description("How many shooting targets can one Ghost have spawned?")]
        public int TargetLimit { get; set; } = 1;

        [Description("Areas, where Ghosts can spawn shooting targets. For each area, provide a pair of positions. Their coordinates will be used as a perimeter along every axis.")]
        public Dictionary<string, string> ShootingRanges { get; set; } = new Dictionary<string, string>()
        {
            { "10, 995, -12", "-10, 996, -4" },
            { "68, 983, -36", "142, 985, -12"}
        };

        [Description("Minimum distance between the Ghosts, that will make them hear eachother via RoundSummary channel instead of Proximity channel (if they have enabled listening to Ghosts).")]
        public float HearDistance { get; set; } = 10f;

        [Description("Time, which the duel request will expire after.")]
        public float DuelRequestTime { get; set; } = 10f;
    }
}
