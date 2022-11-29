using Discord;
using Discord.WebSocket;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Bot.Models;
using Serilog;

namespace Bot.Services;

public sealed class AudioService {
    private readonly LavaNode<XLavaPlayer> _lavaNode;
    public readonly HashSet<ulong> VoteQueue;
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
    private readonly DiscordSocketClient _client;

    public AudioService(LavaNode<XLavaPlayer> lavaNode, IServiceProvider provider) {
        _lavaNode = lavaNode;
        _client = provider.GetRequiredService<DiscordSocketClient>();
        _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

        _lavaNode.OnPlayerUpdated += OnPlayerUpdated;
        _lavaNode.OnStatsReceived += OnStatsReceived;
        _lavaNode.OnTrackEnded += OnTrackEnded;
        _lavaNode.OnTrackStarted += OnTrackStarted;
        _lavaNode.OnTrackException += OnTrackException;
        _lavaNode.OnTrackStuck += OnTrackStuck;
        _lavaNode.OnWebSocketClosed += OnWebSocketClosed;

        VoteQueue = new HashSet<ulong>();
    }

    private Task OnPlayerUpdated(PlayerUpdateEventArgs arg) {
        Log.Information($"Track update received for {arg.Track.Title}: {arg.Position}");
        return Task.CompletedTask;
    }

    private Task OnStatsReceived(StatsEventArgs arg) {
        Log.Information($"Lavalink has been up for {arg.Uptime}.");
        return Task.CompletedTask;
    }

    private async Task OnTrackStarted(TrackStartEventArgs arg) 
    {
        await _client.SetActivityAsync(new Game($"{arg.Track.Title}", ActivityType.Listening, ActivityProperties.None));
        
        var embed = new EmbedBuilder()
            .WithTitle(arg.Track.Title)
            .WithDescription(arg.Track.Author)
            .WithUrl(arg.Track.Url)
            .WithImageUrl(await arg.Track.FetchArtworkAsync())
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithFooter(new EmbedFooterBuilder().WithText("Flecky Bot").WithIconUrl("https://media.discordapp.net/attachments/974447018313408522/974447414285054032/IMG_0185.JPG"));
        
        await arg.Player.TextChannel.SendMessageAsync(embed: embed.Build());
        if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value)) 
        {
            return;
        }

        if (value.IsCancellationRequested) 
        {
            return;
        }

        value.Cancel(true);

        await arg.Player.TextChannel.SendMessageAsync("Auto disconnect has been cancelled!");
    }

    private async Task OnTrackEnded(TrackEndedEventArgs args) 
    {
        if (args.Reason != TrackEndReason.Finished) {
            return;
        }

        var player = args.Player;
        if (!player.Queue.TryDequeue(out var lavaTrack)) 
        {
            await _client.SetActivityAsync(new Game("slash commands", ActivityType.Listening, ActivityProperties.None));
            
            var embedComplete = new EmbedBuilder()
                .WithTitle("Queue completed!")
                .WithDescription("Please add more tracks to rock n' roll!")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter(new EmbedFooterBuilder().WithText("Flecky Bot").WithIconUrl("https://media.discordapp.net/attachments/974447018313408522/974447414285054032/IMG_0185.JPG"));
            
            await player.TextChannel.SendMessageAsync(embed: embedComplete.Build());
            //_ = InitiateDisconnectAsync(args.Player, TimeSpan.FromSeconds(10));
            return;
        }

        if (lavaTrack is null) 
        {
            await _client.SetActivityAsync(new Game("slash commands", ActivityType.Listening, ActivityProperties.None));
            await _client.SetStatusAsync(UserStatus.AFK);
            
            Log.Information("Next item in queue is not a track.");
            return;
        }

        await args.Player.PlayAsync(lavaTrack);
        Log.Information($"{args.Reason}: {args.Track.Title} -> {lavaTrack.Title}");
    }

    private async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan) 
    {
        if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value)) 
        {
            value = new CancellationTokenSource();
            _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
        }
        else if (value.IsCancellationRequested) 
        {
            _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
            value = _disconnectTokens[player.VoiceChannel.Id];
        }

        await player.TextChannel.SendMessageAsync($"Auto disconnect initiated! Disconnecting in {timeSpan}...");
        var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
        if (isCancelled) 
        {
            return;
        }

        await _lavaNode.LeaveAsync(player.VoiceChannel);
        await player.TextChannel.SendMessageAsync("Invite me again sometime, sugar.");
    }

    private async Task OnTrackException(TrackExceptionEventArgs arg) 
    {
        Log.Error($"Track {arg.Track.Title} threw an exception. Please check Lavalink console/logs.");
        arg.Player.Queue.Enqueue(arg.Track);
        await arg.Player.TextChannel.SendMessageAsync(
            $"{arg.Track.Title} has been re-added to queue after throwing an exception.");
    }

    private async Task OnTrackStuck(TrackStuckEventArgs arg) 
    {
        Log.Error(
            $"Track {arg.Track.Title} got stuck for {arg.Threshold}ms. Please check Lavalink console/logs.");
        arg.Player.Queue.Enqueue(arg.Track);
        await arg.Player.TextChannel.SendMessageAsync(
            $"{arg.Track.Title} has been re-added to queue after getting stuck.");
    }

    private Task OnWebSocketClosed(WebSocketClosedEventArgs arg) 
    {
        Log.Fatal($"Discord WebSocket connection closed with following reason: {arg.Reason}");
        return Task.CompletedTask;
    }
}