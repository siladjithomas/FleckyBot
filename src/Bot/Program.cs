using Bot;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Victoria;
using Quartz;
using TsubaHaru.FleckyBot.Bot;
using TsubaHaru.FleckyBot.Bot.Models;
using TsubaHaru.FleckyBot.Bot.Services;
using TsubaHaru.FleckyBot.Database.DatabaseContexts;
using TsubaHaru.FleckyBot.VRChat.Models;
using TsubaHaru.FleckyBot.VRChat.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration((hostContext) =>
    {
        hostContext.Sources.Clear();
        hostContext.AddJsonFile("appsettings.Development.json");
        hostContext.AddJsonFile("appsettings.BotConfig.json");
    })
    .ConfigureLogging((hostContext, logger) => 
    {
        logger.AddSerilog(new LoggerConfiguration()
            .ReadFrom.Configuration(hostContext.Configuration)
            .CreateLogger());
    })
    .ConfigureServices((hostContext, services) =>
    {
        BotSettings botSettings = hostContext.Configuration.GetSection(nameof(BotSettings)).Get<BotSettings>() ?? throw new KeyNotFoundException("Bot Settings in config file not found.");
        MailSettings mailSettings = hostContext.Configuration.GetSection(nameof(MailSettings)).Get<MailSettings>() ?? throw new KeyNotFoundException("Mail Settings in config file not found.");
        VRChatSettings vrChatSettings = hostContext.Configuration.GetSection(nameof(VRChatSettings)).Get<VRChatSettings>() ?? throw new KeyNotFoundException("VRChat Settings in config file not found.");
        
        services.AddSingleton(botSettings);
        services.AddSingleton(mailSettings);
        services.AddSingleton(vrChatSettings);

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
            //q.AddJobAndTrigger<CheckDMMessages>(hostContext.Configuration);
            q.AddJobAndTrigger<BotStatus>(hostContext.Configuration);
            q.AddJobAndTrigger<TimetableService>(hostContext.Configuration);
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
        services.AddSingleton<MailService>();
        services.AddSingleton<VRChatService>();
        services.AddSingleton<InteractionHandler>();

        services.AddDbContext<ApplicationContext>(options => 
        {
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("TsubaHaru.FleckyBot.Bot"));
            
            //options.EnableSensitiveDataLogging(true);
        });

        services.AddHostedService<Worker>();
    })
    .UseSystemd()
    .Build();

host.Run();
