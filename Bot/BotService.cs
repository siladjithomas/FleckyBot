using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Bot.Services;
using Bot.Models;
using Bot.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Bot;

public class BotService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly LoggingService _loggingService;
    private readonly CommandHandler _commandHandler;
    private readonly BotSettings _botSettings;
    private readonly InteractionHandler _interactionHandler;
    private IServiceScope? _scope;

    public BotService(IOptions<BotSettings> botSettings, IServiceProvider provider)
    {
        _provider = provider;
        _client = provider.GetRequiredService<DiscordSocketClient>();
        _interactionService = provider.GetRequiredService<InteractionService>();
        _loggingService = provider.GetRequiredService<LoggingService>();
        _commandHandler = provider.GetRequiredService<CommandHandler>();
        _interactionHandler = provider.GetRequiredService<InteractionHandler>();
        _botSettings = botSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

        Log.Information("[BotService] Setting up bot");

        try
        {
            //_provider.GetRequiredService<LoggingService>();

            _scope = _provider.CreateScope();

            await _commandHandler.InitialiseAsync(_scope);

            await _interactionService.AddModulesAsync(typeof(BotService).Assembly, _scope.ServiceProvider);

            stoppingToken.ThrowIfCancellationRequested();

            await _client.LoginAsync(TokenType.Bot, _botSettings.BotToken);
            await _client.StartAsync();
        }
        catch (Exception ex) when (ex is not TaskCanceledException)
        {
            try
            {
                await _client.LogoutAsync();
            }
            finally
            {
                _client?.Dispose();
                _scope?.Dispose();
            }

            Log.Error($"[BotService] An exception was triggered during set-up.\nError: {ex.Message}");

            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
        
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _client.Dispose();
        _scope?.Dispose();
        ((IDisposable)_interactionService).Dispose();
        
        base.Dispose();
    }
}
