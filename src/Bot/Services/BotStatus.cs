using Discord;
using Discord.WebSocket;
using Quartz;

namespace TsubaHaru.FleckyBot.Bot.Services;

[DisallowConcurrentExecution]
public class BotStatus : IJob
{
    public static readonly JobKey Key = new JobKey("change-status", "flecky");
    private readonly ILogger<BotStatus> _logger;
    private readonly DiscordSocketClient _client;
    
    public BotStatus(ILogger<BotStatus> logger, DiscordSocketClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        List<string> statuses = new List<string>
        {
            "woof woof",
            "bark bark",
            "yawn",
            "FOOOD"
        };

        Random rand = new Random();
        int skipper = rand.Next(0, statuses.Count - 1);

        await _client.SetActivityAsync(new Game(statuses[skipper], ActivityType.Listening, ActivityProperties.None));

        _logger.LogInformation("Set new status for FleckyBot");
    }
}