using Database.DatabaseContexts;
using Database.Models.Guilds;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Bot.Services
{
    public class TimetableService : IJob
    {
        public static readonly JobKey Key = new JobKey("check-active-timetable", "flecky");
        private readonly ILogger<TimetableService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly IServiceScopeFactory _scopeFactory;

        public TimetableService(ILogger<TimetableService> logger, DiscordSocketClient client, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _client = client;
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guilds = dbContext.Guilds?
                .Include(x => x.GuildTimetableLines)
                .Include(x => x.GuildTimetableChannel);

            if (guilds != null)
            {
                foreach (Guild guild in guilds.ToList())
                {
                    _logger.LogDebug("Found guild with name {guildName} in database.", guild.GuildName);

                    if (guild.GuildTimetableChannel != null && guild.GuildTimetableLines != null && guild.GuildTimetableLines.Count > 0)
                    {
                        SocketGuild socketGuild = _client.Guilds.Where(x => x.Id == guild.GuildId).First() ?? throw new Exception("Guild was not found on discord.");

                        foreach (GuildTimetableLine line in guild.GuildTimetableLines.ToList())
                        {
                            _logger.LogDebug("Found appointment at {requestedTime} for {requestingUser} in guild {guildName}.", line.RequestedTime, line.RequestingUserName, guild.GuildName);

                            if (line.RequestedTime < DateTime.Now)
                            {
                                _logger.LogWarning("Appointment is older than today. Deleting...");
                                dbContext.GuildTimetableLines?.Remove(line);
                            }
                        }

                        _logger.LogDebug("Saving changed to database...");
                        await dbContext.SaveChangesAsync();

                        _logger.LogDebug("Refreshing timetable...");
                        await ResendTimetableList(guild, socketGuild);
                    }
                }
            }
        }

        private async Task ResendTimetableList(Guild guild, SocketGuild socketGuild)
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
                    foreach (var line in guild.GuildTimetableLines.FindAll(x => x.RequestedTime >= DateTime.Today))
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

                var textChannel = socketGuild.GetTextChannel(guild.GuildTimetableChannel.ChannelId);

                await textChannel.ModifyMessageAsync(guild.GuildTimetableChannel.TimetableListMessageId, x =>
                {
                    x.Embed = embedTimetableList.Build();
                });
            }
        }
    }
}
