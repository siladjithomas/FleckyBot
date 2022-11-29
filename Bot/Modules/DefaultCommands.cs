using Bot.Models;
using Bot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Bot.Modules;

public class DefaultCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private readonly GuildService _guildService;

    public DefaultCommands(IServiceProvider service)
    {
        commands = service.GetRequiredService<InteractionService>();
        _handler = service.GetRequiredService<CommandHandler>();
        _guildService = service.GetRequiredService<GuildService>();
    }

    [SlashCommand("random", "Get a random number!")]
    public async Task RandomNumber(int min = 1, int max = 100)
    {
        var random = new Random();

        var embed = new EmbedBuilder()
        {
            Title = $"Your random number: {random.Next(min, max)}",
            Description = "Type `/random [min] [max]` again for a new random number.\nIf no min and max defined then a number between 1 and 100 will be chosen.",
            Color = Color.DarkBlue,
            ThumbnailUrl = "https://image.similarpng.com/very-thumbnail/2021/05/Rolling-dice-isolated-on-transparent-background-PNG.png",
            Footer = new EmbedFooterBuilder() { Text = $"Executed by: {Context.User}", IconUrl = Context.User.GetAvatarUrl() },
            Timestamp = DateTimeOffset.Now
        };

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("8ball", "Find your answer!")]
    public async Task EightBall(string question)
    {
        await DeferAsync();
        
        var replies = new List<string>
        {
            "yes",
            "no",
        	"maybe",
            "hazzzzzy...."
        };

        var answer = replies[new Random().Next(replies.Count - 1)];

        var embed = new EmbedBuilder()
            .WithTitle("8ball")
            .WithDescription($"You asked: [**{question}**], and your answer is: [**{answer}**]")
            .WithFooter(new EmbedFooterBuilder().WithText($"Command executed by {Context.User}.").WithIconUrl(Context.User.GetAvatarUrl()))
            .WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/e/e3/8_ball_icon.svg/1200px-8_ball_icon.svg.png")
            .WithColor(Color.DarkBlue)
            .WithCurrentTimestamp();
        
        await FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("getinfo", "Get information about the guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task GetInfoGuild()
    {
        await DeferAsync(ephemeral: true);

        Guild guild = await _guildService.GetByGuildIdAsync(Context.Guild.Id);

        if (guild.Id != null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle(guild.GuildName)
                .WithDescription($"Owned by {guild.GuildAdminName}")
                .WithCurrentTimestamp()
                .WithFooter(new EmbedFooterBuilder().WithText($"Executed by {Context.User}").WithIconUrl(Context.User.GetAvatarUrl()))
                .AddField(new EmbedFieldBuilder().WithName(nameof(GuildSystemMessagesChannel)).WithValue(guild.GuildSystemMessagesChannel.ChannelName))
                .AddField(new EmbedFieldBuilder().WithName(nameof(GuildVotesChannel)).WithValue(guild.GuildVotesChannel.ChannelName))
                .AddField(new EmbedFieldBuilder().WithName(nameof(GuildTicketChannel)).WithValue($"**Category:** {guild.GuildTicketChannel.CategoryName}"));
        
            await FollowupAsync(embed: embed.Build());
        }
        else
        {
            await FollowupAsync("Guild has not been set up. Please ask the owner of the guild to set it up.");
        }
    }

    [SlashCommand("setup", "Set information about the guild")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task SetupInfoGuild()
    {
        await DeferAsync(ephemeral: true);

        Guild guild = await _guildService.GetByGuildIdAsync(Context.Guild.Id);

        if (guild.Id == null)
        {
            SocketTextChannel? channel = Context.Channel as SocketTextChannel;

            await _guildService.CreateAsync(new Guild
            {
                GuildId = Context.Guild.Id,
                GuildName = Context.Guild.Name,
                GuildAdminId = Context.Guild.OwnerId,
                GuildAdminName = Context.Guild.Owner.Nickname
            });

            SelectMenuBuilder systemMessagesChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-systemmessages")
                .WithPlaceholder("Select a channel for system messages");
            SelectMenuBuilder votesChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-votes")
                .WithPlaceholder("Select a channel for votes");
            
            SelectMenuBuilder ticketCategoryChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-tickets-category")
                .WithPlaceholder("Select a category for tickets");
            SelectMenuBuilder ticketAdminRoleChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-tickets-role-admin")
                .WithPlaceholder("Select a role for admin");
            SelectMenuBuilder ticketModeratorRoleChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-tickets-role-moderator")
                .WithPlaceholder("Select a role for moderators");
            
            foreach (SocketTextChannel textChannel in Context.Guild.TextChannels)
            {
                systemMessagesChannel.AddOption(textChannel.Name, $"{textChannel.Id}");
                votesChannel.AddOption(textChannel.Name, $"{textChannel.Id}");
            }

            foreach (SocketCategoryChannel categoryChannel in Context.Guild.CategoryChannels)
                ticketCategoryChannel.AddOption(categoryChannel.Name, $"{categoryChannel.Id}");
            
            foreach (SocketRole roles in Context.Guild.Roles)
            {
                ticketAdminRoleChannel.AddOption(roles.Name, $"{roles.Id}");
                ticketModeratorRoleChannel.AddOption(roles.Name, $"{roles.Id}");
            }

            if (channel != null)
            {
                await FollowupAsync("Guild has been set up. Please choose the channels/groups/roles that you would like to use:");
                
                await channel.SendMessageAsync("System Messages Channel:", components: new ComponentBuilder().WithSelectMenu(systemMessagesChannel).Build());
                await channel.SendMessageAsync("Votes Channel:", components: new ComponentBuilder().WithSelectMenu(votesChannel).Build());
                await channel.SendMessageAsync("Ticket Options:", components: new ComponentBuilder().WithSelectMenu(ticketCategoryChannel).WithSelectMenu(ticketAdminRoleChannel).WithSelectMenu(ticketModeratorRoleChannel).Build());
            }
            else
            {
                await FollowupAsync("Something happened here. I do not know what but yeah...");
            }
        }
        else
        {
            SocketTextChannel? channel = Context.Channel as SocketTextChannel;

            SelectMenuBuilder systemMessagesChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-systemmessages")
                .WithPlaceholder("Select a channel for system messages");
            SelectMenuBuilder votesChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-votes")
                .WithPlaceholder("Select a channel for votes");
            
            SelectMenuBuilder ticketCategoryChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-tickets-category")
                .WithPlaceholder("Select a category for tickets");
            SelectMenuBuilder ticketAdminRoleChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-tickets-role-admin")
                .WithPlaceholder("Select a role for admin");
            SelectMenuBuilder ticketModeratorRoleChannel = new SelectMenuBuilder()
                .WithCustomId("menu-setup-tickets-role-moderator")
                .WithPlaceholder("Select a role for moderators");
            
            foreach (SocketTextChannel textChannel in Context.Guild.TextChannels)
            {
                systemMessagesChannel.AddOption(textChannel.Name, $"{textChannel.Id}");
                votesChannel.AddOption(textChannel.Name, $"{textChannel.Id}");
            }

            foreach (SocketCategoryChannel categoryChannel in Context.Guild.CategoryChannels)
                ticketCategoryChannel.AddOption(categoryChannel.Name, $"{categoryChannel.Id}");
            
            foreach (SocketRole roles in Context.Guild.Roles)
            {
                ticketAdminRoleChannel.AddOption(roles.Name, $"{roles.Id}");
                ticketModeratorRoleChannel.AddOption(roles.Name, $"{roles.Id}");
            }

            if (channel != null)
            {
                await FollowupAsync("Guild has already been set up. Please choose the channels/groups/roles that you would like to use:");
                
                await channel.SendMessageAsync("System Messages Channel:", components: new ComponentBuilder().WithSelectMenu(systemMessagesChannel).Build());
                await channel.SendMessageAsync("Votes Channel:", components: new ComponentBuilder().WithSelectMenu(votesChannel).Build());
                await channel.SendMessageAsync("Ticket Options:", components: new ComponentBuilder().WithSelectMenu(ticketCategoryChannel).WithSelectMenu(ticketAdminRoleChannel).WithSelectMenu(ticketModeratorRoleChannel).Build());
            }
            else
            {
                await FollowupAsync("Something happened here. I do not know what but yeah...");
            }
        }
    }
}