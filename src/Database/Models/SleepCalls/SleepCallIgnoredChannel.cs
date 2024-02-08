using System.ComponentModel.DataAnnotations;
using TsubaHaru.FleckyBot.Database.Models.Guilds;

namespace TsubaHaru.FleckyBot.Database.Models.SleepCalls
{
    public class SleepCallIgnoredChannel
    {
        [Key]
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public string? ChannelName { get; set; }
        public bool IsDeleted { get; set; } = false;

        public int GuildId { get; set; }
        public Guild? Guild { get; set; }
    }
}
