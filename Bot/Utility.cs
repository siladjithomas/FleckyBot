using Bot.Models;
using System;
using Discord;
using Discord.WebSocket;

namespace Bot;

public class Utility
{
    public static DiscordSocketClient GetDiscordSocketClient()
    {
        return new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
//            AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
//            MessageCacheSize = botSettings.MessageCacheSize >= 0 ?
//                botSettings.MessageCacheSize :
//                throw new Exception($"{nameof(botSettings.MessageCacheSize)} must be set to a non negative integer."),
#if DEBUG
            LogLevel = LogSeverity.Debug
#else
            LogLevel = LogSeverity.Warning
#endif
        });
    }
}