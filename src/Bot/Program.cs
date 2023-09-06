using Bot;
using Bot.Models;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Services;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Victoria;
using Quartz;
using TelegramBot;
using TelegramBot.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((hostContext, logger) => 
    {
        logger.AddSerilog(new LoggerConfiguration()
            .ReadFrom.Configuration(hostContext.Configuration)
            .CreateLogger());
    })
    .ConfigureServices((hostContext, services) =>
    {
        var botSettings = hostContext.Configuration.GetSection(nameof(BotSettings)).Get<BotSettings>();

        services.AddSingleton(botSettings);

        var discordSocketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
            MessageCacheSize = botSettings.MessageCacheSize >= 0 ?
                botSettings.MessageCacheSize : throw new Exception($"{nameof(botSettings.MessageCacheSize)} must be set to a non negative integer."),
#if DEBUG
            LogLevel = LogSeverity.Debug
#else
            LogLevel = LogSeverity.Warning
#endif
        };

        services.AddLavaNode(x => 
        {
            x.SelfDeaf = true;
            x.Port = 2333;
            x.Hostname = "127.0.0.1";
            x.Authorization = "SomeSecurePassword";
        });

        services.AddQuartz(q => 
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            q.AddJobAndTrigger<BotStatus>(hostContext.Configuration);
            //q.AddJobAndTrigger<CheckDMMessages>(hostContext.Configuration);
        });

        services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });

        // setting up discord related stuff
        services.AddSingleton<AudioService>();
        services.AddSingleton(new DiscordSocketClient(discordSocketConfig));
        services.AddSingleton(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>()));
        services.AddSingleton<CommandHandler>();
        services.AddSingleton<InteractionHandler>();

        services.AddDbContext<ApplicationContext>(options => 
        {
            if (hostContext.Configuration.GetSection("ConnectionStrings").GetValue<string>("IsSqlite") == "True")
                options.UseSqlite(hostContext.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("Bot"));
            else
                options.UseSqlServer(hostContext.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("Bot"));
            
            //options.EnableSensitiveDataLogging(true);
        });

        services.AddHostedService<Worker>();

        // TODO: Set up so it uses the host settings from TelegramBot project
    })
    .UseSystemd()
    .Build();

host.Run();
