using DiscordBot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<DiscordWorker>();
    })
    .Build();

host.Run();
