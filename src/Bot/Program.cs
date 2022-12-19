using Bot;
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
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
