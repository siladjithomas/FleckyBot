using System;
using MimeKit;
using MailKit;
using MailKit.Search;
using MailKit.Net.Imap;
using Serilog;

namespace Bot.Services;

public class MailService
{
    private readonly ILogger<Worker> _logger;

    public MailService(ILogger<Worker> logger)
    {
        _logger = logger;

        _logger.LogInformation("Set up Mail service...");
    }

    public async Task GetMail()
    {
        using (var client = new ImapClient())
        {
            await client.ConnectAsync("tsuyabashi-dev.netcup-mail.de", 993, true);
            await client.AuthenticateAsync("nexus@tsuyabashi.dev", "NetCafeNexus2023");

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
}
