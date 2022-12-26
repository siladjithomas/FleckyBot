using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord.Rest;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Models;

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

        _logger.LogInformation($"/votes create slash command run by {Context.User}.");

        Guild? guild = null;
        SocketTextChannel? channel = null;

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            guild = context.Guilds.Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault();

            if (guild != null)
            {
                _logger.LogInformation("Guild found. Ignoring...");
            }
            else
            {
                _logger.LogWarning("Guild not found. Ignoring...");
            }

            channel = Context.Guild.GetTextChannel(Context.Channel.Id);

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

                context.Vote.Add(new Vote
                {
                    Id = context.Vote.Count() + 1,
                    MessageId = message.Id,
                    QuestionText = question
                });

                await context.SaveChangesAsync();

                await FollowupAsync($"Vote has been created in channel {channel.Mention}", allowedMentions: AllowedMentions.All);
            }
            else
            {
                await FollowupAsync("Vote could not be created. Channel was not found in Guild.");
            }
        }
    }

    [SlashCommand("setup", "Setup the channel for the guild")]
    public async Task SetupVotesChannel(SocketTextChannel channel)
    {
        await DeferAsync(ephemeral: true);

        _logger.LogInformation($"/votes setup slash command run by {Context.User}.");

        await FollowupAsync("Choosen channel for votes.");
    }
}