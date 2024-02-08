using System.ComponentModel.DataAnnotations.Schema;

namespace TsubaHaru.FleckyBot.Database.Models.Guilds
{
    public class GuildChannel
    {
        [ForeignKey("Guild")]
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public string? ChannelName { get; set; }

        public Guild? Guild { get; set; }
    }
}
