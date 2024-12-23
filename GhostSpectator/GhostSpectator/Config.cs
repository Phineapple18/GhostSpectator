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
        [Description("Should the plugin be enabled?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Should debug be enabled?")]
        public bool Debug { get; set; } = false;

        [Description("Ghost nickname color.")]
        public string GhostColor { get; set; } = "#A0A0A0";

        [Description("Ghost health.")]
        public float GhostHealth { get; set; } = 150f;

        [Description("Spawn message duration.")]
        public ushort SpawnmessageDuration { get; set; } = 5;

        [Description("Ghost spawn positions.")]
        public List<string> SpawnPositions { get; set; } = new List<string>{ "9, 1002, 1" };

        [Description("Item given to every Ghost, that allows them to teleport to alive players when dropped. Set to \"None\" to disable.")]
        public ItemType TeleportItem { get; set; } = ItemType.Lantern;

        [Description("Roles, that Ghosts cannot teleport to. SCP-079 is already included.")]
        public List<RoleTypeId> RoleTeleportBlacklist { get; set; } = new List<RoleTypeId>
        {
            RoleTypeId.Tutorial
        };

        [Description("Should Ghosts, that don't have permission, be despawned and not allowed to spawn after warhead detonation?")]
        public bool DespawnOnDetonation { get; set; } = true;

        [Description("Should Spectators be able to see Ghosts, if the spectated player is not a Ghost?")]
        public bool AlwaysSeeGhosts { get; set; } = false;

        [Description("Should Filmmakers be able to see Ghosts?")]
        public bool FilmmakerSeeGhosts { get; set; } = false;

        [Description("How many shooting targets at once can one Ghost have created?")]
        public int TargetLimit { get; set; } = 1;

        [Description("Areas where Ghosts can create shooting targets. The area exists between a pair of coordinates on each axis.")]
        public Dictionary<string, string> ShootingRanges { get; set; } = new Dictionary<string, string>()
        {
            { "10, 994, -12", "-10, 996, -4" },
            { "68, 982, -36", "142, 985, -12"}
        };

        [Description("Minimum distance between the Ghosts, that will make them hear eachother via RoundSummary channel instead of Proximity channel (if they have enabled listening to Ghosts).")]
        public float HearDistance { get; set; } = 10f;

        [Description("Time after which the duel request will expire.")]
        public float DuelRequestTime { get; set; } = 10f;
    }
}
