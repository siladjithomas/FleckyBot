using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Models;
using Bot.Services;
using Serilog;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace Bot.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly GuildService _guildService;
    private readonly TicketService _ticketService;
    private readonly VoteService _voteService;
    private readonly LavaNode<XLavaPlayer> _lavaNode;

    public InteractionHandler(IServiceProvider services)
    {
        _client = services.GetRequiredService<DiscordSocketClient>();
        _commands = services.GetRequiredService<InteractionService>();
        _guildService = services.GetRequiredService<GuildService>();
        _ticketService = services.GetRequiredService<TicketService>();
        _voteService = services.GetRequiredService<VoteService>();
        _lavaNode = services.GetRequiredService<LavaNode<XLavaPlayer>>();

        _client.Ready += ReadyAsync;
        _client.PresenceUpdated += UserPresenceUpdated;
        _client.UserJoined += UserJoinedGuild;
        _client.UserLeft += UserLeftGuild;
        _client.JoinedGuild += BotJoinedGuild;
        _client.SelectMenuExecuted += SelectMenuExecuted;
        _client.ButtonExecuted += ButtonExecuted;
        _client.MessageReceived += MessageReceived;

        // TODO: When someone joins or gets moved to a specific channels
        //       check if the user wants to create a channel
        _client.UserVoiceStateUpdated += UserVoiceStateUpdated;

        Log.Debug("Singleton \"InteractionHandler\" has been set up.");
    }

    private async Task ReadyAsync()
    {
        if (!_lavaNode.IsConnected)
            await _lavaNode.ConnectAsync();

#if DEBUG
        Log.Information("In debug mode, adding commands to Guild with ID 799042503570358313...");
        await _commands.RegisterCommandsToGuildAsync(799042503570358313);
#else
        Log.Information("In production mode, adding commands globally...");
        await _commands.RegisterCommandsGloballyAsync(true);
#endif

        Log.Information($"Connected as {_client.CurrentUser}.");

        await _client.SetActivityAsync(new Game("slash commands", ActivityType.Listening, ActivityProperties.None));
        await _client.SetStatusAsync(UserStatus.AFK);

        Log.Information($"Status of {_client.CurrentUser} has been set properly.");
    }

    private Task UserPresenceUpdated(SocketUser user, SocketPresence arg1, SocketPresence arg2)
    {
        List<string> activities = new List<string>();
        var guildUser = user as SocketGuildUser;

        foreach (IActivity activity in arg2.Activities)
        {
            activities.Add(activity.Name);
        }

        Log.Information($"[{guildUser?.Guild.Name}] [{user.Username}#{user.DiscriminatorValue}] Presence has been changed from {arg1.Status} to {arg2.Status} ({string.Join(", ", activities)})");

        return Task.CompletedTask;
    }

    private async Task UserJoinedGuild(SocketGuildUser newUser)
    {
        var guild = await _guildService.GetByGuildIdAsync(newUser.Guild.Id);
        var channel = newUser.Guild.GetTextChannel(guild.GuildSystemMessagesChannel.ChannelId);
        await channel.SendMessageAsync($":stars: The user {newUser.Mention} has just joined the server! Say hi to him!");

        var defaultRoles = new List<ulong>
        {
            969545947799506954
        };

        Log.Information($"[InteractionHandler][UserJoinedGuild] {newUser} has joined the guild {guild.GuildName}.");

        await newUser.AddRolesAsync(defaultRoles);
        Log.Information($"[InteractionHandler][UserJoinedGuild] Applied unverified roles to user {newUser} in guild {guild.GuildName}");
    }

    private async Task UserLeftGuild(SocketGuild guild, SocketUser user)
    {
        var guildItem = await _guildService.GetByGuildIdAsync(guild.Id);
        var channel = guild.GetTextChannel(guildItem.GuildSystemMessagesChannel.ChannelId);
        await channel.SendMessageAsync($"{user} has just left the server.");

        Log.Information($"[InteractionHandler][UserLeftGuild] {user} has just left the guild {guildItem.GuildName}.");
    }

    private async Task BotJoinedGuild(IGuild guild)
    {
        Log.Information($"[InteractionHandler][BotJoinedGuild] Bot joined guild {guild.Name}.");

        var oldGuild = await _guildService.GetAsync(guild.Id.ToString());

        if (oldGuild == null)
        {
            var newGuild = new Guild
            {
                GuildId = guild.Id,
                GuildName = guild.Name,
                GuildAdminId = guild.OwnerId,
                GuildAdminName = guild.GetOwnerAsync().Result.Username
            };

            var mainChannel = await guild.GetTextChannelsAsync().ContinueWith( async (a) => 
            {
                var result = a.Result;
                var channel = result.FirstOrDefault(x => x.Name == "system-messages");

                if (channel != null)
                    await channel.SendMessageAsync("Hello lads! This is your *good boy* ~~bot~~ Flecky! Try to find my commands when you hit `/` in any channel!");
                else
                {
                    var resenum = result.GetEnumerator();
                    channel = resenum.Current;

                    await channel.SendMessageAsync("Hello lads! This is your *good boy* ~~bot~~ Flecky! Try to find my commands when you hit `/` in any channel!");

                    resenum.Dispose();
                }

                newGuild.GuildSystemMessagesChannel = new GuildSystemMessagesChannel
                {
                    ChannelId = channel.Id,
                    ChannelName = channel.Name
                };

                Log.Debug($"[InteractionHandler][BotJoinedGuild] Bot sent welcome message to channel {channel.Name} in guild {guild.Name}.");
            });

            await _guildService.CreateAsync(newGuild);
        }
    }

    public async Task SelectMenuExecuted(SocketMessageComponent component)
    {
        Log.Debug($"[InteractionHandler][SelectMenuExecuted] Select Menu has been executed. ID: {component.Id}, Custom ID: {component.Data.CustomId}");

        switch (component.Data.CustomId)
        {
            case "menu-ticket-category":
                await component.DeferAsync();
                await TicketSetRights(component);
                break;
            case "menu-setup-systemmessages":
                await component.DeferAsync();
                await SetupChannels(component);
                break;
            case "menu-setup-votes":
                await component.DeferAsync();
                await SetupChannels(component);
                break;
            case "menu-setup-tickets-category":
                break;
            case "menu-setup-tickets-role-admin":
                break;
            case "menu-setup-tickets-role-moderator":
                break;
            case "role-selector-add":
                await component.DeferAsync();
                await SetGuildRoles(component);
                break;
            default:
                Log.Debug($"Custom id {component.Data.CustomId} is currently not handled by this bot. Please try again later.");
                await component.RespondAsync("Sadly this bot does not know this command.", ephemeral: true);
                break;
        }

        await Task.CompletedTask;
    }

    public async Task ButtonExecuted(SocketMessageComponent component)
    {
        Log.Debug($"[InteractionHandler][ButtonExecuted] Button has been pressed. ID: {component.Id}, Custom ID: {component.Data.CustomId}");
        
        switch(component.Data.CustomId)
        {
            case "close-ticket":
                await component.DeferAsync();
                await CloseTicket(component);
                break;
            case "accept-rules":
                await component.DeferAsync();
                await RulesAcceptedSetRights(component);
                break;
            case "vote-yes":
                await component.DeferAsync();
                await AddChoiceToVote(component, true);
                break;
            case "vote-no":
                await component.DeferAsync();
                await AddChoiceToVote(component, false);
                break;
            case "vote-close":
                await component.DeferAsync();
                await CloseVote(component);
                break;
            default:
                await component.DeferAsync();
                await component.FollowupAsync("Sorry but I do not know what I should do with that. Ask the admin!", ephemeral: true);
                Log.Error($"[InteractionHandler][ButtonExecuted] I do not have a case for {component.Data.CustomId}! Please check the spelling!");
                break;
        }
    }

    public async Task MessageReceived(SocketMessage message)
    {
        if (!message.Author.IsBot)
        {
            // Check if this message is from a ticket
            // If true, write it to the database
            await TicketProtocolMessage(message);
        }
    }

    public async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
    {
        if (state2.VoiceChannel != null)
        {
            if (state2.VoiceChannel?.Id == 1026964208690139186)
            {
                await state2.VoiceChannel.SendMessageAsync("Testing!");

                /*
                var dmChannel = await user.CreateDMChannelAsync();

                await dmChannel.SendMessageAsync("Testing!");
                */
            }
        }

        await Task.CompletedTask;
    }

    /*
    PRIVATE FUNCTIONS
    */

    private async Task SetGuildRoles(SocketMessageComponent component)
    {
        Guild guild = await _guildService.GetByGuildIdAsync(component.GuildId);

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

    // This function is for setting up the rules on the ticket and to whom the question should go
    // TODO: This should soon be read from the database, now it is set up only for Flecky's guild
    private async Task TicketSetRights(SocketMessageComponent component)
    {
        string choise = string.Join(", ", component.Data.Values);
        string mentionedGroup = "INVALID";
        SocketRole? role = null;
        SocketGuildChannel channel = (SocketGuildChannel)await component.GetChannelAsync();
        Guild guild = await _guildService.GetByGuildIdAsync(channel.Guild.Id);

        switch(choise)
        {
            case "menu-ticket-category-admin":
                if (guild.GuildTicketChannel.GuildTicketGroups != null)
                    role = channel.Guild.GetRole(guild.GuildTicketChannel.GuildTicketGroups.Where(x => x.GroupType == "admin").First().GroupId); // 936571989534081044
                await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(channel));
                if (role != null)
                    mentionedGroup = role.Mention;
                Log.Information($"[InteractionHandler][TicketSetRights] User has choosen {choise}, setting up admin role as main contactor.");
                break;
            case "menu-ticket-category-moderator":
                if (guild.GuildTicketChannel.GuildTicketGroups != null)
                    role = channel.Guild.GetRole(guild.GuildTicketChannel.GuildTicketGroups.Where(x => x.GroupType == "mod").First().GroupId); // 971163413088706560
                await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(channel));
                if (role != null)
                    mentionedGroup = role.Mention;
                Log.Information($"[InteractionHandler][TicketSetRights] User has choosen {choise}, setting up moderator role as main contactor.");
                break;
            default:
                Log.Error($"[InteractionHandler][TicketSetRights] Something is wrong with this component, sent value {choise} is incorrect.");
                break;
        }

        var embedTicket = new EmbedBuilder()
            .WithTitle($"{component.User.Username}'s Ticket")
            .WithDescription($"This is a ticket that only the group {mentionedGroup} and you can see. Write along\n\nIf you are finished with the ticket, click the Close button.")
            .WithColor(Color.Magenta)
            .WithCurrentTimestamp()
            .WithFooter(new EmbedFooterBuilder().WithText($"Executed by {component.User}").WithIconUrl(component.User.GetAvatarUrl()));

        var buttonTicket = new ComponentBuilder()
            .WithButton("Close Ticket", "close-ticket", ButtonStyle.Danger);

        await component.ModifyOriginalResponseAsync(x =>
        {
            x.Embed = embedTicket.Build();
            x.Components = buttonTicket.Build();
            x.Content = null;
        });
    }

    // This function is for closing the ticket
    // (in this case, setting isClosed to true and deleting the channel)
    private async Task CloseTicket(SocketMessageComponent component)
    {
        Ticket? openTicket = await _ticketService.GetByChannelIdAsync(component.ChannelId);
        if (openTicket != null)
        {
            openTicket.isClosed = true;
            await _ticketService.UpdateAsync(openTicket.Id, openTicket);
            Log.Information($"[InteractionHandler][CloseTicket] Ticket with id {openTicket.Id} has been set to closed.");

            SocketTextChannel channel = (SocketTextChannel)component.Channel;
            await channel.DeleteAsync();

            Log.Information($"[InteractionHandler][CloseTicket] Channel id {component.ChannelId} has been deleted.");
        }
        else
        {
            Log.Error($"[InteractionHandler][CloseTicket] Ticket with channel id {component.ChannelId} has not been found in the database. Please delete channel manually.");
            await component.RespondAsync("Sorry but there was an error in the database. Please ask the admin to delete the channel manually.", ephemeral: true);
        }

        Log.Information($"[InteractionHandler][CloseTicket] ");
    }

    // This function is here to protocol a message in a ticket channel to the database
    private async Task TicketProtocolMessage(SocketMessage message)
    {
        Ticket? openTicket = await _ticketService.GetByChannelIdAsync(message.Channel.Id);
        if (openTicket != null)
        {
            Log.Debug($"[Interaction Handler][MessageReceived] Message with id {message.Id} is part of ticket {openTicket.channelName} ({openTicket.Id})");
            
            Message newMessage = new Message
            {
                messageId = message.Id,
                message = message.Content,
                userId = message.Author.Id,
                userName = $"{message.Author.Username}#{message.Author.Discriminator}"
            };

            openTicket.messages.Add(newMessage);

            await _ticketService.UpdateAsync(openTicket.Id, openTicket);

            Log.Debug($"[Interaction Handler][MessageReceived] Added message with id {message.Id} to ticket {openTicket.channelName} ({openTicket.Id}) in database");
        }
    }

    private async Task RulesAcceptedSetRights(SocketMessageComponent component)
    {
        // TODO: This should be actually read from the database, but currently set up for Flecky's guild
        var rulesRemove = new List<ulong>
        {
            969545947799506954
        };

        var rulesAdd = new List<ulong>
        {
            936572438140043284,
            968996826986471434,
            969008169227538462
        };

        SocketGuildUser user = (SocketGuildUser)component.User;
        await user.RemoveRolesAsync(rulesRemove);
        await user.AddRolesAsync(rulesAdd);

        Log.Information($"[Interaction Handler][RulesAcceptedSetRights] User {component.User.Username}#{component.User.Discriminator} has accepted the rules, switching roles");

        await component.FollowupAsync("Rules have been accepted. Have fun on the server!", ephemeral: true);
    }

    private async Task AddChoiceToVote(SocketMessageComponent component, bool choice)
    {
        Vote? vote = await _voteService.GetByMessageIdAsync(component.Message.Id);

        if (vote != null && vote.isOpen)
        {
            VoteByUser voteByUser = new VoteByUser
            {
                userId = component.User.Id,
                userName = $"{component.User.Username}#{component.User.Discriminator}",
                choise = choice
            };
            
            if (vote.userVotes != null)
            {
                foreach (VoteByUser userVote in vote.userVotes)
                    if (userVote.userId == component.User.Id)
                    {
                        Log.Debug($"[InteractionHandler][AddChoiceToVote] User {component.User.Username}#{component.User.Discriminator} has already voted in {vote.Id}. Discarding.");
                        await component.FollowupAsync("You already have voted here. You can only vote once.", ephemeral: true);

                        return;
                    }
                
                vote.userVotes.Add(voteByUser);
            }
            else
                vote.userVotes = new List<VoteByUser> { voteByUser };
            
            await _voteService.UpdateAsync(vote.Id, vote);
            Log.Debug($"[InteractionHandler][AddChoiceToVote] Added choice {choice} to vote id {vote.Id}");
            await component.FollowupAsync($"Thank you {component.User.Mention}! Vote has been registered.", ephemeral: true);
        }
        else
        {
            Log.Debug("[InteractionHandler][AddChoiceToVote] Something happened here. The vote does not exist in the database or is closed so doing nothing.");
            await component.DeleteOriginalResponseAsync();
            await component.FollowupAsync($"Whoa there. You tried to vote a ticket that is closed or doesn't exist.", ephemeral: true);
        }
    }

    private async Task CloseVote(SocketMessageComponent component)
    {
        Vote? vote = await _voteService.GetByMessageIdAsync(component.Message.Id);

        if (vote != null)
        {
            SocketGuildUser? user = component.User as SocketGuildUser;

            if (user != null)
                if (vote.userId != component.User.Id && user.Guild.OwnerId != component.User.Id)
                {
                    await component.FollowupAsync("You are not the creator of this vote. You cannot close it!", ephemeral: true);
                    return;
                }
            
            vote.isOpen = false;
            await _voteService.UpdateAsync(vote.Id, vote);

            await component.Message.DeleteAsync();

            int yesVotes = 0;
            int noVotes = 0;

            if (vote.userVotes != null)
                foreach (VoteByUser voteByUser in vote.userVotes)
                {
                    if (voteByUser.choise)
                        yesVotes++;
                    else
                        noVotes++;
                }

            var embed = new EmbedBuilder()
                .WithTitle("Result of votes for question:")
                .WithDescription($"\"{vote.question}\"")
                .AddField(new EmbedFieldBuilder().WithName("Yes:").WithValue(yesVotes).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("No:").WithValue(noVotes).WithIsInline(true))
                .WithCurrentTimestamp()
                .WithFooter(new EmbedFooterBuilder().WithText($"Executed by {component.User.Username}#{component.User.Discriminator}").WithIconUrl(component.User.GetAvatarUrl()));
            
            await component.FollowupAsync(embed: embed.Build());
        }
        else
            await component.FollowupAsync("Vote with ulong string was not found. Please check!", ephemeral: true);
    }

    private async Task SetupChannels(SocketMessageComponent component)
    {
        SocketGuildUser? user = component.User as SocketGuildUser;
        Guild guild = await _guildService.GetByGuildIdAsync(user?.Guild.Id);

        if (user != null && guild.Id != null)
        {
            Log.Debug("[Interaction Handler][SetupChannels] Guild and User are set up.");
            var choises = string.Join(", ", component.Data.Values);
            
            if (ulong.TryParse(choises, out ulong channelId))
            {
                Log.Debug($"[Interaction Handler][SetupChannels] Parsed ulong: {channelId}");
                
                SocketGuildChannel channel = user.Guild.GetChannel(channelId);
                
                if (component.Data.CustomId == "menu-setup-systemmessages")
                    if (guild.GuildSystemMessagesChannel != null)
                    {
                        Log.Debug("[Interaction Handler][SetupChannels] GuildSystemMessagesChannel is not null. Setting values");
                        
                        guild.GuildSystemMessagesChannel.ChannelId = channel.Id;
                        guild.GuildSystemMessagesChannel.ChannelName = channel.Name;
                    }
                    else
                    {
                        Log.Debug("[Interaction Handler][SetupChannels] GuildSystemMessagesChannel is null. Creating object and setting values");
                        
                        guild.GuildSystemMessagesChannel = new GuildSystemMessagesChannel
                        {
                            ChannelId = channel.Id,
                            ChannelName = channel.Name
                        };
                    }
                else if (component.Data.CustomId == "menu-setup-votes")
                    if (guild.GuildVotesChannel != null)
                    {
                        Log.Debug("[Interaction Handler][SetupChannels] GuildVotesChannel is not null. Setting values");
                        
                        guild.GuildVotesChannel.ChannelId = channel.Id;
                        guild.GuildVotesChannel.ChannelName = channel.Name;
                    }
                    else
                    {
                        Log.Debug("[Interaction Handler][SetupChannels] GuildVotesChannel is null. Creating object and setting values");
                        
                        guild.GuildVotesChannel = new GuildVotesChannel
                        {
                            ChannelId = channel.Id,
                            ChannelName = channel.Name
                        };
                    }

                Log.Debug("[Interaction Handler][SetupChannels] Updating guild");
                await _guildService.UpdateAsync(guild.Id, guild);
                await component.FollowupAsync("System Messages Channel has been set up.", ephemeral: true);

                Log.Debug("[Interaction Handler][SetupChannels] Done");
            }
        }
        else
        {
            await component.FollowupAsync("Something went wrong here. Couldn't get user and guild.");
        }
    }
}