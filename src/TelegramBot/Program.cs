using Telegram.Bot;
using TelegramBot;
using TelegramBot.Models;
using TelegramBot.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var telegramBotSettings = hostContext.Configuration.GetSection("Telegram").Get<TelegramBotSettings>();
        
        // setting up telegram related stuff
        services.AddSingleton(new TelegramBotClient(telegramBotSettings.AccessToken));
		services.AddSingleton<TelegramInteractionHandler>();
        
        services.AddHostedService<TelegramWorker>();
    })
    .Build();

host.Run();
