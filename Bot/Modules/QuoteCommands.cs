using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bot.Modules;

[Group("quotes", "This is the group related to quotes")]
public class QuoteCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private QuoteService _quoteService;

    public QuoteCommands(IServiceProvider service)
    {
        commands = service.GetRequiredService<InteractionService>();
        _handler = service.GetRequiredService<CommandHandler>();
        _quoteService = service.GetRequiredService<QuoteService>();
    }

    [SlashCommand("get", "Get a quote from the database, provided by DerEingerostete#9999!")]
    public async Task GetQuote(IGuildUser? user = null)
    {
        await DeferAsync();

        var quote = await _quoteService.GetRandom();

        var embed = new EmbedBuilder()
            .WithTitle($"Quotes")
            .WithDescription($"*{ String.Join("\n", quote.quote) }*\n- { quote.author }")
            .WithFooter(new EmbedFooterBuilder().WithText($"Executed by { Context.User }").WithIconUrl(Context.User.GetAvatarUrl()))
            .WithColor(Color.DarkGreen)
            .WithCurrentTimestamp();

        if (user != null)
            await FollowupAsync($"This is a quote for you, {user.Mention}!", embed: embed.Build(), allowedMentions: AllowedMentions.All);
        else
            await FollowupAsync(embed: embed.Build());
    }
}