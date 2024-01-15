using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models.Guilds
{
    public class GuildTicketsChannel : GuildChannel
    {
        public List<GuildTicketsGroup>? GuildTicketsGroups { get; set; }
    }
}
