using System;
using MimeKit;
using MailKit;
using MailKit.Search;
using MailKit.Net.Imap;
using Serilog;
using Bot.Models;

namespace Bot.Services;

public class MailService
{
    private readonly ILogger<Worker> _logger;
    private readonly MailSettings _settings;

    public MailService(ILogger<Worker> logger, MailSettings settings)
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
