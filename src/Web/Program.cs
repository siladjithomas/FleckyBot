using Bot;
using Bot.Models;
using Bot.Services;
using Database.DatabaseContexts;
using Database.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Serilog;
using Victoria;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));
builder.Host.UseSystemd();

var botSettings = builder.Configuration.GetSection(nameof(BotSettings)).Get<BotSettings>();
builder.Services.AddSingleton(botSettings);

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

builder.Services.AddLavaNode(x => {
    x.SelfDeaf = true;
    x.Port = 2333;
    x.Hostname = "127.0.0.1";
    x.Authorization = "SomeSecurePassword";
});

builder.Services.AddSingleton<AudioService>();
builder.Services.AddSingleton(new DiscordSocketClient(discordSocketConfig));
builder.Services.AddSingleton(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<InteractionHandler>();

builder.Services.AddDbContext<ApplicationContext>(options => 
{
    if (builder.Configuration.GetSection("ConnectionStrings").GetValue<string>("IsSqlite") == "True")
        options.UseSqlite(builder.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("Bot"));
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("Bot"));
    
    //options.EnableSensitiveDataLogging(true);
});

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
