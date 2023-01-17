using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Models;
using Database.DatabaseContexts;
using Database.Models;
using Victoria;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Responses.Search;

namespace Bot.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public InteractionHandler(DiscordSocketClient client, InteractionService commands, ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _client = client;
        _commands = commands;
        _logger = logger;
        _scopeFactory = scopeFactory;

        _client.Ready += ReadyAsync;
        _client.SelectMenuExecuted += SelectMenuExectuted;
        _client.ButtonExecuted += ButtonExecuted;
    }

    private async Task ReadyAsync()
    {
#if DEBUG
        _logger.LogInformation("In debug mode, adding commands to Guild with ID 799042503570358313...");
        await _commands.RegisterCommandsToGuildAsync(799042503570358313);
#else
        _logger.LogInformation("In production mode, adding commands globally...");
        await _commands.RegisterCommandsGloballyAsync(true);
#endif

        _logger.LogInformation($"Logged in as {_client.CurrentUser}, shard id {_client.ShardId}");

        await _client.SetActivityAsync(new Game("slash commands", ActivityType.Listening, ActivityProperties.None));
        await _client.SetStatusAsync(UserStatus.AFK);

        _logger.LogInformation($"Status of {_client.CurrentUser} on shard id {_client.ShardId} has been set properly");
    }

    private async Task SelectMenuExectuted(SocketMessageComponent component)
    {
        await component.DeferAsync();
        
        var values = String.Join(", ", component.Data.Values);
        _logger.LogDebug($"Select menu command has been executed, custom id {component.Data.CustomId}, values {values}");

        switch (component.Data.CustomId)
        {
            case "role-selector-add":
                await SetGuildRoles(component);
                break;
            default:
                _logger.LogInformation($"Ah snap! I can't do anything with custom id {component.Data.CustomId}!");
                break;
        }

        await component.FollowupAsync();
    }

    private async Task ButtonExecuted(SocketMessageComponent component)
    {
        await component.DeferAsync();

        _logger.LogDebug($"Button command has been executed, custom id {component.Data.CustomId}, value {component.Data.Value}");

        switch (component.Data.CustomId)
        {
            case "vote-yes":
                await AddChoiceToVote(component, true);
                break;
            case "vote-no":
                await AddChoiceToVote(component, false);
                break;
            default:
                _logger.LogInformation($"Ah snap. I can't do anything with custom id {component.Data.CustomId}!");
                break;
        }

        await component.FollowupAsync();
    }

    /* ----------------------
        private functions
    ---------------------- */

    public async Task SetGuildRoles(SocketMessageComponent component)
    {
        Guild? guild = null;

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            guild = context.Guilds.Where(x => x.GuildId == component.GuildId).FirstOrDefault();
        }

        if (guild != null)
            Console.WriteLine(guild.GuildId);
        else
            Console.WriteLine("Guild not found");

        List<ulong> roleIds = new List<ulong>();

        foreach (string id in component.Data.Values)
            if (ulong.TryParse(id, out ulong ulongId))
                roleIds.Add(ulongId);
        
        if (roleIds.Count != 0)
        {
            SocketGuildUser user = (SocketGuildUser)component.User;
            await user.AddRolesAsync(roleIds);
            await component.FollowupAsync("Roles have been set.", ephemeral: true);
        }
        else
        {
            await component.FollowupAsync("There has been an error so no roles have been set.", ephemeral: true);
        }

        await Task.CompletedTask;
    }

    private async Task AddChoiceToVote(SocketMessageComponent component, bool choice)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            Vote? vote = context.Vote.Where(x => x.MessageId == component.Message.Id).FirstOrDefault();

            if (vote != null && vote.VoteByUser?.Count() >= 1)
            {
                foreach (VoteUser voteUser in vote.VoteByUser)
                {
                    _logger.LogDebug($"{voteUser.UserName} ({voteUser.UserId} found.)");
                    
                    if (voteUser.UserId == component.User.Id)
                    {
                        _logger.LogWarning($"User {voteUser.UserName} ({voteUser.UserId}) found. Skipping....");
                        return;
                    }
                }
                
                vote.VoteByUser.Add(new VoteUser
                {
                    Id = vote.VoteByUser.Count() + 1,
                    UserId = component.User.Id,
                    UserName = component.User.Username,
                    UserVote = choice,
                    VoteId = vote.Id
                });

                await context.SaveChangesAsync();
            }
            else if (vote != null && vote.VoteByUser == null)
            {
                vote.VoteByUser = new List<VoteUser>
                {
                    new VoteUser
                    {
                        Id = vote.VoteByUser.Count() + 1,
                        UserId = component.User.Id,
                        UserName = component.User.Username,
                        UserVote = choice,
                        VoteId = vote.Id
                    }
                };

                await context.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation($"No vote with the message id {component.Message.Id} could be found. Ignoring...");
            }
        }
    }
}