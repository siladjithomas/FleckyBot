using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using TsubaHaru.FleckyBot.Bot.Services;
using TsubaHaru.FleckyBot.Database.DatabaseContexts;
using TsubaHaru.FleckyBot.Database.Models.Guilds;
using TsubaHaru.FleckyBot.Database.Models.SleepCalls;

namespace TsubaHaru.FleckyBot.Bot.Modules
{
    [Group("sleep", "Sleep related commands")]
    public class SleepCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Service { get; set; }
        private readonly CommandHandler _handler;
        private readonly ILogger<SleepCommands> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SleepCommands(
            InteractionService service, 
            CommandHandler handler, 
            ILogger<SleepCommands> logger, 
            IServiceScopeFactory scopeFactory)
        {
            Service = service;
            _handler = handler;
            _logger = logger;
            _scopeFactory = scopeFactory;

            _logger.LogDebug("Class SleepCommands has been set up.");
        }

        [SlashCommand("addgroup", "Add a sleep group to a sleep category")]
        public async Task AddSleepGroup(SocketCategoryChannel sleepCategory, SocketRole sleepGroup)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            _logger.LogDebug("{function} created scope", nameof(AddSleepGroup));

            using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            _logger.LogDebug("{function} created application context {appContext}", nameof(AddSleepGroup), nameof(ApplicationContext));

            Guild? guild = context.Guilds?
                .Include(x => x.SleepCallCategories)!
                .ThenInclude(y => y.SleepCallGroups)
                .FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild == null)
            {
                await FollowupAsync("The guild is not in the database. Aborting...");
                return;
            }
            _logger.LogDebug("{function} found guild {guildName} ({guildId})", nameof(AddSleepGroup), guild.GuildName, guild.GuildId);

            SleepCallCategory? sleepCallCategory = guild.SleepCallCategories?.Where(x => (x.CategoryId == sleepCategory.Id || x.CategoryName == sleepCategory.Name) && x.IsDeleted == false).FirstOrDefault();

            if (sleepCallCategory == null)
            {
                await FollowupAsync("The sleep category is not in the database. Aborting...");
                return;
            }
            _logger.LogDebug("{function} found sleep category {catName} ({catId})", nameof(AddSleepGroup), sleepCallCategory.CategoryName, sleepCallCategory.CategoryId);

            SleepCallGroup? sleepCallGroup = sleepCallCategory.SleepCallGroups?.Where(x => (x.RoleId == sleepGroup.Id || x.RoleName == sleepGroup.Name) && x.IsDeleted == false).FirstOrDefault();

            if (sleepCallGroup != null)
            {
                await FollowupAsync("The sleep group is already part of this sleep category. Aborting...");
                return;
            }
            _logger.LogDebug("{function} no sleep group found in {catName}, adding group {groupName} ({groupId}) to category", nameof(AddSleepGroup), sleepCallCategory.CategoryName, sleepGroup.Name, sleepGroup.Id);

            SleepCallGroup newGroup = new SleepCallGroup
            {
                RoleId = sleepGroup.Id,
                RoleName = sleepGroup.Name
            };

            if (sleepCallCategory.SleepCallGroups == null)
                sleepCallCategory.SleepCallGroups = new List<SleepCallGroup> { newGroup };
            else
                sleepCallCategory.SleepCallGroups.Add(newGroup);

            await context.SaveChangesAsync();
            _logger.LogDebug("{function} changes has been saved", nameof(AddSleepGroup));

            await FollowupAsync($"Added group {sleepGroup} to list of possible sleep groups.");
        }

        [SlashCommand("addcategory", "Add a category to the guild")]
        public async Task AddSleepCategory(SocketCategoryChannel category)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            _logger.LogDebug("{function} created scope", nameof(AddSleepCategory));

            using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            _logger.LogDebug("{function} created application context {appContext}", nameof(AddSleepCategory), nameof(ApplicationContext));

            Guild? guild = context.Guilds?.Include(x => x.SleepCallCategories).Where(x => x.GuildId == Context.Guild.Id).FirstOrDefault();

            if (guild == null)
            {
                await FollowupAsync("The guild is not in the database. Aborting...");
                return;
            }
            _logger.LogDebug("{function} found guild {guildName} ({guildId})", nameof(AddSleepCategory), guild.GuildName, guild.GuildId);

            SleepCallCategory? sleepCallCategory = guild.SleepCallCategories?.Where(x => x.CategoryId == category.Id).FirstOrDefault();

            if (sleepCallCategory != null)
            {
                await FollowupAsync("The channel is already part of the guild. Aborting...");
                return;
            }

            SleepCallCategory newSleepCallCategory = new SleepCallCategory
            {
                CategoryId = category.Id,
                CategoryName = category.Name
            };

            if (guild.SleepCallCategories == null)
                guild.SleepCallCategories = new List<SleepCallCategory> { newSleepCallCategory };
            else
                guild.SleepCallCategories.Add(newSleepCallCategory);

            await context.SaveChangesAsync();
            _logger.LogDebug("{function} changes has been saved", nameof(AddSleepCategory));

            await FollowupAsync("Category has been added to the guild.");
        }

        [SlashCommand("ignorechannel", "Set a channel that should be ignored by the bot")]
        public async Task AddIgnoreChannel(SocketVoiceChannel ignoreChannel)
        {
            await DeferAsync(ephemeral: true);

            using var scope = _scopeFactory.CreateScope();
            _logger.LogDebug("{function} created scope", nameof(AddIgnoreChannel));

            using var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            _logger.LogDebug("{function} created application context {appContext}", nameof(AddIgnoreChannel), nameof(ApplicationContext));

            Guild? guild = context.Guilds?
                .Include(x => x.SleepCallIgnoredChannels)
                .FirstOrDefault(x => x.GuildId == Context.Guild.Id);

            if (guild == null)
            {
                await FollowupAsync("The guild is not in the database. Aborting...");
                return;
            }
            _logger.LogDebug("{function} found guild {guildName} ({guildId})", nameof(AddIgnoreChannel), guild.GuildName, guild.GuildId);

            SleepCallIgnoredChannel? ignoredChannel = context.SleepCallIgnoredChannels?.FirstOrDefault(x => x.ChannelId == ignoreChannel.Id);

            if (ignoredChannel != null)
            {
                await FollowupAsync("The channel is already ignored in this guild.");
                return;
            }

            var newIgnoredChannel = new SleepCallIgnoredChannel
            {
                ChannelId = ignoreChannel.Id,
                ChannelName = ignoreChannel.Name
            };

            if (guild.SleepCallIgnoredChannels == null)
                guild.SleepCallIgnoredChannels = new List<SleepCallIgnoredChannel> { newIgnoredChannel };
            else
                guild.SleepCallIgnoredChannels.Add(newIgnoredChannel);

            await context.SaveChangesAsync();
            _logger.LogDebug("{function} changes has been saved", nameof(AddIgnoreChannel));

            await FollowupAsync($"Channel {ignoreChannel.Name} will be now ignored.");
        }
    }
}
