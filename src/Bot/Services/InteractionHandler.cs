using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Models;

namespace Bot.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly ILogger<Worker> _logger;

    public InteractionHandler(DiscordSocketClient client, InteractionService commands, ILogger<Worker> logger)
    {
        _client = client;
        _commands = commands;
        _logger = logger;

        _client.Ready += ReadyAsync;
    }

    private async Task ReadyAsync()
    {
#if DEBUG
        _logger.LogInformation("In debug mode, adding commands to Guild with ID 799042503570358313...");
        await _commands.RegisterCommandsToGuildAsync(799042503570358313);
#else
        _logger.LogInformation("In production mode, adding commands globally...");
        await _commands.RegisterCommandsGloballyAsync(true);
#endif

        _logger.LogInformation($"Logged in as {_client.CurrentUser}, shard id {_client.ShardId}");

        await _client.SetActivityAsync(new Game("slash commands", ActivityType.Listening, ActivityProperties.None));
        await _client.SetStatusAsync(UserStatus.AFK);

        _logger.LogInformation($"Status of {_client.CurrentUser} on shard id {_client.ShardId} has been set properly");
    }
}