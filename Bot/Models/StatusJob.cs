using Quartz;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;

namespace Bot.Models;

public class StatusJob : IJob
{
    public IServiceProvider _provider { get; set; }
    private DiscordSocketClient _client;
    
    public StatusJob(IServiceProvider provider)
    {
        _provider = provider;
        _client = _provider.GetRequiredService<DiscordSocketClient>();
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        string message = "Testing message run at: " + context.NextFireTimeUtc.ToString();

        try
        {
            Log.Debug(message);

            await _client.SetActivityAsync(new Game("to dad's advices", ActivityType.Listening, ActivityProperties.None));
            await _client.SetStatusAsync(UserStatus.AFK);
        }
        catch
        {
            throw;
        }
        
        await Task.FromResult(true);
    }
}