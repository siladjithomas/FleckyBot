using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord.Rest;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Database.Models.Guilds;

namespace Bot.Modules;

[Group("votes", "The group for all vote related stuff")]
public class VoteCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public VoteCommands(InteractionService service, CommandHandler handler, ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        commands = service;
        _handler = handler;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    [SlashCommand("create", "Create a new vote either in current or in it's designated votes channel")]
    public async Task CreateVote(string question)
    {
        await DeferAsync(ephemeral: true);

        _logger.LogInformation("/votes create slash command run by {user}.", Context.User);

        Guild? guild = null;
        SocketTextChannel? channel = null;

        await using var scope = _scopeFactory.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        guild = dbContext.Guilds?
            .Include(y => y.GuildVotesChannel)
            .FirstOrDefault(x => x.GuildId == Context.Guild.Id);

        if (guild != null && guild.GuildVotesChannel != null)
        {
            _logger.LogInformation("Guild found. Using channel from database");
            channel = Context.Guild.GetTextChannel(guild.GuildVotesChannel.ChannelId);
        }
        else
        {
            _logger.LogWarning("Guild not found. Using current channel");
            channel = Context.Guild.GetTextChannel(Context.Channel.Id);
        }

        var embed = new EmbedBuilder()
            .WithTitle($"{Context.User} has a question:")
            .WithDescription($"*{question}*")
            .WithCurrentTimestamp()
            .WithFooter(new EmbedFooterBuilder().WithText($"Executed by {Context.User}").WithIconUrl(Context.User.GetAvatarUrl()));

        var buttonYes = new ButtonBuilder()
            .WithCustomId("vote-yes")
            .WithStyle(ButtonStyle.Primary)
            .WithEmote(new Emoji("✔"));

        var buttonNo = new ButtonBuilder()
            .WithCustomId("vote-no")
            .WithStyle(ButtonStyle.Primary)
            .WithEmote(new Emoji("❌"));

        var buttonClose = new ButtonBuilder()
            .WithCustomId("vote-close")
            .WithStyle(ButtonStyle.Danger)
            .WithLabel("Finish Vote");

        var buttonComponent = new ComponentBuilder()
            .WithButton(buttonYes)
            .WithButton(buttonNo)
            .WithButton(buttonClose);

        if (channel != null)
        {
            RestUserMessage message = await channel.SendMessageAsync(embed: embed.Build(), components: buttonComponent.Build());

            SocketThreadChannel threadChannel = await channel.CreateThreadAsync("Thread to message", message: message);

            dbContext.Vote?.Add(new Vote
            {
                UserId = Context.User.Id,
                UserName = $"{Context.User.Username}#{Context.User.Discriminator}",
                MessageId = message.Id,
                QuestionText = question
            });

            await dbContext.SaveChangesAsync();

            await FollowupAsync($"Vote has been created in channel {channel.Mention}", allowedMentions: AllowedMentions.All);
        }
        else
        {
            await FollowupAsync("Vote could not be created. Channel was not found in Guild.");
        }
    }

    [SlashCommand("setup", "Setup the channel for the guild")]
    public async Task SetupVotesChannel(SocketTextChannel channel)
    {
        await DeferAsync(ephemeral: true);

        _logger.LogInformation("/votes setup slash command run by {user}.", Context.User);

        using var scope = _scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        Guild? guild = dbContext.Guilds?
            .Include(x => x.GuildVotesChannel)
            .FirstOrDefault(x => x.GuildId == Context.Guild.Id);

        if (guild != null)
        {
            if (guild.GuildVotesChannel == null)
            {
                guild.GuildVotesChannel = new GuildVotesChannel
                {
                    ChannelId = channel.Id,
                    ChannelName = channel.Name
                };

                dbContext.Update(guild);

                await dbContext.SaveChangesAsync();

                await FollowupAsync($"Channel {channel.Mention} has been set as Votes channel for guild {Context.Guild.Name}.");
                _logger.LogInformation("Channel {channel} has been set as Votes channel for guild {guildName}.", channel, Context.Guild.Name);
            }
            else
            {
                guild.GuildVotesChannel.ChannelId = channel.Id;
                guild.GuildVotesChannel.ChannelName = channel.Name;

                await FollowupAsync($"Channel {channel.Mention} has been set as Votes channel for guild {Context.Guild.Name}.");
                _logger.LogInformation("Channel {channel} has been set as Votes channel for guild {guildName}.", channel, Context.Guild.Name);
            }
        }
        else
        {
            await FollowupAsync("Guild has not been set up. Please use the /setup slash command to set up this guild in the database.");
            _logger.LogWarning("Guild {guildName} has not been found in the database. Informed user to use command", Context.Guild.Name);
        }
    }
}