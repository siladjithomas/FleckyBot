using System.Globalization;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using FleckyBot.Bot.Models;

namespace Bot;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<Worker> _logger;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly BotSettings _settings;

    private IServiceScope? _scope;

    public Worker(ILogger<Worker> logger, 
        IServiceProvider? provider, 
        DiscordSocketClient? client,
        CommandService? commands,
        BotSettings? settings)
    {
        _logger = logger;
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
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

            await _client.LoginAsync(TokenType.Bot, "");
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
