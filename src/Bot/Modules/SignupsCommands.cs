using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;

namespace Bot.Modules;

[Group("signup", "This group is used for signups")]
public class SignupsCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private readonly ApplicationContext _context;
    private readonly ILogger<Worker> _logger;

    public SignupsCommands(InteractionService service, CommandHandler handler, ApplicationContext context, ILogger<Worker> logger)
    {
        commands = service;
        _handler = handler;
        _context = context;
        _logger = logger;
    }

    [SlashCommand("test", "Test the signups here")]
    public async Task TestSignups()
    {
        await DeferAsync();

        await Task.Delay(1000);

        await FollowupAsync("It is working as exprected. No errors.");
    }
}