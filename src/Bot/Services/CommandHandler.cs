using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TsubaHaru.FleckyBot.Bot.Models;

namespace TsubaHaru.FleckyBot.Bot.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private readonly BotSettings _settings;
    private readonly ILogger<Worker> _logger;

    private IServiceScope? _scope;

    public CommandHandler(DiscordSocketClient client, 
        InteractionService commands,
        IServiceProvider services, 
        BotSettings settings, 
        ILogger<Worker> logger)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _settings = settings;
        _logger = logger;
    }

    internal void Initialize(IServiceScope scope)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));

        _client.InteractionCreated += HandleInteraction;

        _commands.SlashCommandExecuted += SlashCommandExecuted;
        _commands.ContextCommandExecuted += ContextCommandExecuted;
        _commands.ComponentCommandExecuted += ComponentCommandExecuted;
    }

    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    arg2.Interaction.RespondAsync("This message has been sent in the wrong channel. Please check again!", ephemeral: true);
                    break;
                case InteractionCommandError.UnknownCommand:
                    //arg2.Interaction.RespondAsync("This command is not known. Please check the spelling!", ephemeral: true);
                    break;
                case InteractionCommandError.BadArgs:
                    arg2.Interaction.RespondAsync("The arguments written are not compliant. Please check those again.", ephemeral: true);
                    break;
                case InteractionCommandError.Exception:
                    arg2.Interaction.RespondAsync("While I ran the command I got an exception. Please try again.", ephemeral: true);
                    break;
                case InteractionCommandError.Unsuccessful:
                    arg2.Interaction.RespondAsync("The command was not successful. Please try again.", ephemeral: true);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    arg2.Interaction.RespondAsync("This message has been sent in the wrong channel. Please check again!", ephemeral: true);
                    break;
                case InteractionCommandError.UnknownCommand:
                    arg2.Interaction.RespondAsync("This command is not known. Please check the spelling!", ephemeral: true);
                    break;
                case InteractionCommandError.BadArgs:
                    arg2.Interaction.RespondAsync("The arguments written are not compliant. Please check those again.", ephemeral: true);
                    break;
                case InteractionCommandError.Exception:
                    arg2.Interaction.RespondAsync("While I ran the command I got an exception. Please try again.", ephemeral: true);
                    break;
                case InteractionCommandError.Unsuccessful:
                    arg2.Interaction.RespondAsync("The command was not successful. Please try again.", ephemeral: true);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
    {
        if (!arg3.IsSuccess)
        {
            switch (arg3.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    arg2.Interaction.RespondAsync("Some preconditions have not been met.\nPlease check if you have run this command in the right channel and/or you have the rights to use it.", ephemeral: true);
                    _logger.LogError($"Command preconditions unmet for [{arg3.Error}] <-> [{arg3.ErrorReason}]");
                    break;
                case InteractionCommandError.UnknownCommand:
                    arg2.Interaction.RespondAsync("This command is not known. Please check the spelling!", ephemeral: true);
                    _logger.LogError($"Command unknown for [{arg3.Error}] <-> [{arg3.ErrorReason}]!");
                    break;
                case InteractionCommandError.BadArgs:
                    arg2.Interaction.RespondAsync("The arguments written are not compliant. Please check those again.", ephemeral: true);
                    _logger.LogError($"Command bad args for [{arg3.Error}] <-> [{arg3.ErrorReason}]!");
                    break;
                case InteractionCommandError.Exception:
                    arg2.Interaction.RespondAsync("While I ran the command I got an exception. Please try again.", ephemeral: true);
                    _logger.LogError($"Command failed to execute for [{arg3.Error}] <-> [{arg3.ErrorReason}]!");
                    break;
                case InteractionCommandError.Unsuccessful:
                    arg2.Interaction.RespondAsync("The command was not successful. Please try again.", ephemeral: true);
                    _logger.LogError($"Command failed to execute for [{arg3.Error}] <-> [{arg3.ErrorReason}]!");
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, arg);
            await _commands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}