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
                GuildTicketsChannel? guildTicketsChannel = context.GuildTicketsChannels.Where(x => x.Id == guild.Id).FirstOrDefault();

                if (guildTicketsChannel == null)
                {
                    guild.GuildTicketsChannel = new GuildTicketsChannel
                    {
                        ChannelId = categoryChannel.Id,
                        ChannelName = categoryChannel.Name,
                        GuildTicketsGroups = new List<GuildTicketsGroup>
                        {
                            new GuildTicketsGroup
                            {
                                GroupId = adminRole.Id,
                                GroupName = adminRole.Name,
                                GroupType = "admin"
                            },
                            new GuildTicketsGroup
                            {
                                GroupId = modRole.Id,
                                GroupName = modRole.Name,
                                GroupType = "mod"
                            }
                        }
                    };

                    await context.SaveChangesAsync();
                }
                else
                {
                    guildTicketsChannel = new GuildTicketsChannel
                    {
                        Id = guildTicketsChannel.Id,
                        ChannelId = categoryChannel.Id,
                        ChannelName = categoryChannel.Name,
                        GuildTicketsGroups = new List<GuildTicketsGroup>
                        {
                            new GuildTicketsGroup
                            {
                                GroupId = adminRole.Id,
                                GroupName = adminRole.Name,
                                GroupType = "admin"
                            },
                            new GuildTicketsGroup
                            {
                                GroupId = modRole.Id,
                                GroupName = modRole.Name,
                                GroupType = "mod"
                            }
                        }
                    };

                    context.GuildTicketsChannels.Update(guildTicketsChannel);
                }
            }
            else
            {
                await FollowupAsync("Guild is not set up. Please set it up first with `/setup`!");
            }
        }
    }
}