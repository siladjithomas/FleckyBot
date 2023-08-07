using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;
using Serilog;

namespace Bot.Modules;

[Group("birthday", "This group is used for bithday related things")]
public class BirthdayUserCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private readonly CommandHandler _handler;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public BirthdayUserCommands(InteractionService service, CommandHandler handler, ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        commands = service;
        _handler = handler;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    [SlashCommand("set", "Set the birthday of the current user or of the set user")]
    public async Task SetBirthday(DateTime birthday, SocketUser? user = null)
    {
        if (Context.User.IsBot)
            return;

        await DeferAsync(ephemeral: true);

        string userInQuestionName = user == null ? Context.User.Username : user.Username;

        // TODO: set the birthday to the user and save it
        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            ulong userInQuestion = user == null ? Context.User.Id : user.Id;

            BirthdayUser? birthdayUser = context.BirthdayUser?.Where(x => x.GuildId == Context.Guild.Id && x.UserId == userInQuestion).FirstOrDefault();

            if (birthdayUser == null)
            {
                BirthdayUser newBirithdayUser = new BirthdayUser
                {
                    GuildId = Context.Guild.Id,
                    UserId = userInQuestion,
                    Birthday = birthday
                };

                context.BirthdayUser?.Add(newBirithdayUser);
                context.SaveChanges();
            }
            else
            {
                birthdayUser.Birthday = birthday;
                context.BirthdayUser?.Update(birthdayUser);
                context.SaveChanges();
            }
        }

        await FollowupAsync($"Birthday of user {userInQuestionName} has been set to {birthday.ToString()}");
    }

    [SlashCommand("get", "Get the birthday of the current user or the set user")]
    public async Task GetBirthday(SocketUser? user = null)
    {
        if (Context.User.IsBot)
            return;

        await DeferAsync(ephemeral: true);

        string userInQuestionName = user == null ? Context.User.Username : user.Username;

        DateTime? setBirthday = null;

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            ulong userInQuestion = user == null ? Context.User.Id : user.Id;

            BirthdayUser? birthdayUser = context.BirthdayUser?.Where(x => x.GuildId == Context.Guild.Id && x.UserId == userInQuestion).FirstOrDefault();

            if (birthdayUser != null)
                setBirthday = birthdayUser.Birthday;
        }

        if (setBirthday != null)
            await FollowupAsync($"Birthday of user {userInQuestionName} is set to {setBirthday.ToString()}");
        else
            await FollowupAsync($"Birthday of user {userInQuestionName} is not set.");
    }

    [SlashCommand("getall", "Get all birthdays of the members in this guild")]
    [RequireOwner]
    public async Task GetAllBirthdays()
    {
        if (Context.User.IsBot)
            return;

        await DeferAsync(ephemeral: true);

        List<BirthdayUser>? allBirthdays = null;

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            allBirthdays = context.BirthdayUser?.Where(x => x.GuildId == Context.Guild.Id).ToList();
        }

        var embed = new EmbedBuilder()
            .WithTitle("All birthdays in this server")
            .WithDescription("All those birthdays have been saved in this server")
            .WithFooter(new EmbedFooterBuilder().WithText($"Command has been run by {Context.User.Username}.").WithIconUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()))
            .WithCurrentTimestamp();

        if (allBirthdays != null)
            foreach (BirthdayUser bdUser in allBirthdays)
                embed.AddField(new EmbedFieldBuilder().WithIsInline(true).WithName(Context.Guild.GetUser(bdUser.UserId).Username).WithValue(bdUser.Birthday.Date));

        await FollowupAsync(embed: embed.Build());
    }
}