using TelegramBot.Services;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot;

public class TelegramWorker : BackgroundService
{
    private readonly ILogger<TelegramWorker> _logger;
    private readonly IServiceProvider _provider;
    private readonly TelegramBotClient _client;
    private readonly TelegramInteractionHandler _handler;
    private IServiceScope? _scope;
    
    public TelegramWorker(ILogger<TelegramWorker> logger, IServiceProvider provider, TelegramBotClient client, TelegramInteractionHandler handler)
    {
        _logger = logger;
        _provider = provider;
        _client = client;
        _handler = handler;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("de-at");
        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

        _logger.LogInformation("Setting up Telegram Bot for execution...");
        
        try
        {
            _scope = _provider.CreateScope();

            var me = await _client.GetMeAsync();

            _logger.LogDebug($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            await Task.CompletedTask;
        }
        catch (Exception ex) when (!(ex is TaskCanceledException))
        {
            _logger.LogError(ex, "An exception has been triggered during set-up. Shutting down...");

            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.CloseAsync();
    }

    public override void Dispose()
    {
        if (_scope != null)
            _scope.Dispose();
    }
}