using Database.DatabaseContexts;
using Database.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TelegramBot.Models;

namespace TelegramBot.Services;

public class TelegramInteractionHandler
{
    private readonly TelegramBotClient _client;
    private readonly ILogger<TelegramInteractionHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

	public TelegramInteractionHandler(
		TelegramBotClient client,
		ILogger<TelegramInteractionHandler> logger,
		IServiceScopeFactory scopeFactory
	)
	{
		_client = client;
		_logger = logger;
		_scopeFactory = scopeFactory;

		ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };

        _client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions
        );
	}

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        

        var chatId = message.Chat.Id;
		var name = !string.IsNullOrEmpty(message.Chat.LastName) ? $"{message.Chat.FirstName} {message.Chat.LastName}" : message.Chat.FirstName;

        _logger.LogDebug($"Received a '{messageText}' message from '{name}' in chat {chatId}.");

        if (messageText.Contains("send me a sticker"))
        {
            await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: InputFile.FromUri("https://thumbs.dreamstime.com/b/red-paper-stickers-transparent-background-round-sticker-labels-stamp-vecror-eps-78760526.jpg")
            );

            return;
        }

        if (messageText.Contains("/quote"))
		{
			using (var scope = _scopeFactory.CreateScope())
			{
				ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
			
				// TODOSIL: get a quote from the database and post it as a message
				Random rand = new Random();
				int skipper = rand.Next(1, context.Quote?.Count() ?? 1);

				Quote? quote = context.Quote?.Skip(skipper).Take(1).First();
				
				if (quote == null)
					return;

				await botClient.SendTextMessageAsync(
					chatId: chatId,
					text: $"<i>{String.Join("%0A", quote.QuoteText)}</i> %0A <b>- { quote.QuoteAuthor }</b>",
                    parseMode: ParseMode.Html,
					cancellationToken: cancellationToken);

				return;
			}
		}

        if (messageText.Contains("/contact"))
        {
            await botClient.SendContactAsync(
                chatId: chatId,
                phoneNumber: "+436666677888",
                firstName: "Haru",
                lastName: "Tsuyabashi",
                replyToMessageId: message.MessageId,
                cancellationToken: cancellationToken
            );

            return;
        }

        // If no slash command has been run, log the conversation

        using (var scope = _scopeFactory.CreateScope())
        {
            ApplicationContext context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            TelegramUser? userInDB = await context.TelegramUser.Where(x => x.FirstName == message.Chat.FirstName).FirstOrDefaultAsync();

            if (userInDB == null)
            {
                TelegramUser newUser = new TelegramUser
                {
                    FirstName = message.Chat.FirstName,
                    LastName = message.Chat.LastName,
                    PhoneNumber = null,
                    UserId = 0
                };

                await context.TelegramUser.AddAsync(newUser);

                await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Added user {newUser.FirstName} in DB!",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);

                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"User {userInDB.FirstName} found!",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(ErrorMessage);
        return Task.CompletedTask;
    }
}