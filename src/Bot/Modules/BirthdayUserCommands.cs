using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TsubaHaru.FleckyBot.Bot.Services;
using TsubaHaru.FleckyBot.Database.DatabaseContexts;
using TsubaHaru.FleckyBot.Database.Models;

namespace TsubaHaru.FleckyBot.Bot.Modules;

[Group("birthday", "This group is used for bithday related things")]
public class BirthdayUserCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private readonly CommandHandler _handler;
    private readonly ILogger<BirthdayUserCommands> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public BirthdayUserCommands(InteractionService service, CommandHandler handler, ILogger<BirthdayUserCommands> logger, IServiceScopeFactory scopeFactory)
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

        user ??= Context.User;

        await using var scope = _scopeFactory.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        BirthdayUser? birthdayUser = dbContext.BirthdayUser?.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.UserId == user.Id);

        if (birthdayUser == null)
        {
            BirthdayUser newBirthdayUser = new BirthdayUser
            {
                GuildId = Context.Guild.Id,
                GuildName = Context.Guild.Name,
                UserId = user.Id,
                UserName = user.GlobalName,
                Birthday = birthday
            };

            dbContext.BirthdayUser?.Add(newBirthdayUser);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            birthdayUser.Birthday = birthday;
            dbContext.BirthdayUser?.Update(birthdayUser);
            await dbContext.SaveChangesAsync();
        }

        await FollowupAsync($"Birthday of user {user.GlobalName} has been set to {birthday}");
    }

    [SlashCommand("get", "Get the birthday of the current user or the set user")]
    public async Task GetBirthday(SocketUser? user = null)
    {
        if (Context.User.IsBot)
            return;

        await DeferAsync(ephemeral: true);

        user ??= Context.User;

        await using var scope = _scopeFactory.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        BirthdayUser? birthdayUser = dbContext.BirthdayUser?.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.UserId == user.Id);

        if (birthdayUser != null)
            await FollowupAsync($"Birthday of user {Context.User.GlobalName} is set to {birthdayUser.Birthday}");
        else
            await FollowupAsync($"Birthday of user {Context.User.GlobalName} is not set.");
    }

    [SlashCommand("getall", "Get all birthdays of the members in this guild")]
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
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
            .WithFooter(new EmbedFooterBuilder().WithText($"Command has been run by {Context.User.Username}").WithIconUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()))
            .WithCurrentTimestamp();

        if (allBirthdays != null)
            foreach (BirthdayUser bdUser in allBirthdays)
                embed.AddField(new EmbedFieldBuilder().WithIsInline(true).WithName(Context.Guild.GetUser(bdUser.UserId).Username).WithValue(bdUser.Birthday.Date));

        await FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("getallsrv", "Get all birthdays from all servers")]
    [RequireOwner]
    [RequireContext(ContextType.DM)]
    public async Task GetAllBirthdaysInDMs()
    {
        if (Context.User.IsBot)
            return;

        await DeferAsync(ephemeral: true);

        await using var scope = _scopeFactory.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        List<BirthdayUser>? allBirthdays = dbContext.BirthdayUser?.ToList();

        var embed = new EmbedBuilder()
            .WithTitle("All birthdays on all servers")
            .WithDescription("All those birthdays have been saved in this server")
            .WithFooter(new EmbedFooterBuilder().WithText($"Command has been run by {Context.User.Username}").WithIconUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()))
            .WithCurrentTimestamp();

        if (allBirthdays != null)
            foreach (BirthdayUser bdUser in allBirthdays)
                embed.AddField(new EmbedFieldBuilder().WithIsInline(true).WithName($"{bdUser.UserId} ({bdUser.GuildId})").WithValue(bdUser.Birthday.Date));

        await FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("delete", "Delete your birthday from this server")]
    public async Task DeleteBirthday(bool thisGuildOnly = true)
    {
        if (Context.User.IsBot)
            return;

        await DeferAsync(ephemeral: true);

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            List<BirthdayUser>? users = null;

            if (thisGuildOnly)
                users = context.BirthdayUser?.Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToList();
            else
                users = context.BirthdayUser?.Where(x => x.UserId == Context.User.Id).ToList();

            if (users != null)
            {
                context.BirthdayUser?.RemoveRange(users);
                context.SaveChanges();
                await FollowupAsync($"Birthday from user {Context.User.Username} has been deleted from the database.");
            }
            else
                await FollowupAsync("No Birthdays have been found in the database. Nothing to delete.");
        }        
    }
}