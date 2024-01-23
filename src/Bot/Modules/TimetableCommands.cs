using System.Globalization;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;
using Database.Models.Guilds;
using Microsoft.EntityFrameworkCore;

namespace Bot.Modules
{
    [Group("timetable", "Timetable related stuff")]
    public class TimetableCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private CommandHandler _handler;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TimetableCommands> _logger;

        public TimetableCommands(InteractionService commands, CommandHandler handler, IServiceScopeFactory scopeFactory, ILogger<TimetableCommands> logger)
        {
            this.commands = commands;
            _handler = handler;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        [RequireOwner]
        [SlashCommand("post", "Post the timetable to a channel")]
        public async Task PostTimetableToChannel(SocketTextChannel channel)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?
                .Include(x => x.GuildTimetableChannel)
                .FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild != null)
            {
                var timetableListMessage = await channel.SendMessageAsync("Timetable currently not available. Please check later.");
                var requestAppointmentMessage = await channel.SendMessageAsync("Requests currently not available. Please check later.");

                var newChannel = new GuildTimetableChannel
                {
                    Guild = guild,
                    ChannelId = channel.Id,
                    ChannelName = channel.Name,
                    RequestAppointmentMessageId = requestAppointmentMessage.Id,
                    TimetableListMessageId = timetableListMessage.Id
                };

                guild.GuildTimetableChannel = newChannel;

                await context.SaveChangesAsync();

                await FollowupAsync($"Posted to channel {channel.Mention} and saved in database.");
            }
            else
            {
                await FollowupAsync("Guild is not set up.");
            }
        }

        [RequireRole(1199324451335061554)]
        [SlashCommand("add", "Add an appointment")]
        public async Task AddAppointment(DateTime startTime, SocketUser requestingUser)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?
                .Include(x => x.GuildTimetableChannel)
                .Include(x => x.GuildTimetableLines)
                .FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild != null && guild.GuildTimetableChannel != null)
            {
                var newLine = new GuildTimetableLine
                {
                    Guild = guild,
                    IsApproved = true,
                    RequestedTime = startTime,
                    RequestingUserId = requestingUser.Id,
                    RequestingUserName = requestingUser.Username
                };

                if (guild.GuildTimetableLines == null)
                    guild.GuildTimetableLines = new List<GuildTimetableLine> { newLine };
                else
                    guild.GuildTimetableLines.Add(newLine);

                await context.SaveChangesAsync();

                _logger.LogDebug("Added appointment from {user} on {dateTime}", requestingUser, startTime.ToString(CultureInfo.CurrentCulture));

                await FollowupAsync($"Added appointment from {requestingUser.Mention} on {startTime}");
            }
            else if (guild != null && guild.GuildTimetableChannel == null)
            {
                await FollowupAsync("Guild timetable channel is not set.");
            }
            else
            {
                await FollowupAsync("Guild is not set up.");
            }
        }

        [SlashCommand("request", "Request an appointment")]
        public async Task RequestAppointment(DateTime startTime)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?
                .Include(x => x.GuildTimetableChannel)
                .Include(x => x.GuildTimetableLines)
                .FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild != null && guild.GuildTimetableChannel != null)
            {
                var newLine = new GuildTimetableLine
                {
                    Guild = guild,
                    RequestedTime = startTime,
                    RequestingUserId = Context.User.Id,
                    RequestingUserName = Context.User.Username
                };

                if (guild.GuildTimetableLines == null)
                    guild.GuildTimetableLines = new List<GuildTimetableLine> { newLine };
                else
                    guild.GuildTimetableLines.Add(newLine);

                await context.SaveChangesAsync();

                _logger.LogDebug("Requested appointment from {user} on {dateTime}", Context.User, startTime.ToString(CultureInfo.CurrentCulture));

                await FollowupAsync($"Termin auf {startTime} gesetzt. Warte auf Bestätigung...");
            }
            else if (guild != null && guild.GuildTimetableChannel == null)
            {
                await FollowupAsync("Guild timetable channel ist nicht gesetzt.");
            }
            else
            {
                await FollowupAsync("Die Gilde wurde nicht eingerichtet.");
            }
        }
    }
}
