using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using TsubaHaru.FleckyBot.Bot.Services;
using TsubaHaru.FleckyBot.Database.DatabaseContexts;
using TsubaHaru.FleckyBot.Database.Models;
using TsubaHaru.FleckyBot.Database.Models.Guilds;

namespace TsubaHaru.FleckyBot.Bot.Modules;

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
        // if (Context.Guild.OwnerId != Context.User.Id)
        //     return;

        await DeferAsync(ephemeral: true);

        _logger.LogInformation($"/ticket setup slash command run by {Context.User}.");

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            Guild? guild = context.Guilds?.Where(x => x.GuildId == Context.Guild.Id).Include(s => s.GuildTicketsChannel).ThenInclude(z => z.GuildTicketsGroups).FirstOrDefault();

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
                    guild.GuildTicketsChannel.ChannelId = categoryChannel.Id;
                    guild.GuildTicketsChannel.ChannelName = categoryChannel.Name;

                    await context.SaveChangesAsync();

                    _logger.LogWarning("GuildsTicketChannel is already set up. Setting new values...");
                }
            }
            else
            {
                _logger.LogWarning("Guild has not been set up. User has been notified.");
                await FollowupAsync("Guild is not set up. Please set it up first with `/setup`!");
                return;
            }

            if (guild.GuildTicketsChannel != null)
            {
                if (guild.GuildTicketsChannel.GuildTicketsGroups == null)
                {
                    guild.GuildTicketsChannel.GuildTicketsGroups =
                    [
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
                        },
                    ];

                    await context.SaveChangesAsync();

                    _logger.LogInformation("Created new GuildTicketsGroups and saved it to database. Continuing...");
                }
                else
                {
                    guild.GuildTicketsChannel.GuildTicketsGroups.RemoveAll(x => x.GuildTicketsChannelId == guild.GuildTicketsChannel.Id);

                    guild.GuildTicketsChannel.GuildTicketsGroups.Add(new GuildTicketsGroup
                    {
                        GroupId = adminRole.Id,
                        GroupName = adminRole.Name,
                        GroupType = "admin"
                    });

                    guild.GuildTicketsChannel.GuildTicketsGroups.Add(new GuildTicketsGroup
                    {
                        GroupId = modRole.Id,
                        GroupName = modRole.Name,
                        GroupType = "mod"
                    });

                    await context.SaveChangesAsync();

                    _logger.LogInformation("GuildSystemGroups list is already existing. Setting new values.");
                }
            }
            else
            {
                _logger.LogWarning("Something went wrong while setting up ticket groups. Check database.");
                await FollowupAsync("Something went wrong while setting up ticket groups! Admin has been notified.");
                return;
            }
        }

        await FollowupAsync("Setup of ticketing system finished successfully.");
    }

    [SlashCommand("create", "Create a new ticket")]
    public async Task CreateTicket(string? channelName = null, bool isVoiceChannel = false)
    {
        await DeferAsync(ephemeral: true);

        if (channelName == null)
            channelName = $"ticket-{Context.User.Username}";

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            Guild? guild = context.Guilds?.Where(x => x.GuildId == Context.Guild.Id)
                .Include(y => y.GuildTicketsChannel)
                .ThenInclude(z => z.GuildTicketsGroups)
                .FirstOrDefault();
            
            if (guild != null)
                if (!isVoiceChannel)
                {
                    var guildTicket = await Context.Guild.CreateTextChannelAsync(channelName, x => {
                        x.CategoryId = guild.GuildTicketsChannel?.ChannelId;
                        x.Topic = $"This ticket has been created in behalf of {Context.User.Username}";
                    });

                    await guildTicket.SyncPermissionsAsync();
                    await guildTicket.AddPermissionOverwriteAsync(Context.User, OverwritePermissions.DenyAll(guildTicket).Modify(
                        sendMessages: PermValue.Allow,
                        addReactions: PermValue.Allow, 
                        embedLinks: PermValue.Allow, 
                        readMessageHistory: PermValue.Allow, 
                        viewChannel: PermValue.Allow, 
                        attachFiles: PermValue.Allow
                    ));

                    context.Ticket?.Add(new Ticket
                    {
                        UserId = Context.User.Id,
                        UserName = $"{Context.User}",
                        ChannelId = guildTicket.Id,
                        ChannelName = channelName,
                        IsOpen = true,
                        TimestampCreated = DateTime.Now
                    });

                    await context.SaveChangesAsync();

                    await FollowupAsync($"Ticket {guildTicket.Mention} has been created.", allowedMentions: AllowedMentions.All);

                    var menu = new SelectMenuBuilder()
                        .WithPlaceholder("Select an option")
                        .WithCustomId("menu-ticket-category")
                        .WithMinValues(1)
                        .WithMaxValues(1);
                    
                    if (guild.GuildTicketsChannel?.GuildTicketsGroups != null)
                        foreach (GuildTicketsGroup group in guild.GuildTicketsChannel.GuildTicketsGroups)
                        {
                            menu.AddOption($"Talk with {group.GroupName}", $"menu-ticket-category-{group.GroupType}", $"Choose this if you want to talk with {group.GroupName}");
                            var groupRole = Context.Guild.GetRole(group.GroupId);
                            await guildTicket.AddPermissionOverwriteAsync(groupRole, OverwritePermissions.DenyAll(guildTicket));
                        }
                    
                    var component = new ComponentBuilder()
                        .WithSelectMenu(menu);
                    
                    await guildTicket.SendMessageAsync("Ticket has been created! Please choose an option to get this ticket to the right group.", components: component.Build());

                    _logger.LogInformation("Ticket with the name {name} ({id}) has been created.", guildTicket.Name, guildTicket.Id);
                }
                else
                {
                    _logger.LogWarning("User {user} tried to create a voice channel ticket. Not implemented for now. Aborting....", Context.User);
                    await FollowupAsync("Currently not implemented. Please create a text channel for now.");
                    return;
                }
            else
            {
                _logger.LogWarning($"Guild is not set up. Cannot create ticket. Aborting....");
                await FollowupAsync("This guild has not been set up for the ticket system. Please use the `/setup` command to set it up.");
                return;
            }
        }
    }

    [SlashCommand("post", "Post the ticket button to a channel")]
    public async Task PostRules(SocketTextChannel ticketsChannel)
    {
        await DeferAsync(ephemeral: true);

        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        var guild = context.Guilds?.FirstOrDefault(x => x.GuildId == Context.Guild.Id);

        if (guild != null)
        {
            var ruleEmbed = new EmbedBuilder()
                .WithTitle("Ticket erstellen")
                .WithDescription("Klicke auf dem Button um ein Ticket zu erstellen.")
                .WithColor(Color.DarkBlue);

            var ruleAcceptButton = new ComponentBuilder()
                .WithButton("Ticket erstellen", "create-ticket", ButtonStyle.Primary);

            var ruleMessage = await ticketsChannel.SendMessageAsync(embed: ruleEmbed.Build(), components: ruleAcceptButton.Build());

            await FollowupAsync($"Added ticket creator to channel {ticketsChannel.Mention}");
        }
        else
        {
            await FollowupAsync("Guild not found in database.");
        }
    }
}