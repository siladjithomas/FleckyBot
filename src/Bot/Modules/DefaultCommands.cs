using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;

namespace Bot.Modules;

public class DefaultCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private readonly CommandHandler _handler;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DefaultCommands(InteractionService service, CommandHandler handler, ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        commands = service;
        _handler = handler;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    [SlashCommand("random", "Get a random number!")]
    public async Task RandomNumber(int min = 1, int max = 100)
    {
        await DeferAsync();

        var random = new Random();

        var embed = new EmbedBuilder()
            .WithTitle($"Your random number: {random.Next(min, max)}")
            .WithDescription("Type `/random [min] [max]` again for a new random number.\nIf no min and max defined then a number between 1 and 100 will be chosen.")
            .WithColor(Color.DarkBlue)
            .WithThumbnailUrl("https://image.similarpng.com/very-thumbnail/2021/05/Rolling-dice-isolated-on-transparent-background-PNG.png")
            .WithFooter(new EmbedFooterBuilder().WithText($"Executed by: {Context.User}").WithIconUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()))
            .WithCurrentTimestamp();

        _logger.LogInformation($"/random slash command run by {Context.User}.");

        await FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("8ball", "Ask a question and get an answer!")]
    public async Task EightBall(string question)
    {
        await DeferAsync();

        var replies = new List<string>
        {
            "yes",
            "no",
            "maybe",
            "hazzzzzy...."
        };

        var answer = replies[new Random().Next(replies.Count - 1)];

        var embed = new EmbedBuilder()
            .WithTitle("8ball")
            .WithDescription($"You asked: [**{question}**], and your answer is: [**{answer}**]")
            .WithFooter(new EmbedFooterBuilder().WithText($"Command executed by {Context.User}.").WithIconUrl(Context.User.GetAvatarUrl()))
            .WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/e/e3/8_ball_icon.svg/1200px-8_ball_icon.svg.png")
            .WithColor(Color.DarkBlue)
            .WithCurrentTimestamp();

        _logger.LogInformation($"/8ball slash command run by {Context.User}.");

        await FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("setup", "Setup the guild in the database")]
    public async Task SetupGuild()
    {
        await DeferAsync(ephemeral: true);

        _logger.LogInformation($"/setup slash command run by {Context.User}.");

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            Guild? guild = context.Guilds.Where(g => g.GuildId == Context.Guild.Id).FirstOrDefault();

            if (guild == null)
            {   
                SocketTextChannel? channel = Context.Channel as SocketTextChannel;
                int count = context.Guilds.Count() + 1;

                context.Guilds.Add(new Guild
                {
                    GuildId = Context.Guild.Id,
                    GuildName = Context.Guild.Name,
                    GuildAdminId = Context.Guild.OwnerId,
                    GuildAdminName = @$"{Context.Guild.Owner.Nickname}#{Context.Guild.Owner.Discriminator}"
                });

                await context.SaveChangesAsync();

                if (channel != null)
                    await FollowupAsync("Guild has been set up. Please use the respective setup tool for each module.");
                else
                    await FollowupAsync("Something happened here. I do not know what but yeah...");
            }
            else
            {
                await FollowupAsync("Guild has been already set up. Skipping...");
                return;
            }
        }
    }

    [SlashCommand("pat", "With this you can pat Flecky")]
    public async Task PatFlecky()
    {
        await DeferAsync();

        var embed = new EmbedBuilder()
            .WithAuthor(Context.User)
            .WithFooter(new EmbedFooterBuilder().WithText("Patting Flecky, since 2023"))
            .WithImageUrl("https://media.discordapp.net/attachments/974447018313408522/974447493557395506/IMG_4177.JPG")
            .WithTitle("Flecky got a headpat!")
            .WithDescription($"Flecky has been patted by {Context.User.Mention}!")
            .WithColor(Color.DarkMagenta)
            .WithCurrentTimestamp();

        await FollowupAsync(embed: embed.Build());
    }
}