using Discord;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Text.Json;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

namespace Bot.Services;

public sealed class AudioService
{
    private readonly LavaNode _lavaNode;
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<Worker> _logger;

    public AudioService(LavaNode lavaNode, DiscordSocketClient client, ILogger<Worker> logger)
    {
        _lavaNode = lavaNode;
        _client = client;
        _logger = logger;
        _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

        _lavaNode.OnTrackException += OnTrackExceptionAsync;
        _lavaNode.OnTrackStuck += OnTrackStuckAsync;
        _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
        _lavaNode.OnStatsReceived += OnStatsReceivedAsync;
        _lavaNode.OnUpdateReceived += OnUpdateReceivedAsync;
        _lavaNode.OnTrackStart += OnTrackStartAsync;
        _lavaNode.OnTrackEnd += OnTrackEndAsync;
    }

    private static Task OnTrackExceptionAsync(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        arg.Player.Vueue.Enqueue(arg.Track);
        return arg.Player.TextChannel.SendMessageAsync($"{arg.Track} has been requeued because it threw an exception.");
    }

    private static Task OnTrackStuckAsync(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        arg.Player.Vueue.Enqueue(arg.Track);
        return arg.Player.TextChannel.SendMessageAsync($"{arg.Track} has been requeued because it got stuck.");
    }

    private Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg)
    {
        _logger.LogCritical($"{arg.Code} {arg.Reason}");
        return Task.CompletedTask;
    }

    private Task OnStatsReceivedAsync(StatsEventArg arg)
    {
        _logger.LogInformation(JsonSerializer.Serialize(arg));
        return Task.CompletedTask;
    }

    private static Task OnUpdateReceivedAsync(UpdateEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        return arg.Player.TextChannel.SendMessageAsync($"Player update received: {arg.Position}/{arg.Track?.Duration}");
    }

    private static Task OnTrackStartAsync(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        return arg.Player.TextChannel.SendMessageAsync($"Started playing {arg.Track}.");
    }

    private static Task OnTrackEndAsync(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        return arg.Player.TextChannel.SendMessageAsync($"Finished playing {arg.Track}.");
    }
}