using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord.Rest;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;

namespace Bot.Modules;

[Group("tickets", "Group for ticket system related stuff")]
public class TicketCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TicketCommands(InteractionService service, CommandHandler handler, ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        commands = service;
        _handler = handler;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    [SlashCommand("setup", "Setup the settings for the ticketing system")]
    public async Task SetupTicketSystem(SocketCategoryChannel categoryChannel, SocketRole adminRole, SocketRole modRole)
    {
        if (Context.Guild.OwnerId != Context.User.Id)
            return;
        
        await DeferAsync(ephemeral: true);

        _logger.LogInformation($"/ticket setup slash command run by {Context.User}.");

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            Guild? guild = context.Guilds.Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault();

            if (guild != null)
            {
                _logger.LogDebug("Guild has been found. Continuing....");

                if (guild.GuildTicketsChannel == null)
                {
                    guild.GuildTicketsChannel = new GuildTicketsChannel
                    {
                        ChannelId = categoryChannel.Id,
                        ChannelName = categoryChannel.Name
                    };

                    await context.SaveChangesAsync();

                    _logger.LogInformation("GuildTicketChannel has been updated. Continuing...");
                }
                else
                {
                    _logger.LogWarning("GuildsTicketChannel is already set up. Continuing...");
                }

                if (guild.GuildTicketsChannel.GuildTicketsGroups == null)
                {
                    guild.GuildTicketsChannel.GuildTicketsGroups = new List<GuildTicketsGroup>
                    {
                        new GuildTicketsGroup
                        {
                            Id = 1,
                            GroupId = adminRole.Id,
                            GroupName = adminRole.Name,
                            GroupType = "admin"
                        },
                        new GuildTicketsGroup
                        {
                            Id = 2,
                            GroupId = modRole.Id,
                            GroupName = modRole.Name,
                            GroupType = "mod"
                        }
                    };

                    await context.SaveChangesAsync();

                    _logger.LogInformation("Created new GuildTicketsGroups and saved it to database. Continuing...");
                }
                else
                {
                    _logger.LogInformation("GuildSystemGroups list is already existing. Continuing...");
                }

                await FollowupAsync("Set up ticketing system in database.");
            }
            else
            {
                await FollowupAsync("Guild is not set up. Please set it up first with `/setup`!");
            }
        }
    }
}