﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using TsubaHaru.FleckyBot.Database.DatabaseContexts;
using TsubaHaru.FleckyBot.Database.Models;
using TsubaHaru.FleckyBot.Database.Models.Guilds;
using TsubaHaru.FleckyBot.Database.Models.SleepCalls;
using TsubaHaru.FleckyBot.VRChat.Services;

namespace TsubaHaru.FleckyBot.Bot.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MailService _mailService;

    public InteractionHandler(DiscordSocketClient client, InteractionService commands, ILogger<Worker> logger,
        IServiceScopeFactory scopeFactory, MailService mailService)
    {
        _client = client;
        _commands = commands;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _mailService = mailService;

        _client.Ready += ReadyAsync;
        _client.SelectMenuExecuted += SelectMenuExectuted;
        _client.ButtonExecuted += ButtonExecuted;
        _client.MessageReceived += MessageReceived;
        _client.UserBanned += UserBanned;
        _client.UserUnbanned += UserUnbanned;
        _client.UserJoined += UserJoined;
        _client.UserLeft += UserLeft;
        _client.RoleCreated += RoleCreated;
        _client.RoleDeleted += RoleDeleted;
        _client.RoleUpdated += RoleChange;
        _client.ModalSubmitted += ModalSubmitted;
        _client.UserVoiceStateUpdated += UserVoiceStateUpdatedAsync;
    }

    private async Task ReadyAsync()
    {
        //_logger.LogInformation("In debug mode, adding commands to Guild with ID 799042503570358313...");
        // Flecky's Server
        //await _commands.RegisterCommandsToGuildAsync(799042503570358313);
        // Afterlife <3
        //await _commands.RegisterCommandsToGuildAsync(1114203792737579102);
        // Garden of Even
        //await _commands.RegisterCommandsToGuildAsync(1010646571567816734);
        // Katsch & Tratsch
        //await _commands.RegisterCommandsToGuildAsync(1195582611075113041);
        _logger.LogInformation("In production mode, adding commands globally...");
        await _commands.RegisterCommandsGloballyAsync(true);

        _logger.LogInformation($"Logged in as {_client.CurrentUser}, shard id {_client.ShardId}");

        await _client.SetActivityAsync(new Discord.Game("woof woof", ActivityType.Listening, ActivityProperties.None));
        await _client.SetStatusAsync(UserStatus.AFK);

        _logger.LogInformation($"Status of {_client.CurrentUser} on shard id {_client.ShardId} has been set properly");
    }

    private async Task UserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
    {
        _logger.LogDebug("User is entering voice...");

        if (user is SocketGuildUser guildUser)
        {
            using var scope = _scopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            if (state1.VoiceChannel != null && state2.VoiceChannel == null)
            {
                _logger.LogDebug("{user} just left voice channel {voiceChannelName}.", user.Username,
                    state1.VoiceChannel.Name);

                var sleepyCategory1 = GetSleepyCategoryIfValid(context, state1.VoiceChannel);
                _logger.LogDebug("{user} is sleepy? {isSleepy1}", guildUser.Username, sleepyCategory1);

                if (sleepyCategory1 != null)
                {
                    await CheckAndDeleteSleepyChannel(context, guildUser, state1);
                }
            }
            else if (state1.VoiceChannel == null && state2.VoiceChannel != null)
            {
                _logger.LogDebug("{user} just joined voice channel {voiceChannelName}.", user.Username,
                    state2.VoiceChannel.Name);

                var sleepyCategory2 = GetSleepyCategoryIfValid(context, state2.VoiceChannel);
                _logger.LogDebug("{user} is sleepy? {isSleepy2}", guildUser.Username, sleepyCategory2);

                if (sleepyCategory2 != null)
                {
                    await CheckAndCreateSleepyChannel(context, guildUser, state2);
                }
            }
            else if (state1.VoiceChannel != null && state2.VoiceChannel != null)
            {
                _logger.LogDebug("{user} just switched from {voiceChannelNameOld} to {voiceChannelNameNew}",
                    user.Username, state1.VoiceChannel.Name, state2.VoiceChannel.Name);

                var sleepyCategory1 = GetSleepyCategoryIfValid(context, state1.VoiceChannel);
                var sleepyCategory2 = GetSleepyCategoryIfValid(context, state2.VoiceChannel);
                _logger.LogDebug("{user} is sleepy? State 1: {isSleepy1} | State 2: {isSleepy2}", guildUser.Username,
                    sleepyCategory1, sleepyCategory2);

                if (sleepyCategory1 != null)
                    await CheckAndDeleteSleepyChannel(context, guildUser, state1);

                if (sleepyCategory2 != null)
                    await CheckAndCreateSleepyChannel(context, guildUser, state2);
            }
        }

        await Task.CompletedTask;
    }

    private async Task SelectMenuExectuted(SocketMessageComponent component)
    {
        await component.DeferAsync();

        var values = String.Join(", ", component.Data.Values);
        _logger.LogDebug("Select menu command has been executed, custom id {customId}, values {values}",
            component.Data.CustomId, values);

        switch (component.Data.CustomId)
        {
            case "role-selector-add":
                await SetGuildRoles(component);
                break;
            case "menu-ticket-category":
                await TicketSetRights(component);
                break;
            case "appointment-delete":
                await AppointmentDelete(component);
                break;
            case "appointment-approve":
                await AppointmentApprove(component);
                break;
            default:
                _logger.LogInformation("Ah snap! I can't do anything with custom id {customId}!",
                    component.Data.CustomId);
                break;
        }

        await component.FollowupAsync();
    }

    private async Task ButtonExecuted(SocketMessageComponent component)
    {
        _logger.LogDebug("Button command has been executed, custom id {customId}, value {value}",
            component.Data.CustomId, component.Data.Value);

        // regarding DMs, because there is no way to send a custom value with an custom id, at least not with a button,
        // I check if the custom ID contains a specific value and get it's channel id from it
        if (component.Data.CustomId.Contains("dm-respond-channel-"))
        {
            _logger.LogDebug("DM will be responded, please wait...");
            var words = component.Data.CustomId.Split("-");

            var channelId = words[3];
            _logger.LogDebug($"Channel ID: {channelId}");

            await ShowModalForReplyDM(component, channelId);
            return;
        }

        switch (component.Data.CustomId)
        {
            case "accept-rules":
                await component.DeferAsync(ephemeral: true);
                await RulesAcceptedSetRights(component);
                break;
            case "vote-yes":
                await component.DeferAsync(ephemeral: true);
                await AddChoiceToVote(component, true);
                break;
            case "vote-no":
                await component.DeferAsync(ephemeral: true);
                await AddChoiceToVote(component, false);
                break;
            case "vote-close":
                await component.DeferAsync(ephemeral: true);
                await CloseVote(component);
                break;
            case "close-ticket":
                await component.DeferAsync(ephemeral: true);
                await CloseTicket(component);
                break;
            case "create-ticket":
                await component.DeferAsync(ephemeral: true);
                await CreateTicketButton(component);
                break;
            case "appointment-create":
                await CreateModalForAppointment(component);
                break;
            default:
                _logger.LogInformation("Ah snap. I can't do anything with custom id {customId}!",
                    component.Data.CustomId);
                break;
        }
    }

    public async Task MessageReceived(SocketMessage message)
    {
        if (!message.Author.IsBot)
        {
            if (message.Author is SocketGuildUser guildUser)
            {
                _logger.LogDebug("User {guildUser} in guild {guildName} has sent a message.", guildUser,
                    guildUser.Guild.Name);
                _logger.LogDebug("Content of message: {messageContent}", message.Content);
                _logger.LogDebug("Count of attachments: {messagePicturesCount}", message.Attachments.Count());
            }

            if (message.Channel is SocketDMChannel)
            {
                _logger.LogInformation("Received a message from a DM!");

                var guild = _client.GetGuild(799042503570358313);
                var dmChannel = guild.GetTextChannel(1145373068462661662);

                var dmEmbed = new EmbedBuilder()
                    .WithTitle($"Received a DM message!")
                    .WithDescription(message.Content)
                    .WithCurrentTimestamp()
                    .WithFields(new EmbedFieldBuilder().WithName("Username").WithValue(message.Author.Mention)
                        .WithIsInline(true))
                    .WithFooter(new EmbedFooterBuilder().WithText("FleckyBot, your good boi!")
                        .WithIconUrl(_client.CurrentUser.GetAvatarUrl() ?? _client.CurrentUser.GetDefaultAvatarUrl()))
                    .WithColor(Discord.Color.Magenta);

                var dmComponents = new ComponentBuilder()
                    .WithButton("Respond to DM", $"dm-respond-channel-{message.Channel.Id}", ButtonStyle.Success);

                await dmChannel.SendMessageAsync(embed: dmEmbed.Build(), components: dmComponents.Build());

                return;
            }

            // Only protocol message when it is part of a ticket
            await ProtocolMessageIfTicket(message);
        }

        await Task.CompletedTask;
    }

    private async Task UserBanned(SocketUser user, SocketGuild guild)
    {
        if (guild.Id == 799042503570358313)
        {
            SocketTextChannel? logschannel = guild.GetChannel(1065032230407245905) as SocketTextChannel;

            if (logschannel != null)
                await logschannel.SendMessageAsync($"User {user.Username}#{user.Discriminator} has been banned.");

            _logger.LogInformation($"User {user.Username}#{user.Discriminator} has been banned.");
        }
    }

    private async Task UserUnbanned(SocketUser user, SocketGuild guild)
    {
        if (guild.Id == 799042503570358313)
        {
            SocketTextChannel? logschannel = guild.GetChannel(1065032230407245905) as SocketTextChannel;

            if (logschannel != null)
                await logschannel.SendMessageAsync($"User {user.Username}#{user.Discriminator} has been unbanned.");

            _logger.LogInformation($"User {user.Username}#{user.Discriminator} has been unbanned.");
        }
    }

    private async Task UserJoined(SocketGuildUser guildUser)
    {
        _logger.LogInformation("User {user} joined the guild {guildName}.", guildUser.Username, guildUser.Guild.Name);

        if (guildUser.Guild.Id == 799042503570358313)
        {
            SocketTextChannel? logsChannel = guildUser.Guild.GetChannel(1065032230407245905) as SocketTextChannel;
            SocketTextChannel? welcomeChannel = guildUser.Guild.GetChannel(946996188970905610) as SocketTextChannel;

            if (welcomeChannel != null)
            {
                var embed = new EmbedBuilder()
                    .WithTitle($"Welcome to the Server, {guildUser}!")
                    .WithDescription(
                        "Please do not forget to read and accept the rules in #rules!\n\nGodspeed, traveler!\n\nAlso, a pic from Flecky.")
                    .WithCurrentTimestamp()
                    .WithColor(Discord.Color.Magenta)
                    .WithFooter(new EmbedFooterBuilder()
                        .WithText("Executed by FleckyBot#3339")
                        .WithIconUrl(
                            "https://media.discordapp.net/attachments/974447018313408522/974447414285054032/IMG_0185.JPG?width=200&height=200"))
                    .WithThumbnailUrl(guildUser.GetAvatarUrl())
                    .WithImageUrl(
                        "https://media.discordapp.net/attachments/974447018313408522/974447414285054032/IMG_0185.JPG");

                await welcomeChannel.SendMessageAsync(embed: embed.Build());
            }

            if (logsChannel != null)
                await logsChannel.SendMessageAsync(
                    $"User {guildUser.Username}#{guildUser.Discriminator} has joined the guild {guildUser.Guild.Name}!");

            _logger.LogInformation(
                $"User {guildUser.Username}#{guildUser.Discriminator} has joined the guild {guildUser.Guild.Name}!");

            var defaultRoles = new List<ulong>
            {
                969545947799506954
            };

            await guildUser.AddRolesAsync(defaultRoles);

            _logger.LogInformation($"Added role \"Unverified\" to user {guildUser}.");
        }
        else
        {
            using var scope = _scopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?.Include(x => x.ImportantGuildRoles)
                .FirstOrDefault(g => g.GuildId == guildUser.Guild.Id);

            if (guild != null && guild.ImportantGuildRoles != null)
            {
                var guildRole = guild.ImportantGuildRoles.FirstOrDefault(x => x.RoleDescription == "unverified");

                if (guildRole != null)
                {
                    var defaultRoles = new List<ulong>
                    {
                        guildRole.RoleId
                    };

                    await guildUser.AddRolesAsync(defaultRoles);

                    _logger.LogInformation("Added role {role} to user {guildUser}.", guildRole.RoleName, guildUser);
                }
            }
        }
    }

    private async Task UserLeft(SocketGuild guild, SocketUser user)
    {
        if (guild.Id == 799042503570358313)
        {
            SocketTextChannel? logschannel = guild.GetChannel(1065032230407245905) as SocketTextChannel;
            SocketTextChannel? goodbyeChannel = guild.GetChannel(946996188970905610) as SocketTextChannel;

            if (goodbyeChannel != null)
                await goodbyeChannel.SendMessageAsync($"**{user}** just left the guild. Adieu!");

            if (logschannel != null)
                await logschannel.SendMessageAsync(
                    $"User {user.Username}#{user.Discriminator} has left the guild {guild.Name}!");

            _logger.LogInformation($"User {user.Username}#{user.Discriminator} has left the guild {guild.Name}!");
        }
    }

    private async Task RoleChange(SocketRole role1, SocketRole role2)
    {
        if (role2.Guild.Id == 799042503570358313)
        {
            SocketTextChannel? logschannel = role1.Guild.GetChannel(1065032230407245905) as SocketTextChannel;

            if (logschannel != null)
                await logschannel.SendMessageAsync(
                    $"Role {role1.Name} has been changed to {role2.Name} in guild {role1.Guild.Name}.");

            _logger.LogInformation($"Role {role1.Name} has been changed to {role2.Name} in guild {role1.Guild.Name}.");
        }
    }

    private async Task RoleCreated(SocketRole role)
    {
        if (role.Guild.Id == 799042503570358313)
        {
            SocketTextChannel? logschannel = role.Guild.GetChannel(1065032230407245905) as SocketTextChannel;

            if (logschannel != null)
                await logschannel.SendMessageAsync($"Role {role.Name} has been created in guild {role.Guild.Name}.");

            _logger.LogInformation($"Role {role.Name} has been created in guild {role.Guild.Name}.");
        }
    }

    private async Task RoleDeleted(SocketRole role)
    {
        if (role.Guild.Id == 799042503570358313)
        {
            SocketTextChannel? logschannel = role.Guild.GetChannel(1065032230407245905) as SocketTextChannel;

            if (logschannel != null)
                await logschannel.SendMessageAsync($"Role {role.Name} has been deleted in guild {role.Guild.Name}.");

            _logger.LogInformation($"Role {role.Name} has been deleted in guild {role.Guild.Name}.");
        }
    }

    private async Task ModalSubmitted(SocketModal modal)
    {
        await modal.DeferAsync(ephemeral: true);

        if (modal.Data.CustomId.Contains("dm-respond-"))
        {
            _logger.LogDebug("Received a modal to respond to a dm.");

            var words = modal.Data.CustomId.Split("-");

            if (ulong.TryParse(words[2], out var channelId))
            {
                var channel = await _client.GetDMChannelAsync(channelId);

                foreach (SocketMessageComponentData data in modal.Data.Components)
                {
                    await channel.SendMessageAsync(data.Value);

                    var dmEmbed = new EmbedBuilder()
                        .WithTitle("You have replied with:")
                        .WithDescription(data.Value)
                        .WithCurrentTimestamp()
                        .WithFooter(new EmbedFooterBuilder().WithText("FleckyBot, your good boi!")
                            .WithIconUrl(
                                _client.CurrentUser.GetAvatarUrl() ?? _client.CurrentUser.GetDefaultAvatarUrl()))
                        .WithColor(Discord.Color.Green);

                    await modal.FollowupAsync(embed: dmEmbed.Build());
                    return;
                }
            }
        }
        else if (modal.Data.CustomId == "appointment-create")
        {
            _logger.LogDebug("Received a modal to create an appointment.");

            if (modal.User is SocketGuildUser guildUser)
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                await using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                var guild = context.Guilds?
                    .Include(x => x.GuildTimetableLines)
                    .Include(x => x.GuildTimetableChannel)
                    .FirstOrDefault(x => x.GuildId == guildUser.Guild.Id);

                if (guild != null && guild.GuildTimetableChannel != null)
                {
                    foreach (SocketMessageComponentData data in modal.Data.Components)
                    {
                        var newLine = new GuildTimetableLine
                        {
                            Guild = guild,
                            RequestedTime = DateTime.Parse(data.Value),
                            RequestingUserId = guildUser.Id,
                            RequestingUserName = guildUser.Username
                        };

                        if (guild.GuildTimetableLines == null)
                            guild.GuildTimetableLines = new List<GuildTimetableLine>() { newLine };
                        else
                            guild.GuildTimetableLines.Add(newLine);

                        await context.SaveChangesAsync();

                        await modal.FollowupAsync("Der Termin wurde soeben hinzugefügt.", ephemeral: true);

                        await ResendTimetableList(guild, guildUser);

                        return;
                    }
                }
            }
        }
    }

    /* ----------------------
        private functions
    ---------------------- */

    private SocketVoiceChannel? GetSleepyCategoryIfValid(ApplicationContext context,
        SocketVoiceChannel? sleepyVoiceChannel)
    {
        if (sleepyVoiceChannel == null) return null;

        var guild = context.Guilds?
            .FirstOrDefault(x => x.GuildId == sleepyVoiceChannel.Guild.Id);

        if (guild == null) return null;

        var ignoredChannel = context.SleepCallIgnoredChannels?
            .FirstOrDefault(x => x.Guild == guild && x.ChannelId == sleepyVoiceChannel.Id);

        if (ignoredChannel != null) return null;

        var sleepCategory = context.SleepCallCategorys?
            .FirstOrDefault(x => x.Guild == guild && x.CategoryId == sleepyVoiceChannel.Category.Id);

        return sleepCategory == null ? null : sleepyVoiceChannel;
    }

    private async Task CheckAndDeleteSleepyChannel(ApplicationContext context, SocketGuildUser guildUser,
        SocketVoiceState state1)
    {
        var guild = context.Guilds?
            .FirstOrDefault(x => x.GuildId == guildUser.Guild.Id);

        if (state1.VoiceChannel.ConnectedUsers.Count == 0)
        {
            if (guild != null)
            {
                var category = context.SleepCallCategorys
                    ?.Where(x => x.Guild == guild && x.CategoryId == state1.VoiceChannel.CategoryId).FirstOrDefault();

                if (category != null)
                {
                    var channelToDelete = context.SleepCallActiveChannels.FirstOrDefault(x =>
                        x.SleepCallCategory == category && x.ChannelId == state1.VoiceChannel.Id);

                    if (channelToDelete != null)
                    {
                        await state1.VoiceChannel.DeleteAsync();

                        context.Remove(channelToDelete);

                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }

    private async Task CheckAndCreateSleepyChannel(ApplicationContext context, SocketGuildUser guildUser,
        SocketVoiceState state2)
    {
        var guild = context.Guilds?
            .FirstOrDefault(x => x.GuildId == state2.VoiceChannel.Guild.Id);

        if (guild != null)
        {
            var category = context.SleepCallCategorys
                ?.Where(x => x.Guild == guild && x.CategoryId == state2.VoiceChannel.CategoryId).FirstOrDefault();

            if (category != null)
            {
                var isInPossibleGroup = context.SleepCallGroups.FirstOrDefault(x => x.SleepCallCategory == category);

                if (isInPossibleGroup == null)
                {
                    _logger.LogWarning("{user} is not in any group.", guildUser.Username);
                    await guildUser.ModifyAsync(x => x.Channel = null);
                    return;
                }
                else if (!guildUser.Roles.Contains(state2.VoiceChannel.Guild.GetRole(isInPossibleGroup.RoleId)))
                {
                    _logger.LogWarning("{user} is not in any group.", guildUser.Username);
                    await guildUser.ModifyAsync(x => x.Channel = null);
                    return;
                }

                var voiceChannel = context.SleepCallActiveChannels.FirstOrDefault(x =>
                    x.ChannelId == state2.VoiceChannel.Id && x.SleepCallCategory == category);

                if (voiceChannel == null)
                {
                    var newSleepyChannel = await state2.VoiceChannel.Guild.CreateVoiceChannelAsync(
                        $"{guildUser.Username}'s Sleepy Channel", x => { x.CategoryId = category.CategoryId; });

                    await newSleepyChannel.SyncPermissionsAsync();
                    await newSleepyChannel.AddPermissionOverwriteAsync(guildUser,
                        OverwritePermissions.InheritAll.Modify(
                            manageChannel: PermValue.Allow
                        ));

                    var newActiveChannel = new SleepCallActiveChannel
                    {
                        ChannelId = newSleepyChannel.Id,
                        ChannelName = newSleepyChannel.Name,
                        SleepCallCategory = category
                    };

                    await context.SleepCallActiveChannels.AddAsync(newActiveChannel);

                    await context.SaveChangesAsync();

                    await guildUser.ModifyAsync(x => x.Channel = newSleepyChannel);
                }
            }
        }
    }

    private async Task RulesAcceptedSetRights(SocketMessageComponent component)
    {
        if (component.GuildId == 799042503570358313)
        {
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

            SocketGuildUser guildUser = (SocketGuildUser)component.User;
            await guildUser.RemoveRolesAsync(rulesRemove);
            await guildUser.AddRolesAsync(rulesAdd);

            _logger.LogInformation($"User {guildUser} has accepted the rules on guild.");

            await component.FollowupAsync("Rules have been accepted. Have fun on the server!", ephemeral: true);
        }
        else
        {
            using var scope = _scopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?.Include(x => x.ImportantGuildRoles)
                .FirstOrDefault(x => x.GuildId == component.GuildId);

            if (guild != null && guild.ImportantGuildRoles != null)
            {
                GuildRole unverified =
                    guild.ImportantGuildRoles.FirstOrDefault(x => x.RoleDescription == "unverified") ??
                    throw new Exception("Unverified role not found in database.");
                GuildRole verified = guild.ImportantGuildRoles.FirstOrDefault(x => x.RoleDescription == "verified") ??
                                     throw new Exception("Unverified role not found in database.");

                var rulesRemove = new List<ulong>
                {
                    unverified.RoleId
                };

                var rulesAdd = new List<ulong>
                {
                    verified.RoleId
                };

                SocketGuildUser guildUser = (SocketGuildUser)component.User;
                await guildUser.RemoveRolesAsync(rulesRemove);
                await guildUser.AddRolesAsync(rulesAdd);

                _logger.LogInformation($"User {guildUser} has accepted the rules on guild.");

                await component.FollowupAsync("Rules have been accepted. Have fun on the server!", ephemeral: true);
            }
        }
    }

    private async Task ProtocolMessageIfTicket(SocketMessage message)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            Ticket? ticket = context.Ticket?.Where(x => x.ChannelId == message.Channel.Id)
                .Include(y => y.TicketMessages).FirstOrDefault();

            if (ticket == null)
                return;
            else
            {
                _logger.LogInformation($"Received message is part of a ticket, adding to ticket id {ticket.Id}.");

                if (ticket.TicketMessages != null)
                {
                    ticket.TicketMessages.Add(new TicketMessage
                    {
                        UserId = message.Author.Id,
                        UserName = $"{message.Author.Username}#{message.Author.Discriminator}",
                        Message = message.CleanContent,
                        TimestampCreated = DateTime.Now
                    });

                    await context.SaveChangesAsync();
                }
                else
                {
                    ticket.TicketMessages = new List<TicketMessage>
                    {
                        new TicketMessage
                        {
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

            guild = context.Guilds?.Where(x => x.GuildId == component.GuildId).FirstOrDefault();
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

            Vote? vote = context.Vote?.Where(x => x.MessageId == component.Message.Id)
                .Include(y => y.VoteByUser)
                .FirstOrDefault();

            if (vote != null)
            {
                if (vote.VoteByUser != null)
                {
                    VoteUser? voteUser = vote.VoteByUser.Where(x => x.UserId == component.User.Id).FirstOrDefault();

                    if (voteUser != null)
                    {
                        _logger.LogWarning(
                            $"User {component.User} has already voted on message id {component.Message.Id}. Skipping...");
                        await component.FollowupAsync("You already voted on that one. One vote per user only.",
                            ephemeral: true);
                        return;
                    }

                    vote.VoteByUser.Add(new VoteUser
                    {
                        UserId = component.User.Id,
                        UserName = component.User.Username,
                        UserVote = choice
                    });

                    await context.SaveChangesAsync();

                    _logger.LogInformation(
                        $"Added choice {choice} to vote {vote.MessageId} for user {component.User}.");
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

                    _logger.LogInformation(
                        $"Added choice {choice} to vote {vote.MessageId} for user {component.User}.");
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
                _logger.LogInformation(
                    $"No vote with the message id {component.Message.Id} could be found. Ignoring...");
                await component.FollowupAsync(
                    $"I was not able to find a vote corresponding to that message. Something went wrong here...",
                    ephemeral: true);
            }
        }
    }

    private async Task CloseVote(SocketMessageComponent component)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            Vote? vote = context.Vote?.Where(x => x.MessageId == component.Message.Id)
                .Include(y => y.VoteByUser)
                .FirstOrDefault();

            if (vote != null)
            {
                if (vote.UserId != component.User.Id)
                {
                    await component.FollowupAsync(
                        "You did not start the vote. Only the owner of the vote can close the vote.");
                    return;
                }

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
                    .WithFooter(new EmbedFooterBuilder()
                        .WithText($"Executed by {component.User.Username}#{component.User.Discriminator}")
                        .WithIconUrl(component.User.GetAvatarUrl()));

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
                await component.FollowupAsync(
                    "Sorry but I was not able to close the ticket. I was not able to find the vote in the database.",
                    ephemeral: true);
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
            Guild? guild = context.Guilds?.Where(x => x.GuildId == component.GuildId)
                .Include(y => y.GuildTicketsChannel)
                .ThenInclude(z => z.GuildTicketsGroups)
                .FirstOrDefault();

            if (guild != null)
            {
                switch (choice)
                {
                    case "menu-ticket-category-admin":
                        if (guild.GuildTicketsChannel?.GuildTicketsGroups != null)
                            role = channel.Guild.GetRole(guild.GuildTicketsChannel.GuildTicketsGroups
                                .Where(x => x.GroupType == "admin").First().GroupId);
                        await channel.AddPermissionOverwriteAsync(role, OverwritePermissions.AllowAll(channel));
                        if (role != null)
                            mentionedRole = role.Mention;
                        break;
                    case "menu-ticket-category-mod":
                        if (guild.GuildTicketsChannel?.GuildTicketsGroups != null)
                            role = channel.Guild.GetRole(guild.GuildTicketsChannel.GuildTicketsGroups
                                .Where(x => x.GroupType == "mod").First().GroupId);
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
                    .WithDescription(
                        $"This is a ticket that only the group {mentionedRole} and you can see. Write along\n\nIf you are finished with the ticket, click the Close button.")
                    .WithColor(Discord.Color.Magenta)
                    .WithCurrentTimestamp()
                    .WithFooter(new EmbedFooterBuilder().WithText($"Executed by {component.User}")
                        .WithIconUrl(component.User.GetAvatarUrl()));

                var buttonTicket = new ComponentBuilder()
                    .WithButton("Close Ticket", "close-ticket", ButtonStyle.Danger);

                await component.ModifyOriginalResponseAsync(x =>
                {
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

                _logger.LogInformation(
                    $"Ticket with the name {ticket.ChannelName} ({ticket.ChannelId}) has been closed.");
            }
            else
            {
                _logger.LogError(
                    $"I was not able to find a ticket with the id {component.Message.Id} in the database. Aborting....");
                return;
            }
        }
    }

    private async Task CreateTicketButton(SocketMessageComponent messageComponent)
    {
        var channelName = $"ticket-{messageComponent.User.Username}";
        SocketGuildUser guildUser = messageComponent.User as SocketGuildUser ?? throw new Exception();

        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        Guild? guild = context.Guilds?.Where(x => x.GuildId == guildUser.Guild.Id)
            .Include(y => y.GuildTicketsChannel)
            .ThenInclude(z => z.GuildTicketsGroups)
            .FirstOrDefault();

        if (guild != null)
        {
            var guildTicket = await guildUser.Guild.CreateTextChannelAsync(channelName, x =>
            {
                x.CategoryId = guild.GuildTicketsChannel?.ChannelId;
                x.Topic = $"This ticket has been created in behalf of {guildUser.Username}";
            });

            await guildTicket.SyncPermissionsAsync();
            await guildTicket.AddPermissionOverwriteAsync(messageComponent.User, OverwritePermissions
                .DenyAll(guildTicket).Modify(
                    sendMessages: PermValue.Allow,
                    addReactions: PermValue.Allow,
                    embedLinks: PermValue.Allow,
                    readMessageHistory: PermValue.Allow,
                    viewChannel: PermValue.Allow,
                    attachFiles: PermValue.Allow
                ));

            context.Ticket?.Add(new Ticket
            {
                UserId = messageComponent.User.Id,
                UserName = $"{messageComponent.User}",
                ChannelId = guildTicket.Id,
                ChannelName = channelName,
                IsOpen = true,
                TimestampCreated = DateTime.Now
            });

            await context.SaveChangesAsync();

            await messageComponent.FollowupAsync($"Ticket {guildTicket.Mention} has been created.",
                allowedMentions: AllowedMentions.All, ephemeral: true);

            var menu = new SelectMenuBuilder()
                .WithPlaceholder("Select an option")
                .WithCustomId("menu-ticket-category")
                .WithMinValues(1)
                .WithMaxValues(1);

            if (guild.GuildTicketsChannel?.GuildTicketsGroups != null)
                foreach (GuildTicketsGroup group in guild.GuildTicketsChannel.GuildTicketsGroups)
                {
                    menu.AddOption($"Talk with {group.GroupName}", $"menu-ticket-category-{group.GroupType}",
                        $"Choose this if you want to talk with {group.GroupName}");
                    var groupRole = guildUser.Guild.GetRole(group.GroupId);
                    await guildTicket.AddPermissionOverwriteAsync(groupRole, OverwritePermissions.DenyAll(guildTicket));
                }

            var component = new ComponentBuilder()
                .WithSelectMenu(menu);

            await guildTicket.SendMessageAsync(
                "Ticket has been created! Please choose an option to get this ticket to the right group.",
                components: component.Build());

            _logger.LogInformation("Ticket with the name {name} ({id}) has been created.", guildTicket.Name,
                guildTicket.Id);
        }
        else
        {
            _logger.LogWarning($"Guild is not set up. Cannot create ticket. Aborting....");
            await messageComponent.FollowupAsync(
                "This guild has not been set up for the ticket system. Please use the `/setup` command to set it up.");
            return;
        }
    }

    private async Task ShowModalForReplyDM(SocketMessageComponent component, string channelId)
    {
        var dmReplyModal = new ModalBuilder()
            .WithTitle("Respond to DM")
            .WithCustomId($"dm-respond-{channelId}")
            .AddTextInput("Message", "message", TextInputStyle.Paragraph);

        await component.RespondWithModalAsync(dmReplyModal.Build());
        _logger.LogDebug("Modal sent.");
    }

    private async Task CreateModalForAppointment(SocketMessageComponent component)
    {
        var modal = new ModalBuilder()
            .WithCustomId("appointment-create")
            .WithTitle("Termin ausmachen")
            .AddTextInput("Gewünschter Termin", "appointment-create-datetime", TextInputStyle.Short,
                DateTime.Now.ToString("dd.MM.yyyy HH:mm"), required: true,
                value: DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

        await component.RespondWithModalAsync(modal.Build());
    }

    private async Task AppointmentDelete(SocketMessageComponent component)
    {
        if (component.User is SocketGuildUser guildUser)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?
                .Include(x => x.GuildTimetableLines)
                .Include(x => x.GuildTimetableChannel)
                .FirstOrDefault(x => x.GuildId == guildUser.Guild.Id);

            if (guild != null && guild.GuildTimetableLines != null && guild.GuildTimetableChannel != null)
            {
                _logger.LogDebug("Guild {guildName} found.", guild.GuildName);

                foreach (var value in component.Data.Values)
                {
                    _logger.LogDebug("Got value {rawId} from component.", value);

                    var id = int.Parse(value);

                    _logger.LogDebug("Appointment with ID {appointmentId} successfully parsed.", id);

                    var lineToDelete = context.GuildTimetableLines?.FirstOrDefault(x => x.Id == id);

                    if (lineToDelete != null)
                    {
                        _logger.LogDebug("Found line with ID {lineToDeleteId}. Time to delete!", lineToDelete.Id);

                        context.GuildTimetableLines?.Remove(lineToDelete);

                        await context.SaveChangesAsync();

                        _logger.LogInformation("Appointment with ID {id} has been deleted.", id);

                        await component.FollowupAsync("Termin wurde soeben gelöscht.", ephemeral: true);
                        await component.DeleteOriginalResponseAsync();

                        await ResendTimetableList(guild, guildUser);
                    }
                }
            }
        }
    }

    private async Task AppointmentApprove(SocketMessageComponent component)
    {
        if (component.User is SocketGuildUser guildUser)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?
                .Include(x => x.GuildTimetableLines)
                .Include(x => x.GuildTimetableChannel)
                .FirstOrDefault(x => x.GuildId == guildUser.Guild.Id);

            if (guild != null && guild.GuildTimetableLines != null && guild.GuildTimetableChannel != null)
            {
                foreach (var value in component.Data.Values)
                {
                    _logger.LogDebug("Got value {rawId} from component.", value);

                    var id = int.Parse(value);

                    _logger.LogDebug("Appointment with ID {appointmentId} successfully parsed.", id);

                    var lineToApprove = context.GuildTimetableLines?
                        .FirstOrDefault(x => x.Id == id);

                    if (lineToApprove != null)
                    {
                        _logger.LogDebug("Found line with ID {lineToDeleteId}. Time to approve!", lineToApprove.Id);

                        lineToApprove.IsApproved = true;
                        context.GuildTimetableLines?.Update(lineToApprove);

                        await context.SaveChangesAsync();

                        await component.FollowupAsync("Termin wurde soeben akzeptiert.", ephemeral: true);
                        await component.DeleteOriginalResponseAsync();

                        await ResendTimetableList(guild, guildUser);
                    }
                }
            }
        }
    }

    private async Task ResendTimetableList(Guild guild, SocketGuildUser guildUser)
    {
        if (guild.GuildTimetableChannel != null)
        {
            var embedTimetableList = new EmbedBuilder()
                .WithTitle("Derzeitige Termine")
                .WithDescription("Hier werden die derzeitigen Termine angezeigt.\n\nMögliche Termine:\n- Mo-Fr 20-23 Uhr\n- Sa-So 14-23 Uhr")
                .WithColor(Color.DarkPurple)
                .WithCurrentTimestamp();

            if (guild.GuildTimetableLines != null && guild.GuildTimetableLines.Count > 0)
            {
                foreach (var line in guild.GuildTimetableLines.FindAll(x => x.RequestedTime >= DateTime.Today && !x.IsDone))
                    if (line.RequestedTime.HasValue)
                    {
                        var accepted = line.IsApproved ? "✔" : "✖";

                        embedTimetableList.AddField(line.RequestedTime.Value.ToString("dddd, dd MMMM yyyy HH:mm"), line.RequestingUserName + $"\nAccepted: ({accepted})", true);
                    }
            }
            else
            {
                embedTimetableList.Description += "\n\n***Derzeit sind keine Termine vergeben.***";
            }

            var textChannel = guildUser.Guild.GetTextChannel(guild.GuildTimetableChannel.ChannelId);

            await textChannel.ModifyMessageAsync(guild.GuildTimetableChannel.TimetableListMessageId, x =>
            {
                x.Embed = embedTimetableList.Build();
            });
        }
    }
}
