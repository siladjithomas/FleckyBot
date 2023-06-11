using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord.Rest;
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

    [SlashCommand("post", "Post the signups on Discord")]
    public async Task TestSignups()
    {
        await DeferAsync(ephemeral: true);

        List<Signup>? signups = _context.Signup?.Where(x => !x.isOnDiscord).ToList();
        ulong? ChannelId = _context.Guilds?.Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault()?.GuildSignupChannel?.ChannelId;

        int count = 0;

        if (signups == null || signups.Count == 0)
        {
            await FollowupAsync("No new signups found. Quitting...");
            return;
        }

        foreach (Signup signup in signups)
        {
            if (ChannelId == null)
                ChannelId = Context.Channel.Id;
            
            // TODOSIL: don't set the var yet, still in testing
            //signup.isOnDiscord = true;

            // TODOSIL: do some stuff

            count++;
        }

        //await _context.SaveChangesAsync();

        await FollowupAsync($"It is working as exprected. Found {count} signup pages in database. No errors.");
    }

    [SlashCommand("add", "Add a new signup sheet in database")]
    public async Task AddSignup(string Name, DateTime StartTime, DateTime EndTime, SocketRole AttendeeRole, string FirstCategoryName)
    {
        await DeferAsync(ephemeral: true);
        
        var NewSignup = new Signup
        {
            Name = Name,
            EventStart = StartTime,
            EventEnd = EndTime,
            isOnDiscord = false
        };

        NewSignup.SignupRestrictedTo.Add(
            new SignupAllowedGroup
            {
                GroupId = (long)AttendeeRole.Id
            }
        );

        NewSignup.SignupCategories.Add(
            new SignupCategory
            {
                Name = FirstCategoryName
            }
        );

        _context.Signup?.Add(NewSignup);

        await _context.SaveChangesAsync();

        await FollowupAsync($"Added new signup page {NewSignup.Name} to database!");
    }
}