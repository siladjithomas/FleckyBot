using Bot;
using Serilog;
using Victoria;
using Bot.Services;
using Discord.Interactions;
using Discord.WebSocket;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => 
    {
        logging.AddSerilog();
    })
    .ConfigureServices(services => 
    {
        services.AddLavaNode(x => {
            x.SelfDeaf = true;
            x.Port = 2333;
            x.Hostname = "127.0.0.1";
            x.IsSsl = false;
            x.Authorization = "da-chef123";
        });

        services.AddSingleton<QuoteService>();
        services.AddSingleton<TicketService>();
        services.AddSingleton<GuildService>();
        services.AddSingleton<AudioService>();
        services.AddSingleton<ImageService>();
        services.AddSingleton<VoteService>();

        services.AddSingleton(Utility.GetDiscordSocketClient());
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<InteractionHandler>();
        services.AddSingleton<LoggingService>();
        
        services.AddHostedService<BotService>();
    })
    .Build();

await host.RunAsync();