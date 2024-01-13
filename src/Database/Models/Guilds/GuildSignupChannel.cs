using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models.Guilds
{
    public class GuildSignupChannel
    {
        [ForeignKey("Guild")]
        public int Id { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; } = string.Empty;

        public Guild? Guild { get; set; }
    }
}
