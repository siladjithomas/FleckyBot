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
                .Include(x => x.GuildTimetableLines)
                .FirstOrDefault(x => x.GuildId == Context.Guild.Id); 

            if (guild != null)
            {
                var embedTimetableList = new EmbedBuilder()
                    .WithTitle("Derzeitige Termine")
                    .WithDescription("Hier werden die derzeitigen Termine angezeigt.\n\nMögliche Termine:\n- Mo-Fr 20-23 Uhr\n- Sa-So 14-23 Uhr")
                    .WithColor(Color.DarkPurple)
                    .WithCurrentTimestamp();
                var embedAppointment = new EmbedBuilder()
                    .WithTitle("Terminvergabe")
                    .WithDescription("Bitte klicke auf dem Button um einen Termin auszumachen.")
                    .WithColor(Color.Purple);
                var buttonAppointment = new ComponentBuilder()
                    .WithButton("Termin ausmachen", "appointment-create", ButtonStyle.Success);

                if (guild.GuildTimetableLines != null && guild.GuildTimetableLines.Count > 0)
                    foreach (var line in guild.GuildTimetableLines.FindAll(x => x.RequestedTime >= DateTime.Today && !x.IsDone))
                        if (line.RequestedTime.HasValue)
                        {
                            var accepted = line.IsApproved ? "✔" : "✖";

                            embedTimetableList.AddField(line.RequestedTime.Value.ToString("dd.MM.yyyy HH:mm"), line.RequestingUserName + $"\nAccepted: ({accepted})", true);
                        }
                
                var timetableListMessage = await channel.SendMessageAsync(embed: embedTimetableList.Build());
                var requestAppointmentMessage = await channel.SendMessageAsync(embed: embedAppointment.Build(), components: buttonAppointment.Build());

                if (guild.GuildTimetableChannel == null)
                {
                    guild.GuildTimetableChannel = new GuildTimetableChannel
                    {
                        Guild = guild,
                        ChannelId = channel.Id,
                        ChannelName = channel.Name,
                        RequestAppointmentMessageId = requestAppointmentMessage.Id,
                        TimetableListMessageId = timetableListMessage.Id
                    };
                }
                else
                {
                    guild.GuildTimetableChannel.ChannelId = channel.Id;
                    guild.GuildTimetableChannel.ChannelName = channel.Name;
                    guild.GuildTimetableChannel.RequestAppointmentMessageId = requestAppointmentMessage.Id;
                    guild.GuildTimetableChannel.TimetableListMessageId = timetableListMessage.Id;
                }

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

                await ResendTimetableList(guild);
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

                await ResendTimetableList(guild);
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

        [RequireRole(1199324451335061554)]
        [SlashCommand("delete", "Delete an appointment")]
        public async Task DeleteAppointment()
        {
            await DeferAsync(ephemeral: true);

            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?
                .Include(x => x.GuildTimetableChannel)
                .Include(x => x.GuildTimetableLines)
                .FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild != null && guild.GuildTimetableChannel != null && guild.GuildTimetableLines != null)
            {
                var deleteAppointmentSelect = new SelectMenuBuilder()
                    .WithCustomId("appointment-delete");

                foreach (GuildTimetableLine line in guild.GuildTimetableLines)
                    deleteAppointmentSelect.AddOption($"{line.RequestedTime} from {line.RequestingUserName}",
                        $"{line.Id}");

                var components = new ComponentBuilder()
                    .WithSelectMenu(deleteAppointmentSelect);

                await FollowupAsync("Welchen Termin willst du löschen?", components: components.Build());
                return;
            }

            await FollowupAsync("");
        }

        [RequireRole(1199324451335061554)]
        [SlashCommand("approve", "Approve an appointment")]
        public async Task ApproveAppointemnt()
        {
            await DeferAsync(ephemeral: true);

            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?
                .Include(x => x.GuildTimetableChannel)
                .Include(x => x.GuildTimetableLines)
                .FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild != null && guild.GuildTimetableChannel != null && guild.GuildTimetableLines != null)
            {
                var approveAppointmentSelect = new SelectMenuBuilder()
                    .WithCustomId("appointment-approve");

                foreach (GuildTimetableLine line in guild.GuildTimetableLines.FindAll(x => x.IsApproved == false))
                    approveAppointmentSelect.AddOption($"{line.RequestedTime} from {line.RequestingUserName}",
                        $"{line.Id}");

                var components = new ComponentBuilder()
                    .WithSelectMenu(approveAppointmentSelect);

                await FollowupAsync("Welchen Termin willst du akzeptieren?", components: components.Build());
                return;
            }

            await FollowupAsync("");
        }

        private async Task ResendTimetableList(Guild guild)
        {
            if (guild.GuildTimetableChannel != null)
            {
                var embedTimetableList = new EmbedBuilder()
                    .WithTitle("Derzeitige Termine")
                    .WithDescription("Hier werden die derzeitigen Termine angezeigt.\n\nMögliche Termine:\n- Mo-Fr 20-23 Uhr\n- Sa-So 14-23 Uhr")
                    .WithColor(Color.DarkPurple)
                    .WithCurrentTimestamp();

                if (guild.GuildTimetableLines != null && guild.GuildTimetableLines.Count > 0)
                {
                    foreach (var line in guild.GuildTimetableLines.FindAll(x => x.RequestedTime >= DateTime.Today && !x.IsDone))
                        if (line.RequestedTime.HasValue)
                        {
                            var accepted = line.IsApproved ? "✔" : "✖";

                            embedTimetableList.AddField(line.RequestedTime.Value.ToString("dddd, dd MMMM yyyy HH:mm"), line.RequestingUserName + $"\nAccepted: ({accepted})", true);
                        }
                }
                else
                {
                    embedTimetableList.Description += "\n\n***Derzeit sind keine Termine vergeben.***";
                }

                var textChannel = Context.Guild.GetTextChannel(guild.GuildTimetableChannel.ChannelId);

                await textChannel.ModifyMessageAsync(guild.GuildTimetableChannel.TimetableListMessageId, x =>
                {
                    x.Embed = embedTimetableList.Build();
                });
            }
        }
    }
}
