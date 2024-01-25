using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.DatabaseContexts;
using Database.Models.Guilds;
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

            var guilds = dbContext.Guilds?.Include(x => x.GuildTimetableLines);

            if (guilds != null)
                foreach (Guild guild in guilds)
                {
                    _logger.LogDebug("Found guild with name {guildName}.", guild.GuildName);
                }
        }
    }
}
