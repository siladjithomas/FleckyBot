using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using TsubaHaru.FleckyBot.Bot.Services;
using TsubaHaru.FleckyBot.Database.DatabaseContexts;
using TsubaHaru.FleckyBot.Database.Models.Guilds;

namespace TsubaHaru.FleckyBot.Bot.Modules
{
    [Group("rules", "Group for roles related commands")]
    public class RuleCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private readonly CommandHandler _handler;
        private readonly ILogger<RuleCommands> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public RuleCommands(InteractionService commands, CommandHandler handler, ILogger<RuleCommands> logger, IServiceScopeFactory scopeFactory)
        {
            this.commands = commands;
            _handler = handler;
            _logger = logger;
            _scopeFactory = scopeFactory;

            _logger.LogDebug("{funcname} has been initialised.", nameof(RoleCommands));
        }

        [SlashCommand("post", "Post the rules to a channel")]
        public async Task PostRules(SocketTextChannel rulesChannel)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?.Include(x => x.GuildRules)?.Include(x => x.GuildRuleChannel)?.FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild != null && guild.GuildRules != null) 
            {
                if (guild.GuildRuleChannel == null)
                {
                    var ruleEmbed = new EmbedBuilder()
                        .WithTitle("Unsere Regeln")
                        .WithDescription("")
                        .WithColor(Color.DarkGreen);

                    ruleEmbed.Description += "**Wir bitten euch diese Regeln zu beachten:**\n\n";

                    int count = 1;

                    foreach (GuildRule rule in guild.GuildRules)
                    {
                        ruleEmbed.Description += $"{count}. {rule.RuleText}\n";

                        count++;
                    }

                    ruleEmbed.Description += "\n*Wenn du die Regeln akzeptierst dann klicke hier auf den Button.*";

                    var ruleAcceptButton = new ComponentBuilder()
                        .WithButton("Regeln akzeptieren", "accept-rules", ButtonStyle.Success);

                    var ruleMessage = await rulesChannel.SendMessageAsync(embed: ruleEmbed.Build(), components: ruleAcceptButton.Build());

                    guild.GuildRuleChannel = new GuildRuleChannel
                    {
                        Guild = guild,
                        ChannelId = ruleMessage.Channel.Id,
                        ChannelName = ruleMessage.Channel.Name,
                        MessageId = ruleMessage.Id
                    };

                    await context.SaveChangesAsync();

                    await FollowupAsync($"Added rules to channel {rulesChannel.Mention}");
                }
                else
                {
                    var ruleEmbed = new EmbedBuilder()
                        .WithTitle("Unsere Regeln")
                        .WithDescription("")
                        .WithColor(Color.DarkGreen);

                    ruleEmbed.Description += "**Wir bitten euch diese Regeln zu beachten:**\n\n";

                    int count = 1;

                    foreach (GuildRule rule in guild.GuildRules)
                    {
                        ruleEmbed.Description += $"{count}. {rule.RuleText}\n";

                        count++;
                    }

                    ruleEmbed.Description += "\n*Wenn du die Regeln akzeptierst dann klicke hier auf den Button.*";

                    var ruleAcceptButton = new ComponentBuilder()
                        .WithButton("Regeln akzeptieren", "accept-rules", ButtonStyle.Success);

                    _logger.LogDebug("Message ID: {messageId}", guild.GuildRuleChannel.MessageId);

                    await rulesChannel.ModifyMessageAsync(guild.GuildRuleChannel.MessageId, x =>
                    {
                        x.Embeds = new Embed[] { ruleEmbed.Build() };
                        x.Components = ruleAcceptButton.Build();
                    });

                    await FollowupAsync($"Updated rules to channel {rulesChannel.Mention}");
                }
            }
            else
            {
                await FollowupAsync("Guild not found in database.");
            }
        }

        [SlashCommand("add", "Add a rule to the server")]
        public async Task AddRule(string ruleText)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?.Include(x => x.GuildRules).FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild != null)
            {
                var newRule = new GuildRule
                {
                    Guild = guild,
                    RuleLanguage = "de",
                    RuleText = ruleText
                };

                if (guild.GuildRules == null)
                    guild.GuildRules = [newRule];
                else
                    guild.GuildRules.Add(newRule);

                await context.SaveChangesAsync();

                await FollowupAsync("Added rule to guild");
            }
            else
            {
                await FollowupAsync("Guild not in database");
            }
        }
    }
}
