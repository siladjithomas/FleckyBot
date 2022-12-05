using Bot.Models;
using Bot.Services;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;
using SpotifyAPI.Web;
using System.Text.RegularExpressions;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Bot.Modules;

[Group("audio", "A group related to audio player stuff, powered by Victoria/Lavanode")]
public sealed class AudioCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly LavaNode<XLavaPlayer> _lavaNode;
    private readonly AudioService _audioService;
    private readonly DiscordSocketClient _client;
    private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

    public AudioCommands(IServiceProvider service)
    {
        _lavaNode = service.GetRequiredService<LavaNode<XLavaPlayer>>();
        _audioService = service.GetRequiredService<AudioService>();
        _client = service.GetRequiredService<DiscordSocketClient>();
    }

    [SlashCommand("join", "Let the bot join a voice channel")]
    public async Task JoinAsync() {
        await DeferAsync(ephemeral: true);
        
        if (_lavaNode.HasPlayer(Context.Guild)) {
            await FollowupAsync("I'm already connected to a voice channel!");
            return;
        }

        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null) {
            await FollowupAsync("You must be connected to a voice channel!");
            return;
        }

        try {
            await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            await FollowupAsync($"Joined {voiceState.VoiceChannel.Mention}!");
        }
        catch (Exception exception) {
            Log.Error($"[{exception.Source}] {exception.Message}");
        }

        try
        {
            var stageChannel = (SocketStageChannel)voiceState.VoiceChannel;
            await stageChannel.BecomeSpeakerAsync();
            await stageChannel.StartStageAsync("Playing random music feat. Flecky Bot");
        }
        catch (Exception exception)
        {
            Log.Information($"This is not a stage channel. Continuing.");
            Log.Debug($"[{exception.Source}] {exception.Message}");
        }
    }

    [SlashCommand("leave", "Let the bot leave a voice channel")]
    public async Task LeaveAsync() {
        await DeferAsync(ephemeral: true);
        
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to any voice channels!");
            return;
        }

        var voiceChannel = (Context.User as IVoiceState)?.VoiceChannel ?? player.VoiceChannel;
        if (voiceChannel == null) {
            await FollowupAsync("Not sure which voice channel to disconnect from.");
            return;
        }

        try {
            await _lavaNode.LeaveAsync(voiceChannel);
            await _client.SetActivityAsync(new Game("slash commands", ActivityType.Listening, ActivityProperties.None));
            await FollowupAsync($"I've left {voiceChannel.Mention}!");
        }
        catch (Exception exception) {
            await FollowupAsync(exception.Message);
        }

        try
        {
            var stageChannel = (SocketStageChannel)voiceChannel;
            await stageChannel.StopStageAsync();
        }
        catch (Exception exception)
        {
            Log.Information($"This is not a stage channel. Continuing.");
            Log.Debug($"[{exception.Source}] {exception.Message}");
        }
    }

    [SlashCommand("play", "Play a song")]
    public async Task PlayAsync(string searchQuery) 
    {
        await DeferAsync(ephemeral: true);
        
        if (string.IsNullOrWhiteSpace(searchQuery)) {
            await FollowupAsync("Please provide search terms.");
            return;
        }

        if (!_lavaNode.HasPlayer(Context.Guild)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        var searchResponse = await _lavaNode.SearchAsync(SearchType.Direct, searchQuery);
        if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches) {
            await FollowupAsync($"I wasn't able to find anything for `{searchQuery}`.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name)) {
            player.Queue.Enqueue(searchResponse.Tracks);
            await FollowupAsync($"Enqueued {searchResponse.Tracks.Count} songs.");
        }
        else {
            var track = searchResponse.Tracks.FirstOrDefault();
            player.Queue.Enqueue(track);

            await FollowupAsync($"Enqueued {track?.Title}");
        }

        if (player.PlayerState is PlayerState.Playing or PlayerState.Paused) {
            return;
        }

        player.Queue.TryDequeue(out var lavaTrack);
        await player.PlayAsync(x => {
            x.Track = lavaTrack;
            x.ShouldPause = false;
        });
    }

    [SlashCommand("pause", "Pause a song")]
    public async Task PauseAsync() {
        await DeferAsync(ephemeral: true);
        
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing) {
            await FollowupAsync("I cannot pause when I'm not playing anything!");
            return;
        }

        try {
            await player.PauseAsync();
            await FollowupAsync($"Paused: {player.Track.Title}");
        }
        catch (Exception exception) {
            await FollowupAsync(exception.Message);
        }
    }

    [SlashCommand("resume", "Resume a paused song")]
    public async Task ResumeAsync() {
        await DeferAsync(ephemeral: true);
        
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Paused) {
            await FollowupAsync("I cannot resume when I'm not playing anything!");
            return;
        }

        try {
            await player.ResumeAsync();
            await FollowupAsync($"Resumed: {player.Track.Title}");
        }
        catch (Exception exception) {
            await FollowupAsync(exception.Message);
        }
    }

    [SlashCommand("stop", "Stop playing a song")]
    public async Task StopAsync() {
        await DeferAsync(ephemeral: true);
        
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState == PlayerState.Stopped) {
            await FollowupAsync("Woaaah there, I can't stop the stopped forced.");
            return;
        }

        try {
            await player.StopAsync();
            await _client.SetActivityAsync(new Game("slash commands", ActivityType.Listening, ActivityProperties.None));
            await FollowupAsync("No longer playing anything.");
        }
        catch (Exception exception) {
            await FollowupAsync(exception.Message);
        }
    }

    [SlashCommand("skip", "Skip a song")]
    public async Task SkipAsync() {
        await DeferAsync(ephemeral: true);
        
        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState != PlayerState.Playing) {
            await FollowupAsync("Woaaah there, I can't skip when nothing is playing.");
            return;
        }

        // As this is not used by many people I am going to turn off voting for now
        /*
        var voiceChannelUsers = (player.VoiceChannel as SocketVoiceChannel)?.Users
            .Where(x => !x.IsBot)
            .ToArray();

        if (_audioService.VoteQueue.Contains(Context.User.Id)) {
            await FollowupAsync("You can't vote again.");
            return;
        }

        _audioService.VoteQueue.Add(Context.User.Id);
        if (voiceChannelUsers != null) {
            var percentage = _audioService.VoteQueue.Count / voiceChannelUsers.Length * 100;
            if (percentage < 40) {
                await FollowupAsync("You need more than 85% votes to skip this song.");
                return;
            }
        }
        */

        try {
            var (oldTrack, currenTrack) = await player.SkipAsync();
            await FollowupAsync($"Skipped: {oldTrack.Title}\nNow Playing: {player.Track.Title}");
            Log.Information($"Skipped: {oldTrack.Title}\nNow Playing: {player.Track.Title}");
        }
        catch (Exception exception) {
            Log.Information(exception.Message);
        }

        _audioService.VoteQueue.Clear();
    }
}