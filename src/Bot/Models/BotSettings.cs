namespace TsubaHaru.FleckyBot.Bot.Models;

public class BotSettings
{
    public string BotToken { get; set; } = string.Empty;
    public string Prefix { get; set; } = "/";
    public int MessageCacheSize { get; set; }
    public bool AlwaysDownloadUsers { get; set; }
    public bool CaseSensitiveCommands { get; set; }
    public bool UseMentionPrefix { get; set; }
}