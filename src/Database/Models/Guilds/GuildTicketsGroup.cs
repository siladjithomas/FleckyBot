using System.ComponentModel.DataAnnotations;

namespace Database.Models.Guilds
{
    public class GuildTicketsGroup
    {
        [Key]
        public int Id { get; set; }
        public ulong GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupType { get; set; }

        public GuildTicketsChannel? GuildTicketsChannel { get; set; }
        public int? GuildTicketsChannelId { get; set; }
    }
}
