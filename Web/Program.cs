using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.AspNetCore.HttpOverrides;
using Web.Models;
using Web.Services;
using Bot;
using Bot.Services;
using Bot.Models;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Discord.OAuth2;
using Victoria;
using Serilog;
using Serilog.Events;
using System.IO;
using DinkToPdf;
using DinkToPdf.Contracts;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Set up Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Set up database connection via MongoDB Client
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
builder.Services.Configure<BotSettings>(builder.Configuration.GetSection(nameof(BotSettings)));
builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection(nameof(AuthenticationSettings)));

// As this will run on linux, use the Systemd integration
builder.Host.UseSystemd();

// Set up Victoria for Discord
builder.Services.AddLavaNode<XLavaPlayer>(x => {
    x.SelfDeaf = true;
    x.Port = 2333;
    x.Hostname = "127.0.0.1";
    x.IsSsl = false;
    x.Authorization = "SecurePasswordHere";
});

builder.Services.AddSingleton<UserService>();

// Set up Discord Authentication
builder.Services.AddAuthentication( x => 
    {
        x.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        x.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = DiscordDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddDiscord( x => 
    {
        x.AppId = "";
        x.AppSecret = "";
        x.Scope.Add("guilds");

        //Required for accessing the oauth2 token in order to make requests on the user's behalf, ie. accessing the user's guild list
        x.SaveTokens = true;
    });

builder.Services.AddSingleton<QuoteService>();
builder.Services.AddSingleton<TicketService>();
builder.Services.AddSingleton<GuildService>();
builder.Services.AddSingleton<AudioService>();
builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton<VoteService>();
builder.Services.AddSingleton<TemplateGenerator>();

// Discord bot related dependency injections
builder.Services.AddSingleton(Utility.GetDiscordSocketClient());
builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<InteractionHandler>();
builder.Services.AddSingleton<LoggingService>();

builder.Services.AddHostedService<BotService>();

/*
builder.Services.AddQuartz(q => 
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    var jobKey = new JobKey("StatusJob");
    q.AddJob<Bot.Models.StatusJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("StatusJob-Trigger")
        .WithCronSchedule("15 0/6 * * * ?"));
});
*/

//builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

// Comment it on production because then we don't need it
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseRouting();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();

app.MapBlazorHub();
app.MapDefaultControllerRoute();
app.MapFallbackToPage("/_Host");

app.Run();
