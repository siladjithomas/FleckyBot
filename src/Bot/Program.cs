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
using Serilog.Events;

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
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages,
            AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
            MessageCacheSize = botSettings.MessageCacheSize >= 0 ?
                botSettings.MessageCacheSize : throw new Exception($"{nameof(botSettings.MessageCacheSize)} must be set to a non negative integer."),
#if DEBUG
            LogLevel = LogSeverity.Debug
#else
            LogLevel = LogSeverity.Warning
#endif
        };

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
            
            options.EnableSensitiveDataLogging(true);
        });

        services.AddHostedService<Worker>();

        services.AddScoped<RequestableRoleManager>();
    })
    .Build();

host.Run();
