using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;

namespace Bot.Modules;

[Group("quotes", "This is the group related to quotes")]
public class QuoteCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private readonly ApplicationContext _context;
    private readonly ILogger<Worker> _logger;

    public QuoteCommands(InteractionService service, CommandHandler handler, ApplicationContext context, ILogger<Worker> logger)
    {
        commands = service;
        _handler = handler;
        _context = context;
        _logger = logger;
    }

    [SlashCommand("get", "Get an inspirational quote")]
    public async Task GetQuote()
    {
        await DeferAsync();

        _logger.LogInformation($"/quotes get slash command run by {Context.User}.");

        Random rand = new Random();
        int skipper = rand.Next(1, _context.Quote.Count());

        Quote quote = _context.Quote.Skip(skipper).Take(1).First();

        var embed = new EmbedBuilder()
            .WithTitle($"Quotes")
            .WithDescription($"*{ String.Join("\n", quote.QuoteText) }*\n- { quote.QuoteAuthor }")
            .WithFooter(new EmbedFooterBuilder().WithText($"Executed by { Context.User }").WithIconUrl(Context.User.GetAvatarUrl()))
            .WithColor(Color.DarkGreen)
            .WithCurrentTimestamp();

        await FollowupAsync(embed: embed.Build());
    }
}