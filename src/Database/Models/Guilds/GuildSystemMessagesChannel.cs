using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models.Guilds
{
    public class GuildSystemMessagesChannel
    {
        [ForeignKey("Guild")]
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public string? ChannelName { get; set; }

        public Guild? Guild { get; set; }
    }
}
