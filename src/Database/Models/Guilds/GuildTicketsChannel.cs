namespace TsubaHaru.FleckyBot.Database.Models.Guilds
{
    public class GuildTicketsChannel : GuildChannel
    {
        public List<GuildTicketsGroup>? GuildTicketsGroups { get; set; }
    }
}
