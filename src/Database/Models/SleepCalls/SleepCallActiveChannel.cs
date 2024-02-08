using System.ComponentModel.DataAnnotations;

namespace TsubaHaru.FleckyBot.Database.Models.SleepCalls
{
    public class SleepCallActiveChannel
    {
        [Key]
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public string? ChannelName { get; set; }
        public bool IsDeleted { get; set; } = false;

        public int SleepCallCategoryId { get; set; }
        public SleepCallCategory? SleepCallCategory { get; set; }
    }
}
