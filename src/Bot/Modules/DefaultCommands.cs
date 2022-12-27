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

                SelectMenuBuilder systemMessagesChannel = new SelectMenuBuilder()
                    .WithCustomId("menu-setup-systemmessages")
                    .WithPlaceholder("Select a channel for system messages");
                SelectMenuBuilder votesChannel = new SelectMenuBuilder()
                    .WithCustomId("menu-setup-votes")
                    .WithPlaceholder("Select a channel for votes");

                SelectMenuBuilder ticketCategoryChannel = new SelectMenuBuilder()
                    .WithCustomId("menu-setup-tickets-category")
                    .WithPlaceholder("Select a category for tickets");
                SelectMenuBuilder ticketAdminRoleChannel = new SelectMenuBuilder()
                    .WithCustomId("menu-setup-tickets-role-admin")
                    .WithPlaceholder("Select a role for admin");
                SelectMenuBuilder ticketModeratorRoleChannel = new SelectMenuBuilder()
                    .WithCustomId("menu-setup-tickets-role-moderator")
                    .WithPlaceholder("Select a role for moderators");

                foreach (SocketTextChannel textChannel in Context.Guild.TextChannels)
                {
                    systemMessagesChannel.AddOption(textChannel.Name, $"{textChannel.Id}");
                    votesChannel.AddOption(textChannel.Name, $"{textChannel.Id}");
                }

                foreach (SocketCategoryChannel categoryChannel in Context.Guild.CategoryChannels)
                    ticketCategoryChannel.AddOption(categoryChannel.Name, $"{categoryChannel.Id}");

                foreach (SocketRole roles in Context.Guild.Roles)
                {
                    ticketAdminRoleChannel.AddOption(roles.Name, $"{roles.Id}");
                    ticketModeratorRoleChannel.AddOption(roles.Name, $"{roles.Id}");
                }

                if (channel != null)
                {
                    await FollowupAsync("Guild has been set up. Please choose the channels/groups/roles that you would like to use:");

                    await channel.SendMessageAsync("System Messages Channel:", components: new ComponentBuilder().WithSelectMenu(systemMessagesChannel).Build());
                    await channel.SendMessageAsync("Votes Channel:", components: new ComponentBuilder().WithSelectMenu(votesChannel).Build());
                    await channel.SendMessageAsync("Ticket Options:", components: new ComponentBuilder().WithSelectMenu(ticketCategoryChannel).WithSelectMenu(ticketAdminRoleChannel).WithSelectMenu(ticketModeratorRoleChannel).Build());
                }
                else
                {
                    await FollowupAsync("Something happened here. I do not know what but yeah...");
                }
            }
            else
            {
                await FollowupAsync("Guild has been already set up. Skipping...");
                return;
            }
        }
    }
}