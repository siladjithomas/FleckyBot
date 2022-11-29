using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bot.Modules;

[Group("images", "Get some images for... idk... whatever you wish.")]
public class ImageCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private ImageService _imageService;

    public ImageCommands(IServiceProvider service)
    {
        commands = service.GetRequiredService<InteractionService>();
        _handler = service.GetRequiredService<CommandHandler>();
        _imageService = service.GetRequiredService<ImageService>();
    }

    [SlashCommand("get", "Get a specific type of image")]
    public async Task GetImage(
        [Choice("Headpats", "headpat")]
        [Choice("Slap", "slap")]
        [Choice("Kiss", "kiss")]
        [Choice("Fuck", "fuck")]
        [Choice("Cum", "cum")]
        string choise,
        bool hidden = false
    )
    {
        if (hidden)
            await DeferAsync(ephemeral: true);
        else
            await DeferAsync();

        var choiseImage = await _imageService.GetRandom(choise);
        var description = "";

        switch(choise)
        {
            case "headpat":
                description = $"{Context.User.Mention} **just got a headpat!**";
                break;
            case "slap":
                description = $"{Context.User.Mention} **just got slapped!**";
                break;
            case "kiss":
                description = $"{Context.User.Mention} **just got kissed!**";
                break;
            case "fuck":
                description = $"{Context.User.Mention} **just got fucked!**";
                break;
            case "cum":
                description = $"{Context.User.Mention} **found out that it's breeding time!**";
                break;
            default:
                description = $"{Context.User.Mention} **just got rick rolled!**";
                break;
        }

        var embed = new EmbedBuilder()
            .WithDescription(description)
            .WithImageUrl(choiseImage.url)
            .WithFooter(new EmbedFooterBuilder() { Text = "Images provided by Gelbooru", IconUrl = "https://pbs.twimg.com/profile_images/1118350008003301381/3gG6lQMl_400x400.png" })
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.Now);

        await FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("mention", "Get a specific type of image and mention someone")]
    public async Task GetMentionImage(
        [Choice("Headpats", "headpat")]
        [Choice("Slap", "slap")]
        [Choice("Kiss", "kiss")]
        [Choice("Fuck", "fuck")]
        [Choice("Cum", "cum")]
        string choise,
        SocketGuildUser user
    )
    {
        await DeferAsync();

        var choiseImage = await _imageService.GetRandom(choise);
        var description = "";

        switch(choise)
        {
            case "headpat":
                description = $"{user.Mention} **just got a headpat from {Context.User.Mention}!**";
                break;
            case "slap":
                description = $"{user.Mention} **just got slapped from {Context.User.Mention}!**";
                break;
            case "kiss":
                description = $"{user.Mention} **just got kissed from {Context.User.Mention}!**";
                break;
            case "fuck":
                description = $"{user.Mention} **just got fucked from {Context.User.Mention}!**";
                break;
            case "cum":
                description = $"{user.Mention} **found out that it's breeding time and got bred from {Context.User.Mention}!**";
                break;
            default:
                description = $"{user.Mention} **just got rick rolled!**";
                break;
        }

        var embed = new EmbedBuilder()
            .WithDescription(description)
            .WithImageUrl(choiseImage.url)
            .WithFooter(new EmbedFooterBuilder() { Text = "Images provided by Gelbooru", IconUrl = "https://pbs.twimg.com/profile_images/1118350008003301381/3gG6lQMl_400x400.png" })
            .WithColor(Color.Blue)
            .WithTimestamp(DateTimeOffset.Now);

        await FollowupAsync(embed: embed.Build());
    }
}