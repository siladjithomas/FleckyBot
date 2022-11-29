using Bot.Models;
using Bot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bot.Models;

[Group("votes", "The group for all vote related stuff")]
public class VoteCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private VoteService _voteService;
    private GuildService _guildService;

    public VoteCommands(IServiceProvider service)
    {
        commands = service.GetRequiredService<InteractionService>();
        _handler = service.GetRequiredService<CommandHandler>();
        _voteService = service.GetRequiredService<VoteService>();
        _guildService = service.GetRequiredService<GuildService>();
    }

    [SlashCommand("create", "Use this command to create a vote")]
    public async Task CreateVote(string question)
    {
        await DeferAsync(ephemeral: true);

        Log.Debug($"[VoteCommands][CreateVote] Question of vote: {question}");

        Guild guild = await _guildService.GetByGuildIdAsync(Context.Guild.Id);
        SocketTextChannel? channel = Context.Guild.GetChannel(guild.GuildVotesChannel.ChannelId) as SocketTextChannel;
        
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
            
            await _voteService.CreateAsync(new Vote
            {
                messageId = message.Id,
                userId = Context.User.Id,
                userName = $"{Context.User.Username}#{Context.User.Discriminator}",
                question = question,
                isOpen = true
            });

            await FollowupAsync($"Vote has been created in channel {channel.Mention}.", allowedMentions: AllowedMentions.All);
        }
        else
        {
            await FollowupAsync("Vote was not able to be created. Channel not found or the guild is not set up properly.", ephemeral: true);
        }
    }

    [SlashCommand("close", "Use this to close a vote.")]
    public async Task CloseVote(string messageId)
    {
        await DeferAsync();

        if(ulong.TryParse(messageId, out var ulongId))
        {
            Vote? vote = await _voteService.GetByMessageIdAsync(ulongId);

            if (vote != null)
            {
                vote.isOpen = false;
                await _voteService.UpdateAsync(vote.Id, vote);

                var message = await Context.Channel.GetMessageAsync(ulongId);
                await message.DeleteAsync();

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
                    .WithFooter(new EmbedFooterBuilder().WithText($"Executed by {Context.User.Username}#{Context.User.Discriminator}").WithIconUrl(Context.User.GetAvatarUrl()));
                
                await FollowupAsync(embed: embed.Build());
            }
            else
                await FollowupAsync("Vote with ulong string was not found. Please check!", ephemeral: true);
        }
        else
        {
            await FollowupAsync("Format of ulong string is wrong. Please check!", ephemeral: true);
        }
    }

    [SlashCommand("setup", "Use this to set up voting.")]
    public async Task SetupChannel(SocketTextChannel textChannel)
    {
        await DeferAsync(ephemeral: true);
        
        var guild = await _guildService.GetByGuildIdAsync(textChannel.Guild.Id);

        if (guild.Id != null)
        {
            guild.GuildVotesChannel.ChannelId = textChannel.Id;
            guild.GuildVotesChannel.ChannelName = textChannel.Name;

            await _guildService.UpdateAsync(guild.Id, guild);
            await FollowupAsync($"Votes channel in guild has been set to {textChannel.Mention}", allowedMentions: AllowedMentions.All);
        }
        else
        {
            await FollowupAsync("Guild has not been set up. Please ask the owner of the guild to set it up.");
        }
    }
}