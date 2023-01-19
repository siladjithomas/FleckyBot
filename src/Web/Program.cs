using Bot;
using Bot.Models;
using Bot.Services;
using Database;
using Database.DatabaseContexts;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Victoria;

var builder = WebApplication.CreateBuilder(args);

// Setting up Systemd support on linux
builder.Host.UseSystemd();
// Setting up Serilog for extended logging
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// seperately getting config from appsettings.*.json and adding it as singleton
var botSettings = builder.Configuration.GetSection(nameof(BotSettings)).Get<BotSettings>();
builder.Services.AddSingleton(botSettings);

// preparing config for discord client
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

// Setting up lavalink node for music support
builder.Services.AddLavaNode(x =>
{
    x.SelfDeaf = true;
    x.Port = 2333;
    x.Hostname = "127.0.0.1";
    x.Authorization = "SomeSecurePassword";
});

// adding services that will be needed by the bot
builder.Services.AddSingleton<AudioService>();
builder.Services.AddSingleton(new DiscordSocketClient(discordSocketConfig));
builder.Services.AddSingleton(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<InteractionHandler>();

// setting up the database for the bot
builder.Services.AddDbContext<ApplicationContext>(options => 
{
    if (builder.Configuration.GetSection("ConnectionStrings").GetValue<string>("IsSqlite") == "True")
        options.UseSqlite(builder.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("Bot"));
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("Bot"));
    
    //options.EnableSensitiveDataLogging(true);
});

// adding the worker service as a hosted service
builder.Services.AddHostedService<Worker>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
