using System.ComponentModel.DataAnnotations;
using TsubaHaru.FleckyBot.Database.Models.Guilds;

namespace TsubaHaru.FleckyBot.Database.Models.SleepCalls
{
    public class SleepCallCategory
    {
        [Key]
        public int Id { get; set; }
        public ulong CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsDeleted { get; set; } = false;

        public List<SleepCallActiveChannel>? SleepCallActiveChannels { get; set; }
        public List<SleepCallGroup>? SleepCallGroups { get; set; }

        public int GuildId { get; set; }
        public Guild? Guild { get; set; }
    }
}
