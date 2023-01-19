using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;

namespace Bot.Modules;

[Group("images", "Get some images for... idk... whatever you wish.")]
public class ImageCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private readonly CommandHandler _handler;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ImageCommands(InteractionService service, CommandHandler handler, ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        commands = service;
        _handler = handler;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    [SlashCommand("post", "Post an image from the database")]
    public async Task PostImage(
        [Choice("Headpats", "headpat")]
        [Choice("Slap", "slap")]
        [Choice("Kiss", "kiss")]
        string choice,
        SocketUser user,
        bool hidden = false
    )
    {
        if (hidden)
            await DeferAsync(ephemeral: true);
        else
            await DeferAsync();

        _logger.LogInformation($"/images post slash command run by {Context.User}.");

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            Random rand = new Random();
            int skipper = rand.Next(1, context.Images.Count());

            Database.Models.Image image = context.Images.Where(x => x.Type == choice).OrderBy(y => Guid.NewGuid()).Take(1).First();

            var description = $"{user.Mention}";

            switch(choice)
            {
                case "headpat":
                    description += $" **just got a headpat!**";
                    break;
                case "slap":
                    description += $" **just got slapped!**";
                    break;
                case "kiss":
                    description += $" **just got kissed!**";
                    break;
                default:
                    description += $" **just got rick rolled!**";
                    break;
            }

            var embed = new EmbedBuilder()
                .WithDescription(description)
                .WithImageUrl(image.Uri)
                .WithFooter(new EmbedFooterBuilder() { Text = "Images provided by Gelbooru", IconUrl = "https://pbs.twimg.com/profile_images/1118350008003301381/3gG6lQMl_400x400.png" })
                .WithColor(Color.Blue)
                .WithTimestamp(DateTimeOffset.Now);

            await FollowupAsync(embed: embed.Build());
        }

        await Task.CompletedTask;
    }
}