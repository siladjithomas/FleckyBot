using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models.Guilds
{
    public class GuildTicketsChannel
    {
        [ForeignKey("Guild")]
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public string? ChannelName { get; set; }

        public List<GuildTicketsGroup>? GuildTicketsGroups { get; set; }
        public Guild? Guild { get; set; }
    }
}
