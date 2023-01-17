using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;
using Victoria;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Responses.Search;

namespace Bot.Modules;

[Group("audio", "A group for audio related commands")]
public class AudioCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly LavaNode _lavaNode;
    private readonly AudioService _audioService;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<Worker> _logger;
    private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

    public AudioCommands(LavaNode lavaNode, AudioService audioService, DiscordSocketClient client, ILogger<Worker> logger)
    {
        _lavaNode = lavaNode;
        _audioService = audioService;
        _client = client;
        _logger = logger;
    }

    [SlashCommand("join", "Bot will join the current audio channel you are in")]
    public async Task JoinAsync()
    {
        await DeferAsync(ephemeral: true);

        if (!_lavaNode.IsConnected)
            await _lavaNode.ConnectAsync();

        if (_lavaNode.HasPlayer(Context.Guild))
        {
            _logger.LogWarning($"{Context.User} tried to add another instance into a voice channel, even when the bot is already in a voice channel. Canceling request.");
            await FollowupAsync("I'm already connected to a voice channel!");
            return;
        }

        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null)
        {
            _logger.LogWarning($"{Context.User} tried to add the bot to a voice channel, even if the user is not in any. Cancelling request.");
            await FollowupAsync("You must be connected to a voice channel!");
            return;
        }

        try
        {
            await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            _logger.LogInformation($"{Context.User} added me to the voice channel {voiceState.VoiceChannel.Name}.");
            await FollowupAsync($"Joined {voiceState.VoiceChannel.Name}!");
        }
        catch (Exception exception)
        {
            _logger.LogError($"{exception.Source} {exception.Message}");
            await FollowupAsync("Sadly something happened. Please try again later.");
        }

        try
        {
            var stageChannel = (SocketStageChannel)voiceState.VoiceChannel;
            await stageChannel.BecomeSpeakerAsync();
            await stageChannel.StartStageAsync("Playing random music feat. FleckyBot");
        }
        catch (Exception exception)
        {
            _logger.LogInformation($"This is not a stage channel. Continuing...");
            _logger.LogDebug($"{exception.Source} {exception.Message}");
        }
    }

    [SlashCommand("leave", "Leave a voice channel")]
    public async Task LeaveAsync()
    {
        await DeferAsync(ephemeral: true);

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            _logger.LogWarning($"{Context.User} tried to remove the bot from a voice channel, which is not possible as the bot isn't in any voice channel.");
            await FollowupAsync("I'm not connected to any voice channel.");
        }

        var voiceChannel = (Context.User as IVoiceState)?.VoiceChannel ?? player.VoiceChannel;
        if (voiceChannel == null)
        {
            _logger.LogWarning($"{Context.User} tried to remove me from a voice channel, but I am not in any. Ignoring request...");
            await FollowupAsync("Not sure which voice channel to disconnect from.");
            return;
        }

        try
        {
            await _lavaNode.LeaveAsync(voiceChannel);
            _logger.LogInformation($"{Context.User} removed me from the voice channel {voiceChannel.Name}.");
            await FollowupAsync($"I've left {voiceChannel.Mention}!");
        }
        catch (Exception exception)
        {
            await FollowupAsync(exception.Message);
        }

        try
        {
            var stageChannel = (SocketStageChannel)voiceChannel;
            await stageChannel.StopStageAsync();
        }
        catch (Exception exception)
        {
            _logger.LogInformation($"This is not a stage channel. Continuing.");
            _logger.LogDebug($"[{exception.Source}] {exception.Message}");
        }

        if (_lavaNode.IsConnected)
            await _lavaNode.DisconnectAsync();
    }

    [SlashCommand("play", "Play a song on the bot")]
    public async Task PlayAsync(string searchQuery)
    {
        await DeferAsync(ephemeral: true);

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await FollowupAsync("Please provide search terms.");
            return;
        }

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            await FollowupAsync("Player is not connected. Please use `/audio join` to join the bot into the channel.");
        }

        var searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, searchQuery);
        if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
        {
            await FollowupAsync($"I wasn't able to find anything for `{searchQuery}`.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
        {
            player.Vueue.Enqueue(searchResponse.Tracks);
            await FollowupAsync($"Enqueued {searchResponse.Tracks.Count} songs.");
        }
        else
        {
            var track = searchResponse.Tracks.FirstOrDefault();
            player.Vueue.Enqueue(track);

            await FollowupAsync($"Enqueued {track?.Title}");
        }

        if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
            return;

        player.Vueue.TryDequeue(out var lavaTrack);
        await player.PlayAsync(lavaTrack);
        _logger.LogDebug($"Now really started playing {lavaTrack.Title}!");
    }

    [SlashCommand("stop", "Stop playing a song")]
    public async Task StopAsync()
    {
        await DeferAsync(ephemeral: true);

        if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
        {
            _logger.LogWarning($"{Context.User}'s command was not successful. Bot is not connected to a voice channel.");
            await FollowupAsync("I'm not connected to a voice channel.");
            return;
        }

        if (player.PlayerState == PlayerState.Stopped)
        {
            _logger.LogWarning($"{Context.User}'s command was not successful. Bot is already stopped.");
            await FollowupAsync("Nothing is playing right now.");
            return;
        }

        try
        {
            await player.StopAsync();
            _logger.LogInformation("Bot has stopped playing.");
            await FollowupAsync("No longer playing anything.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"{exception.Source} {exception.Message}");
            await FollowupAsync("There was an exception. Please try again later.");
        }
    }
}