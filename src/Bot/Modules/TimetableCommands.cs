using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;

namespace Bot.Modules
{
    [Group("timetable", "Timetable related stuff")]
    public class TimetableCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private CommandHandler _handler;
        private readonly ApplicationContext _context;
        private readonly ILogger<TimetableCommands> _logger;

        public TimetableCommands(InteractionService commands, CommandHandler handler, ApplicationContext context, ILogger<TimetableCommands> logger)
        {
            this.commands = commands;
            _handler = handler;
            _context = context;
            _logger = logger;
        }

        [RequireRole(1195582801563635772)]
        [SlashCommand("add", "Add an appointment")]
        public async Task AddAppointment(DateTime startTime, SocketUser requestingUser)
        {
            await DeferAsync(ephemeral: true);
            
            _logger.LogDebug("Requested appointment from {user} on {dateTime}", requestingUser, startTime.ToString());

            await FollowupAsync($"Requested appointment from {requestingUser.Mention} on {startTime}");
        }
    }
}
