using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Models;
using Database.DatabaseContexts;
using Database.Models;
using Victoria;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Responses.Search;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Bot.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public InteractionHandler(DiscordSocketClient client, InteractionService commands, ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _client = client;
        _commands = commands;
        _logger = logger;
        _scopeFactory = scopeFactory;

        _client.Ready += ReadyAsync;
        _client.SelectMenuExecuted += SelectMenuExectuted;
        _client.ButtonExecuted += ButtonExecuted;
        _client.MessageReceived += MessageReceived;
    }

    private async Task ReadyAsync()
    {
#if DEBUG
        _logger.LogInformation("In debug mode, adding commands to Guild with ID 799042503570358313...");
        await _commands.RegisterCommandsToGuildAsync(799042503570358313);
#else
        _logger.LogInformation("In production mode, adding commands globally...");
        await _commands.RegisterCommandsGloballyAsync(true);
#endif

        _logger.LogInformation($"Logged in as {_client.CurrentUser}, shard id {_client.ShardId}");

        await _client.SetActivityAsync(new Game("slash commands", ActivityType.Listening, ActivityProperties.None));
        await _client.SetStatusAsync(UserStatus.AFK);

        _logger.LogInformation($"Status of {_client.CurrentUser} on shard id {_client.ShardId} has been set properly");
    }

    private async Task SelectMenuExectuted(SocketMessageComponent component)
    {
        await component.DeferAsync();

        var values = String.Join(", ", component.Data.Values);
        _logger.LogDebug($"Select menu command has been executed, custom id {component.Data.CustomId}, values {values}");

        switch (component.Data.CustomId)
        {
            case "role-selector-add":
                await SetGuildRoles(component);
                break;
            case "menu-ticket-category":
                await TicketSetRights(component);
                break;
            default:
                _logger.LogInformation($"Ah snap! I can't do anything with custom id {component.Data.CustomId}!");
                break;
        }

        await component.FollowupAsync();
    }

    private async Task ButtonExecuted(SocketMessageComponent component)
    {
        await component.DeferAsync(ephemeral: true);

        _logger.LogDebug($"Button command has been executed, custom id {component.Data.CustomId}, value {component.Data.Value}");

        switch (component.Data.CustomId)
        {
            case "vote-yes":
                await AddChoiceToVote(component, true);
                break;
            case "vote-no":
                await AddChoiceToVote(component, false);
                break;
            case "vote-close":
                await CloseVote(component);
                break;
            case "close-ticket":
                await CloseTicket(component);
                break;
            default:
                _logger.LogInformation($"Ah snap. I can't do anything with custom id {component.Data.CustomId}!");
                break;
        }

        await component.FollowupAsync();
    }

    public async Task MessageReceived(SocketMessage message)
    {
        if (!message.Author.IsBot)
        {
            // Only protocol message when it is part of a ticket
            await ProtocolMessageIfTicket(message);
        }

        await Task.CompletedTask;
    }

    /* ----------------------
        private functions
    ---------------------- */

    private async Task ProtocolMessageIfTicket(SocketMessage message)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            Ticket? ticket = context.Ticket.Where(x => x.ChannelId == message.Channel.Id).Include(y => y.TicketMessages).FirstOrDefault();

            if (ticket == null)
                return;
            else
            {
                _logger.LogInformation($"Received message is part of a ticket, adding to ticket id {ticket.Id}.");

                if (ticket.TicketMessages != null)
                {
                    ticket.TicketMessages.Add(new TicketMessage {
                        UserId = message.Author.Id,
                        UserName = $"{message.Author.Username}#{message.Author.Discriminator}",
                        Message = message.CleanContent,
                        TimestampCreated = DateTime.Now
                    });

                    await context.SaveChangesAsync();
                }
                else
                {
                    ticket.TicketMessages = new List<TicketMessage>{
                        new TicketMessage {
                            UserId = message.Author.Id,
                            UserName = $"{message.Author.Username}#{message.Author.Discriminator}",
                            Message = message.CleanContent,
                            TimestampCreated = DateTime.Now
                        }
                    };

                    await context.SaveChangesAsync();
                }
            }
        }
    }

    public async Task SetGuildRoles(SocketMessageComponent component)
    {
        Guild? guild = null;

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            guild = context.Guilds.Where(x => x.GuildId == component.GuildId).FirstOrDefault();
        }

        if (guild != null)
            Console.WriteLine(guild.GuildId);
        else
            Console.WriteLine("Guild not found");

        List<ulong> roleIds = new List<ulong>();

        foreach (string id in component.Data.Values)
            if (ulong.TryParse(id, out ulong ulongId))
                roleIds.Add(ulongId);

        if (roleIds.Count != 0)
        {
            SocketGuildUser user = (SocketGuildUser)component.User;
            await user.AddRolesAsync(roleIds);
            await component.FollowupAsync("Roles have been set.", ephemeral: true);
        }
        else
        {
            await component.FollowupAsync("There has been an error so no roles have been set.", ephemeral: true);
        }

        await Task.CompletedTask;
    }

    private async Task AddChoiceToVote(SocketMessageComponent component, bool choice)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            Vote? vote = context.Vote.Where(x => x.MessageId == component.Message.Id)
                .Include(y => y.VoteByUser)
                .FirstOrDefault();

            if (vote != null)
            {
                if (vote.VoteByUser != null)
                {
                    VoteUser? voteUser = vote.VoteByUser.Where(x => x.UserId == component.User.Id).FirstOrDefault();

                    if (voteUser != null)
                    {
                        _logger.LogWarning($"User {component.User} has already voted on message id {component.Message.Id}. Skipping...");
                        await component.FollowupAsync("You already voted on that one. One vote per user only.", ephemeral: true);
                        return;
                    }

                    vote.VoteByUser.Add(new VoteUser
                    {
                        UserId = component.User.Id,
                        UserName = component.User.Username,
                        UserVote = choice
                    });

                    await context.SaveChangesAsync();

                    _logger.LogInformation($"Added choice {choice} to vote {vote.MessageId} for user {component.User}.");
                    await component.FollowupAsync("Added choice to vote.", ephemeral: true);
                }
                else if (vote.VoteByUser == null)
                {
                    vote.VoteByUser = new List<VoteUser>
                    {
                        new VoteUser
                        {
                            UserId = component.User.Id,
                            UserName = component.User.Username,
                            UserVote = choice
                        }
                    };

                    await context.SaveChangesAsync();

                    _logger.LogInformation($"Added choice {choice} to vote {vote.MessageId} for user {component.User}.");
                    await component.FollowupAsync("Added choice to vote.", ephemeral: true);
                }
                else
                {
                    _logger.LogInformation($"Vote was found but was not able to do anything. Skipping....");
                    await component.FollowupAsync($"Found the vote, was not able to add vote tho...", ephemeral: true);
                }
            }
            else
            {
                _logger.LogInformation($"No vote with the message id {component.Message.Id} could be found. Ignoring...");
                await component.FollowupAsync($"I was not able to find a vote corresponding to that message. Something went wrong here...", ephemeral: true);
            }
        }
    }

    private async Task CloseVote(SocketMessageComponent component)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            Vote? vote = context.Vote.Where(x => x.MessageId == component.Message.Id)
                .Include(y => y.VoteByUser)
                .FirstOrDefault();

            if (vote != null)
            {
                vote.isOpen = false;

                await context.SaveChangesAsync();

                int yesVotes = 0;
                int noVotes = 0;

                if (vote.VoteByUser != null)
                    foreach (VoteUser voteUser in vote.VoteByUser)
                        if (voteUser.UserVote == true)
                            yesVotes++;
                        else
                            noVotes++;

                var embed = new EmbedBuilder()
                    .WithTitle("Result of votes for question:")
                    .WithDescription($"\"{vote.QuestionText}\"")
                    .AddField(new EmbedFieldBuilder().WithName("Yes:").WithValue(yesVotes).WithIsInline(true))
                    .AddField(new EmbedFieldBuilder().WithName("No:").WithValue(noVotes).WithIsInline(true))
                    .WithCurrentTimestamp()
                    .WithFooter(new EmbedFooterBuilder().WithText($"Executed by {component.User.Username}#{component.User.Discriminator}").WithIconUrl(component.User.GetAvatarUrl()));

                await component.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed.Build();
                    x.Components = new ComponentBuilder().Build();
                });

                await component.FollowupAsync();
            }
            else
            {
                _logger.LogError($"There is no vote with message id {component.Message.Id}! Aborting...");
                await component.FollowupAsync("Sorry but I was not able to close the ticket. I was not able to find the vote in the database.", ephemeral: true);
                return;
            }
        }
    }

    private async Task TicketSetRights(SocketMessageComponent component)
    {
        var choice = string.Join(", ", component.Data.Values);
        string mentionedRole = "NULL";
        SocketRole? role = null;
        SocketGuildChannel channel = (SocketGuildChannel)await component.GetChannelAsync();

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            Guild? guild = context.Guilds.Where(x => x.GuildId == component.GuildId)
                .Include(y => y.GuildTicketsChannel)
                .ThenInclude(z => z.GuildTicketsGroups)
                .FirstOrDefault();

            if (guild != null)
            {
                switch (choice)
                {
                    case "menu-ticket-category-admin":
                        if (guild.GuildTicketsChannel?.GuildTicketsGroups != null)
                            role = channel.Guild.GetRole(guild.GuildTicketsChannel.GuildTicketsGroups.Where(x => x.GroupType == "admin").First().GroupId);
                        await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(channel));
                        if (role != null)
                            mentionedRole = role.Mention;
                        break;
                    case "menu-ticket-category-mod":
                        if (guild.GuildTicketsChannel?.GuildTicketsGroups != null)
                            role = channel.Guild.GetRole(guild.GuildTicketsChannel.GuildTicketsGroups.Where(x => x.GroupType == "mod").First().GroupId);
                        await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(channel));
                        if (role != null)
                            mentionedRole = role.Mention;
                        break;
                    default:
                        _logger.LogError($"{choice} is not a valid selection. Aborting...");
                        break;
                }

                var embedTicket = new EmbedBuilder()
                    .WithTitle($"{component.User.Username}'s Ticket")
                    .WithDescription($"This is a ticket that only the group {mentionedRole} and you can see. Write along\n\nIf you are finished with the ticket, click the Close button.")
                    .WithColor(Color.Magenta)
                    .WithCurrentTimestamp()
                    .WithFooter(new EmbedFooterBuilder().WithText($"Executed by {component.User}").WithIconUrl(component.User.GetAvatarUrl()));

                var buttonTicket = new ComponentBuilder()
                    .WithButton("Close Ticket", "close-ticket", ButtonStyle.Danger);

                await component.ModifyOriginalResponseAsync(x => {
                    x.Embed = embedTicket.Build();
                    x.Components = buttonTicket.Build();
                    x.Content = "";
                });
            }
            else
            {
                _logger.LogWarning("Guild not found. Was not able to get required info for ticket to set admin.");
                return;
            }
        }
    }

    private async Task CloseTicket(SocketMessageComponent component)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            Ticket? ticket = context.Ticket.Where(x => x.ChannelId == component.Channel.Id)
                .Include(y => y.TicketMessages)
                .FirstOrDefault();
            
            if (ticket != null)
            {
                ticket.IsOpen = false;

                await context.SaveChangesAsync();

                SocketTextChannel channel = (SocketTextChannel)component.Channel;
                await channel.DeleteAsync();

                _logger.LogInformation($"Ticket with the name {ticket.ChannelName} ({ticket.ChannelId}) has been closed.");
            }
            else
            {
                _logger.LogError($"I was not able to find a ticket with the id {component.Message.Id} in the database. Aborting....");
                return;
            }
        }
    }
}