using Bot.Models;
using Bot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bot.Modules;

[Group("tickets", "The group for all ticket related stuff")]
public class TicketCommands : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService commands { get; set; }
    private CommandHandler _handler;
    private TicketService _ticketService;
    private readonly GuildService _guildService;

    public TicketCommands(IServiceProvider service)
    {
        commands = service.GetRequiredService<InteractionService>();
        _handler = service.GetRequiredService<CommandHandler>();
        _ticketService = service.GetRequiredService<TicketService>();
        _guildService = service.GetRequiredService<GuildService>();
    }

    [SlashCommand("complain", "Use this to send a complaint to the admins of this server.")]
    public async Task CreateComplaint(string? channelName = null)
    {
        if (channelName == null)
            channelName = $"complaint-{Context.User.Username}";
        
        Log.Debug($"[TicketCommands][CreateComplaint] Ticket Channel Name: {channelName}");

        Guild guild = await _guildService.GetByGuildIdAsync(Context.Guild.Id);
        var guildCategory = Context.Guild.GetCategoryChannel(guild.GuildTicketChannel.CategoryId);
        var botsRole = Context.Guild.GetRole(943581066965426176);

        var guildTicket = await Context.Guild.CreateTextChannelAsync(channelName, x => {
            x.CategoryId = guildCategory.Id;
            x.Topic = $"This complaint ticket has been created in behalf of {Context.User.Username}";
        });

        var complaintModal = new ModalBuilder()
            .WithTitle("Complaint Form")
            .WithCustomId("complaint")
            .AddTextInput(new TextInputBuilder().WithCustomId("complaint-name").WithLabel("Name").WithRequired(true).WithStyle(TextInputStyle.Short).WithValue(Context.User.ToString()))
            .AddTextInput(new TextInputBuilder().WithCustomId("complaint-ticket-id").WithLabel("Ticket ID").WithRequired(true).WithStyle(TextInputStyle.Short).WithValue(guildTicket.Id.ToString()))
            .AddTextInput(new TextInputBuilder().WithCustomId("complaint-text").WithLabel("Complaint").WithRequired(true).WithStyle(TextInputStyle.Paragraph));
        
        await RespondWithModalAsync(complaintModal.Build());

        Log.Debug($"[TicketCommands][CreateComplaint] Ticket type: Complaint");
        Log.Debug($"[TicketCommands][CreateComplaint] Ticket channel id: {guildTicket.Id}");
        Log.Debug($"[TicketCommands][CreateComplaint] Ticket channel name: {guildTicket.Name}");

        await guildTicket.SyncPermissionsAsync();
        await guildTicket.AddPermissionOverwriteAsync(Context.User, OverwritePermissions.DenyAll(guildTicket).Modify(
            sendMessages: PermValue.Allow,
            addReactions: PermValue.Allow, 
            embedLinks: PermValue.Allow, 
            readMessageHistory: PermValue.Allow, 
            viewChannel: PermValue.Allow, 
            attachFiles: PermValue.Allow
        ));

        await _ticketService.CreateAsync(new Ticket
        {
            userId = Context.User.Id,
            userName = $"{Context.User.Username}#{Context.User.Discriminator}",
            channelId = guildTicket.Id,
            channelName = guildTicket.Name
        });
    }

    [SlashCommand("create", "Use this command to create a new ticket")]
    public async Task CreateTicket(string? channelName = null, bool isVoiceChannel = false)
    {
        await DeferAsync(ephemeral: true);

        if (channelName == null)
            channelName = $"ticket-{Context.User.Username}";
        
        Log.Debug($"[TicketCommands][CreateTicket] Ticket Channel Name: {channelName}");

        // TODO: rewrite so it uses the settings from the database
        //       now hardcoded on Flecky's discord guild
        Guild guild = await _guildService.GetByGuildIdAsync(Context.Guild.Id);
        var guildCategory = Context.Guild.GetCategoryChannel(guild.GuildTicketChannel.CategoryId); // 968992943811723355
        //var adminGroup = Context.Guild.GetRole(guild.GuildTicketChannel.AdminGroupId); // 936571989534081044
        //var moderatorGroup = Context.Guild.GetRole(guild.GuildTicketChannel.ModeratorGroupId); // 971163413088706560
        var botsRole = Context.Guild.GetRole(943581066965426176);

        if (!isVoiceChannel)
        {
            var guildTicket = await Context.Guild.CreateTextChannelAsync(channelName, x => {
                x.CategoryId = guildCategory.Id;
                x.Topic = $"This ticket has been created in behalf of {Context.User.Username}";
            });

            Log.Debug($"[TicketCommands][CreateTicket] Ticket type: Text");
            Log.Debug($"[TicketCommands][CreateTicket] Ticket channel id: {guildTicket.Id}");
            Log.Debug($"[TicketCommands][CreateTicket] Ticket channel name: {guildTicket.Name}");

            await guildTicket.SyncPermissionsAsync();
            await guildTicket.AddPermissionOverwriteAsync(Context.User, OverwritePermissions.DenyAll(guildTicket).Modify(
                sendMessages: PermValue.Allow,
                addReactions: PermValue.Allow, 
                embedLinks: PermValue.Allow, 
                readMessageHistory: PermValue.Allow, 
                viewChannel: PermValue.Allow, 
                attachFiles: PermValue.Allow
            ));

            await _ticketService.CreateAsync(new Ticket
            {
                userId = Context.User.Id,
                userName = $"{Context.User.Username}#{Context.User.Discriminator}",
                channelId = guildTicket.Id,
                channelName = guildTicket.Name
            });

            await FollowupAsync($"Ticket {guildTicket.Mention} has been created.", allowedMentions: AllowedMentions.All);

            var menu = new SelectMenuBuilder()
                .WithPlaceholder("Select an option")
                .WithCustomId("menu-ticket-category")
                .WithMinValues(1)
                .WithMaxValues(1);
            
            if (guild.GuildTicketChannel.GuildTicketGroups != null)
                foreach(GuildTicketGroup ticketGroup in guild.GuildTicketChannel.GuildTicketGroups)
                {
                    menu.AddOption($"Talk with {ticketGroup.GroupName}", $"menu-ticket-category-{ticketGroup.GroupType}", $"Choose this if you want to talk with {ticketGroup.GroupName}");
                    var groupRole = Context.Guild.GetRole(ticketGroup.GroupId);
                    await guildTicket.AddPermissionOverwriteAsync(groupRole, OverwritePermissions.DenyAll(guildTicket));
                }

            var component = new ComponentBuilder()
                .WithSelectMenu(menu);
            
            await guildTicket.SendMessageAsync("Ticket has been created! Please choose an option to get this ticket to the right group.", components: component.Build());

            Log.Information($"[TicketCommands][CreateTicket] Ticket with the name {guildTicket.Name} ({guildTicket.Id}) has been created.");
        }
        else
        {
            var guildTicket = await Context.Guild.CreateVoiceChannelAsync(channelName, x => {
                x.CategoryId = guildCategory.Id;
            });

            Log.Debug($"[TicketCommands][CreateTicket] Ticket type: Voice");
            Log.Debug($"[TicketCommands][CreateTicket] Ticket channel id: {guildTicket.Id}");
            Log.Debug($"[TicketCommands][CreateTicket] Ticket channel name: {guildTicket.Name}");

            await guildTicket.SyncPermissionsAsync();
            await guildTicket.AddPermissionOverwriteAsync(Context.User, OverwritePermissions.DenyAll(guildTicket).Modify(
                sendMessages: PermValue.Allow,
                addReactions: PermValue.Allow, 
                embedLinks: PermValue.Allow, 
                readMessageHistory: PermValue.Allow, 
                viewChannel: PermValue.Allow, 
                attachFiles: PermValue.Allow
            ));

            await _ticketService.CreateAsync(new Ticket
            {
                userId = Context.User.Id,
                userName = $"{Context.User.Username}#{Context.User.Discriminator}",
                channelId = guildTicket.Id,
                channelName = guildTicket.Name
            });

            await FollowupAsync($"Ticket {guildTicket.Mention} has been created.", allowedMentions: AllowedMentions.All);

            var menu = new SelectMenuBuilder()
                .WithPlaceholder("Select an option")
                .WithCustomId("menu-ticket-category")
                .WithMinValues(1)
                .WithMaxValues(1);

            if (guild.GuildTicketChannel.GuildTicketGroups != null)
                foreach(GuildTicketGroup ticketGroup in guild.GuildTicketChannel.GuildTicketGroups)
                {
                    menu.AddOption($"Talk with {ticketGroup.GroupName}", $"menu-ticket-category-{ticketGroup.GroupType}", $"Choose this if you want to talk with {ticketGroup.GroupName}");
                    var groupRole = Context.Guild.GetRole(ticketGroup.GroupId);
                    await guildTicket.AddPermissionOverwriteAsync(groupRole, OverwritePermissions.DenyAll(guildTicket));
                }

            var component = new ComponentBuilder()
                .WithSelectMenu(menu);
            
            await guildTicket.SendMessageAsync("Ticket has been created! Please choose an option to get this ticket to the right group.", components: component.Build());

            Log.Information($"[TicketCommands][CreateTicket] Ticket with the name {guildTicket.Name} ({guildTicket.Id}) has been created.");
        }
    }

    [SlashCommand("setup", "Use this to set up ticketing.")]
    public async Task SetupChannel(SocketCategoryChannel categoryChannel, SocketRole adminRole, SocketRole moderatorRole)
    {
        await DeferAsync(ephemeral: true);

        Guild guild = await _guildService.GetByGuildIdAsync(Context.Guild.Id);

        if (guild.Id != null)
        {
            Log.Debug($"Category: {categoryChannel.Name}, Admin Role: {adminRole.Name}, Moderator Role: {moderatorRole.Name}");
            
            guild.GuildTicketChannel.CategoryId = categoryChannel.Id;
            guild.GuildTicketChannel.CategoryName = categoryChannel.Name;

            await _guildService.UpdateAsync(guild.Id, guild);
            await FollowupAsync($"Ticket category in guild has been set to {categoryChannel.Name}, admin and moderator roles have been set up.");
        }
        else
        {
            await FollowupAsync("Guild has not been set up. Please ask the owner of the guild to set it up.");
        }
    }
}