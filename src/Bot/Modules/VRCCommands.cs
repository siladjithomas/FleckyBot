using Discord;
using Discord.Interactions;
using TsubaHaru.FleckyBot.Bot.Services;
using TsubaHaru.FleckyBot.VRChat.Services;

namespace TsubaHaru.FleckyBot.Bot.Modules
{
    [Group("vrc", "The group for all VRChat related stuff")]
    public class VRCCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService commands { get; set; }
        private CommandHandler _handler;
        private readonly VRChatService _vrcService;
        private readonly ILogger<VRCCommands> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public VRCCommands(InteractionService commands, CommandHandler handler, VRChatService vrcService, ILogger<VRCCommands> logger, IServiceScopeFactory scopeFactory)
        {
            this.commands = commands;
            _handler = handler;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _vrcService = vrcService;
        }

        [SlashCommand("get", "Get a VRChat user")]
        public async Task GetUser(string displayName)
        {
            await DeferAsync(ephemeral: true);

            var limitedUser = _vrcService.SearchUser(displayName);
            var realUser = _vrcService.GetUser(limitedUser["Id"]);

            var vrcEmbed = new EmbedBuilder()
                .WithTitle(realUser["DisplayName"])
                .WithDescription(realUser["Bio"])
                .WithThumbnailUrl(realUser["CurrentAvatarThumbnailImageUrl"])
                .WithImageUrl(realUser["CurrentAvatarImageUrl"])
                .WithCurrentTimestamp()
                .WithColor(Color.DarkMagenta);

            await FollowupAsync("User found.", embed: vrcEmbed.Build(), ephemeral: true);
        }
    }
}
