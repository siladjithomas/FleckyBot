using System.Globalization;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Bot.Models;
using Bot.Services;
using Quartz;

namespace Bot;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<Worker> _logger;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly BotSettings _settings;
    private readonly CommandHandler _handler;
    private readonly InteractionHandler _interaction;

    private IServiceScope? _scope;

    public Worker(ILogger<Worker> logger, 
        IServiceProvider? provider, 
        DiscordSocketClient? client,
        InteractionService? commands,
        BotSettings? settings,
        InteractionHandler? interaction)
    {
        _logger = logger;
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));

        _handler = new CommandHandler(_client, _commands, _provider, _settings, _logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("de-at");
        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

        _logger.LogInformation("Setting up FleckyBot for execution...");

        IServiceScope? scope = default;
        try
        {
            _scope = _provider.CreateScope();

            _handler.Initialize(_scope);

            await _commands.AddModulesAsync(typeof(Worker).Assembly, _scope.ServiceProvider);

            await StartClientAsync(stoppingToken);
        }
        catch (Exception ex) when (!(ex is TaskCanceledException))
        {
            try
            {
                await _client.LogoutAsync();
            }
            finally
            {
                _client?.Dispose();
                scope?.Dispose();
            }

            _logger.LogError(ex, "An exception has been triggered during set-up. Shutting down...");

            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _client.StopAsync();

        return base.StopAsync(cancellationToken);
    }

    private async Task StartClientAsync(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _client.LoginAsync(TokenType.Bot, _settings.BotToken);
            await _client.StartAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not connect to Discord. Exception: {ex.Message}");
            throw;
        }
    }

    public override void Dispose()
    {
        _client.Dispose();
        _scope?.Dispose();
        ((IDisposable)_commands).Dispose();
        
        base.Dispose();
    }
}
