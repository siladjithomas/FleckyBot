namespace TsubaHaru.FleckyBot.Database.Models.Guilds
{
    public class GuildTimetableChannel : GuildChannel
    {
        public ulong TimetableListMessageId { get; set; }
        public ulong RequestAppointmentMessageId { get; set; }
    }
}
