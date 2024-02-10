using TsubaHaru.FleckyBot.Database.DatabaseContexts;
using TsubaHaru.FleckyBot.Database.Models.Guilds;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Bot.Services
{
    [DisallowConcurrentExecution]
    public class VoteService : IJob
    {
        public static readonly JobKey Key = new JobKey("check-active-votes", "flecky");
        private readonly ILogger<VoteService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly IServiceScopeFactory _scopeFactory;

        public VoteService(ILogger<VoteService> logger, DiscordSocketClient client, IServiceScopeFactory scopeFactory)
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
                .Include(x => x.GuildVotesChannel);

            if (guilds != null)
            {
                foreach (Guild guild in guilds.ToList())
                {

                }
            }
        }
    }
}
