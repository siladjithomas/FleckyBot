using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Models;
using Bot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord.Rest;
using Serilog;

namespace Bot.Modules;

[Group("roles", "Use this group to set up roles in this guild.")]
public class RuleCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private GuildService _guildService;

    public RuleCommands(InteractionService commands, CommandHandler handler, GuildService guildService)
    {
        this.commands = commands;
        _handler = handler;
        _guildService = guildService;
    }

    [SlashCommand("setup", "Use this command to read all roles from the database and show them in the current channel")]
    public async Task SetupRulesInChannel()
    {
        await DeferAsync(ephemeral: true);
        
        Guild guild = await _guildService.GetByGuildIdAsync(Context.Guild.Id);

        if (Context.Channel is SocketGuildChannel channel && guild.Id != null)
        {
            var guildRolesChannel = new GuildRolesChannel
            {
                ChannelId = channel.Id,
                ChannelName = channel.Name
            };

            guild.GuildRolesChannel = guildRolesChannel;

            await _guildService.UpdateAsync(guild.Id, guild);
        }
        else
        {
            await FollowupAsync("Sadly something went wrong there. Please try again later.");
            Log.Error("Variable [channel] or id of variable [guild] was null.");
            return;
        }

        if (guild.GuildRoles != null)
            foreach (GuildRoles guildRole in guild.GuildRoles)
            {
                if (guildRole.IsEnabled)
                {
                    var guildRoleSelector = new SelectMenuBuilder()
                    .WithPlaceholder("Choose a role")
                    .WithCustomId($"role-selector-add")
                    .WithMinValues(1)
                    .WithMaxValues(1);

                    if (guildRole.SelectableRoles != null)
                        foreach (SelectableRoles role in guildRole.SelectableRoles)
                        {
                            guildRoleSelector.AddOption(role.RoleName, role.RoleId.ToString());
                        }
                    else
                    {
                        await FollowupAsync("Sadly something went wrong there. Please try again later.");
                        Log.Error("Variable [guildRole.SelectableRoles] was null.");
                        return;
                    }

                    var guildRoleComponent = new ComponentBuilder()
                        .WithSelectMenu(guildRoleSelector);

                    var guildRoleEmbed = new EmbedBuilder()
                        .WithTitle(guildRole.RoleDisplayName)
                        .WithDescription(guildRole.RoleDescription)
                        .WithCurrentTimestamp()
                        .WithFooter(new EmbedFooterBuilder().WithText("Powered by FleckyBot").WithIconUrl(Context.Client.CurrentUser.GetAvatarUrl()));

                    SocketTextChannel? rulesChannel = null;
                    
                    if (guild.GuildRolesChannel != null)
                        rulesChannel = Context.Guild.GetChannel(guild.GuildRolesChannel.ChannelId) as SocketTextChannel;
                    if (rulesChannel != null)
                        await rulesChannel.SendMessageAsync(embed: guildRoleEmbed.Build(), components: guildRoleComponent.Build());
                }
            }
        else
        {
            await FollowupAsync("Sadly something went wrong there. Please try again later.");
            Log.Error("Variable [guild.GuildRoles] was null.");
            return;
        }

        await FollowupAsync("Role selectors have been set up.", ephemeral: true);
        await Task.CompletedTask;
    }
}
