using Discord.WebSocket;
using Quartz;
using TsubaHaru.FleckyBot.Database.DatabaseContexts;

namespace TsubaHaru.FleckyBot.Bot.Services
{
    public class BirthdayService : IJob
    {
        public static readonly JobKey Key = new JobKey("check-birthdays", "flecky");
        private readonly ILogger<BirthdayService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly IServiceScopeFactory _scopeFactory;

        public BirthdayService(ILogger<BirthdayService> logger, DiscordSocketClient client,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _client = client;
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogDebug("Running job {jobName}...", nameof(BirthdayService));

            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var birthdays = dbContext.BirthdayUser?.Where(x => x.Birthday.Date == DateTime.Today).ToList();

            if (birthdays != null && birthdays.Any())
            {
                foreach (var birthday in birthdays)
                {
                    //TODO: set up so that if the birthday is global that it should post in every guild this user is part of
                    //      and if private that it should post it only in the set guild
                    
                    _logger.LogInformation("User {guildUser} with id {guildUserId} has birthday today!", birthday.UserName, birthday.UserId);
                }
            }

            await Task.CompletedTask;
        }
    }
}
