namespace TsubaHaru.FleckyBot.VRChat.Models
{
    public class MailSettings
    {
        public string? ImapHost { get; set; }
        public int ImapPort { get; set; }
        public bool ImapUseSsl { get; set; } = true;
        public string? ImapUsername { get; set; }
        public string? ImapPassword { get; set; }
    }
}
