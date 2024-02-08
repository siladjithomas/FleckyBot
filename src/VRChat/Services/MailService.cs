using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Logging;
using TsubaHaru.FleckyBot.VRChat.Models;

namespace TsubaHaru.FleckyBot.VRChat.Services;

public class MailService
{
    private readonly ILogger<MailService> _logger;
    private readonly MailSettings _settings;

    public MailService(ILogger<MailService> logger, MailSettings settings)
    {
        _logger = logger;
        _settings = settings;

        _logger.LogInformation("Set up Mail service...");
    }

    public async Task GetMail()
    {
        using var client = new ImapClient();

        await client.ConnectAsync(_settings.ImapHost, _settings.ImapPort, _settings.ImapUseSsl);
        await client.AuthenticateAsync(_settings.ImapUsername, _settings.ImapPassword);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly);

        _logger.LogInformation("Total messages: {messages}", inbox.Count);
        _logger.LogInformation("Recent messages: {messages}", inbox.Recent);

        for (int i = 0; i < inbox.Count; i++)
        {
            var message = await inbox.GetMessageAsync(i);
            _logger.LogInformation("Subject: {subject}", message.Subject);
        }

        await client.DisconnectAsync(true);
    }
}
