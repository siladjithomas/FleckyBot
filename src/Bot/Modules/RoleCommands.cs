using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using TsubaHaru.FleckyBot.Bot.Services;
using TsubaHaru.FleckyBot.Database.DatabaseContexts;
using TsubaHaru.FleckyBot.Database.Models.Guilds;

namespace TsubaHaru.FleckyBot.Bot.Modules
{
    [Group("roles", "Group for roles related commands")]
    public class RoleCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private readonly CommandHandler _handler;
        private readonly ILogger<RoleCommands> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public RoleCommands(InteractionService commands, CommandHandler handler, ILogger<RoleCommands> logger, IServiceScopeFactory scopeFactory)
        {
            this.commands = commands;
            _handler = handler;
            _logger = logger;
            _scopeFactory = scopeFactory;

            _logger.LogDebug("{funcname} has been initialised.", nameof(RoleCommands));
        }

        [SlashCommand("add", "Add a new role")]
        public async Task AddGuildRole(SocketRole guildSocketRole, [Choice("Verified", "verified"), Choice("Unverified", "unverified")] string description)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?.Include(x => x.ImportantGuildRoles).FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild != null)
            {
                var guildRole = context.ImportantGuildRoles?.FirstOrDefault(x => x.Guild == guild && x.RoleId == guildSocketRole.Id && x.RoleDescription == description);

                if (guildRole == null)
                {
                    var newGuildRole = new GuildRole
                    {
                        Guild = guild,
                        RoleId = guildSocketRole.Id,
                        RoleName = guildSocketRole.Name,
                        RoleDescription = description
                    };

                    if (guild.ImportantGuildRoles == null)
                        guild.ImportantGuildRoles = new List<GuildRole> { newGuildRole };
                    else
                        guild.ImportantGuildRoles.Add(newGuildRole);

                    await context.SaveChangesAsync();

                    await FollowupAsync($"Added role {guildSocketRole.Name} as {description} to database.");
                }
                else
                {
                    await FollowupAsync("Role already exists in database.");
                }
            }
        }

        [SlashCommand("remove", "Remove a role")]
        public async Task RemoveGuildRole(SocketRole guildSocketRole)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            var guild = context.Guilds?.Include(x => x.ImportantGuildRoles).FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild != null)
            {
                var guildRole = context.ImportantGuildRoles?.FirstOrDefault(x => x.Guild == guild && x.RoleId == guildSocketRole.Id);

                if (guildRole != null)
                {
                    context.ImportantGuildRoles?.Remove(guildRole);

                    await context.SaveChangesAsync();
                    await FollowupAsync($"Guild Role {guildSocketRole.Name} has been removed from the database.");
                }
                else
                {
                    await FollowupAsync($"Guild Role {guildSocketRole.Name} is not in the database.");
                }
            }
        }
    }
}
