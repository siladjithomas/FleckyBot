using System.ComponentModel.DataAnnotations;

namespace TsubaHaru.FleckyBot.Database.Models.Guilds
{
    public class GuildTimetableLine
    {
        [Key]
        public int Id { get; set; }
        public ulong RequestingUserId { get; set; }
        public string? RequestingUserName { get; set; }
        public DateTime? RequestedTime { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsDone { get; set; } = false;

        public int GuildId { get; set; }
        public Guild? Guild { get; set; }
    }
}
