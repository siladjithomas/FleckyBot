using Bot.Models;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace Bot.Services;

public class LoggingService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly LavaNode<XLavaPlayer> _lavaNode;

    public LoggingService(IServiceProvider services)
    {
        _client = services.GetRequiredService<DiscordSocketClient>();
        _commands = services.GetRequiredService<InteractionService>();
        _lavaNode = services.GetRequiredService<LavaNode<XLavaPlayer>>();

        _client.Log += OnLogAsync;
        _commands.Log += OnLogAsync;
        _lavaNode.OnLog += OnLogAsync;

        Log.Debug("[FleckyBot] Singleton \"LoggingService\" has been set up.");
    }

    public Task OnLogAsync(LogMessage msg)
    {
        var severity = msg.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        Log.Write(severity, msg.Exception, "[{Source}] {msg}", msg.Source ?? "Discord", msg.Message);

        return Task.CompletedTask;
    }
}
