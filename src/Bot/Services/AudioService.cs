using System.Collections.Concurrent;
using System.Text.Json;
using Discord;
using Discord.WebSocket;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

namespace TsubaHaru.FleckyBot.Bot.Services;

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

    private Task OnTrackExceptionAsync(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        _logger.LogError($"Track {arg.Track.Title} threw an exception. Please check Lavalink console/logs.");
        arg.Player.Vueue.Enqueue(arg.Track);
        return arg.Player.TextChannel.SendMessageAsync($"{arg.Track} has been requeued because it threw an exception.");
    }

    private Task OnTrackStuckAsync(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        _logger.LogError($"Track {arg.Track.Title} got stuck for {arg.Threshold}ms. Please check Lavalink console/logs.");
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

    private Task OnUpdateReceivedAsync(UpdateEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        _logger.LogInformation($"Track update received for {arg.Track.Title}: {arg.Position}");
        return Task.CompletedTask;
    }

    private async Task OnTrackStartAsync(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        var embed = new EmbedBuilder()
            .WithTitle(arg.Track.Title)
            .WithDescription(arg.Track.Author)
            .WithUrl(arg.Track.Url)
            //.WithImageUrl(await arg.Track.FetchArtworkAsync())
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithFooter(new EmbedFooterBuilder().WithText("Flecky Bot").WithIconUrl("https://media.discordapp.net/attachments/974447018313408522/974447414285054032/IMG_0185.JPG"));
        
        await arg.Player.TextChannel.SendMessageAsync(embed: embed.Build());
        
        if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value))
            return;
        
        if (value.IsCancellationRequested)
            return;

        value.Cancel(true);

        await arg.Player.TextChannel.SendMessageAsync("Auto disconnect has been canceled!");
    }

    private async Task OnTrackEndAsync(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
    {
        if (arg.Reason != TrackEndReason.Finished)
            return;

        if (!arg.Player.Vueue.TryDequeue(out var lavaTrack))
        {
            var embedComplete = new EmbedBuilder()
                .WithTitle("Queue completed!")
                .WithDescription("Please add more tracks to rock n' roll!")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter(new EmbedFooterBuilder().WithText("Flecky Bot").WithIconUrl("https://media.discordapp.net/attachments/974447018313408522/974447414285054032/IMG_0185.JPG"));
            
            await arg.Player.TextChannel.SendMessageAsync(embed: embedComplete.Build());
            return;
        }
        
        if (lavaTrack is null)
        {
            _logger.LogInformation("Next item in queue is not a track.");
            return;
        }

        await arg.Player.PlayAsync(lavaTrack);
        _logger.LogInformation($"{arg.Reason} {arg.Track.Title} -> {lavaTrack.Title}");
    }
}