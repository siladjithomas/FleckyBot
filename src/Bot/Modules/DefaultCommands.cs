using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot;
using Bot.Services;

namespace Bot.Modules;

public class DefaultCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private readonly CommandHandler _handler;
    private readonly ILogger<Worker> _logger;

    public DefaultCommands(InteractionService service, CommandHandler handler, ILogger<Worker> logger)
    {
        commands = service;
        _handler = handler;
        _logger = logger;
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
        
        await FollowupAsync(embed: embed.Build());
    }
}